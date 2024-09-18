using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public partial class Controller : MonoBehaviour
{
    public GameObject SimAgentPrefab;
    public GameObject SimAgentsFolder;
    public UnityEngine.UI.Button StepButton;
    public UnityEngine.UI.Button ResumeButton;
    public UnityEngine.UI.Button PauseButton;
    public TMPro.TMP_InputField SimSpeedText;
    public TMPro.TMP_Text AgentDataText;
    public static Controller instance;
    public bool paused { get; private set; }
    /// <summary>
    /// the speed of the sim in ops/second
    /// </summary>
    public float simspeed { get { return _simspeed; } set { _simspeed = value; if (!paused) { Pause(); Unpause(); } } }
    private float _simspeed;
    private Dictionary<SimAgent, AgentData> ADbyAgent;
    private IList<SimAgent> agents { get { return ADbyAgent.Keys.AsReadOnlyList(); } }
    private Dictionary<uint, AgentData> ADbyID;
    private List<baseRecipe> recipes;
    private List<uint> recipeAgentCount;
    public uint MinStartCapital { get; private set; }
    public uint MaxStartCapital { get; private set; }
    public uint MinNetworking { get; private set; }
    public uint MaxNetworking { get; private set; }
    public uint MinBatchCount { get; private set; }
    public uint MaxBatchCount { get; private set; }
    public uint currentTick { get; private set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        paused = true;
        simspeed = 1;
        ResumeButton.gameObject.SetActive(true);
        PauseButton.gameObject.SetActive(false);
        StepButton.interactable = true;
        OpenOffers = new Dictionary<ResourceType, List<Offer>>(); 
        Resources = new List<ResourceType>();
        recipes = new List<baseRecipe>();
        ADbyID = new Dictionary<uint, AgentData>();
        ADbyAgent = new Dictionary<SimAgent, AgentData>();
        recipeAgentCount = new List<uint>();
        MinStartCapital = 1000;
        MaxStartCapital = 2000;
        minCostMult = 0.9f;
        maxCostMult = 1.1f;
        minIncomeMult = 0.9f;
        maxIncomeMult = 1.1f;
        minInputMult = 0.9f;
        maxInputMult = 1.1f;
        minOutputMult = 0.9f;
        maxOutputMult = 1.1f;
        MinNetworking = 4;
        MaxNetworking = 8;
        MinBatchCount = 2;
        MaxBatchCount = 4;
        Resources.Add(new ResourceType("Raw Iron"));
        recipes.Add(new baseRecipe("Raw Iron mining", new Resource[]{ },new Resource[] { new Resource(Resources[0], 10) },10,0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Coal"));
        recipes.Add(new baseRecipe("Coal mining", new Resource[] { }, new Resource[] { new Resource(Resources[1], 10) }, 8, 0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Coke"));
        recipes.Add(new baseRecipe("Coal Coking", new Resource[] { new Resource(Resources[1],10) }, new Resource[] { new Resource(Resources[2],10) }, 1, 0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Iron"));
        recipes.Add(new baseRecipe("Iron smelting", new Resource[] { new Resource(Resources[0], 10) }, new Resource[] { new Resource(Resources[3], 9) }, 2, 0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Steel"));
        recipes.Add(new baseRecipe("Steelmaking", new Resource[] { new Resource(Resources[2], 10), new Resource(Resources[3], 6) }, new Resource[] { new Resource(Resources[4], 10) }, 5, 0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Steel screw"));
        recipes.Add(new baseRecipe("Screwmaking", new Resource[] { new Resource(Resources[4], 1)}, new Resource[] { new Resource(Resources[5], 40) }, 3, 0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Wood log"));
        recipes.Add(new baseRecipe("Woodcutting", new Resource[] { }, new Resource[] { new Resource(Resources[6], 10) }, 6, 0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Wood plank"));
        recipes.Add(new baseRecipe("Wood", new Resource[] { new Resource(Resources[6], 1) }, new Resource[] { new Resource(Resources[7], 5) }, 3, 0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Chair"));
        recipes.Add(new baseRecipe("Carpentry: Chairs", new Resource[] { new Resource(Resources[7], 3), new Resource(Resources[5], 16) }, new Resource[] { new Resource(Resources[8], 1) }, 3, 0));
        recipeAgentCount.Add(6);
        Resources.Add(new ResourceType("Table"));
        recipes.Add(new baseRecipe("Carpentry: Tables", new Resource[] { new Resource(Resources[7], 8), new Resource(Resources[5], 24) }, new Resource[] { new Resource(Resources[9], 1) }, 3, 0));
        recipeAgentCount.Add(6);
        recipes.Add(new baseRecipe("Chair sellig", new Resource[] { new Resource(Resources[7], 1) }, new Resource[] { }, 0, 20));
        recipeAgentCount.Add(6);
        recipes.Add(new baseRecipe("Table sellig", new Resource[] { new Resource(Resources[8], 1) }, new Resource[] { }, 0, 40));
        recipeAgentCount.Add(6);
        Setup();
    }

    public void Setup()
    {
        uint id = 0;
        if (recipes.Count != recipeAgentCount.Count)
            Debug.LogWarning("Count of agent counts must be equal to the count of recipes, this warning only makes sense in testing");
        for (int j = 0; j < recipes.Count; j++)
        {
            GameObject folder = Instantiate(new GameObject(recipes[j].recipeName), SimAgentsFolder.transform);
            for (int i = 0; i < recipeAgentCount[j]; i++)
            {
                GameObject agent = Instantiate(SimAgentPrefab, folder.transform);
                SimAgent simAgent = agent.GetComponent<SimAgent>();
                ADbyAgent[simAgent] = simAgent.init(recipes[j].recipeName + " " + i, null, recipes[j], (uint)UnityEngine.Random.Range(MinStartCapital,MaxStartCapital), (uint)UnityEngine.Random.Range(MinNetworking, MaxNetworking), (uint)UnityEngine.Random.Range(MinBatchCount, MaxBatchCount), Color.HSVToRGB((float)j/recipes.Count,1,1),id);
                id++;
                ADbyID[ADbyAgent[simAgent].agent.AgentId] = ADbyAgent[simAgent];
            }
        }
    }

    public void Pause()
    {
        Debug.Log("Paused");
        paused = true;
        CancelInvoke("Step");
        ResumeButton.gameObject.SetActive(true);
        PauseButton.gameObject.SetActive(false);
        StepButton.interactable = true;
    }

    public void Unpause()
    {
        paused = false;
        InvokeRepeating("Step", 1f / simspeed, 1f / simspeed);
        ResumeButton.gameObject.SetActive(false);
        PauseButton.gameObject.SetActive(true);
        StepButton.interactable = false;
    }
    public void UpdateSimSpeed(string str)
    {
        if (str.All(x => Char.IsDigit(x)))
            simspeed = int.Parse(str);
    }

    public void Step()
    {
        currentTick++;
        foreach (AgentData agent in ADbyAgent.Values) { agent.Tick(); }
        foreach (var a in OpenOffers)
        {
            //Debug.Log(a.Key.name + " " + a.Value.Count);
        }
        Debug.LogWarning("Tick " + currentTick);
    }

    public uint FillScopeByOffer(SimAgent self, uint cnt, ResourceType resource)
    {
        AgentData agent = ADbyAgent[self];
        if (!Controller.instance.OpenOffers.ContainsKey(resource))
            return 0;
        var tmp = Controller.instance.OpenOffers[resource];
        cnt = (uint)Mathf.Min(cnt, tmp.Count(), agent.NetworkingRemaining+NetworkingBase);
        uint cnt2 = cnt;
        while (cnt > 0)
        {
            int i = UnityEngine.Random.Range(0, tmp.Count());
            agent.OfferScope.Add(tmp[i]);
            tmp.RemoveAt(i);
            cnt--;
            agent.NetworkingRemaining--;
        }

        return cnt2;
    }
    public uint FillScopeByOffer(SimAgent self, ResourceType resource)
    {
        AgentData agent = ADbyAgent[self];
        var tmp = Controller.instance.OpenOffers[resource];
        uint cnt = (uint)Mathf.Min(tmp.Count(), agent.NetworkingRemaining+NetworkingBase);
        uint cnt2 = cnt;
        while (cnt > 0)
        {
            int i = UnityEngine.Random.Range(0, tmp.Count());
            agent.OfferScope.Add(tmp[i]);
            tmp.RemoveAt(i);
            cnt--;
            agent.NetworkingRemaining--;
        }

        return cnt2;
    }

    public void DisplayAgentData(AgentData agent)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(agent.agent.name + "\n");
        sb.Append("Recipe name: " + agent.recipe.recipe.recipeName + "\n");
        if (agent.recipe.recipe.recipeCost * agent.recipe.CostMult != 0)
            sb.Append("Cost: " + agent.recipe.recipe.recipeCost*agent.recipe.CostMult + "\n");
        if (agent.recipe.recipe.recipeInput.Length != 0)
        {
            sb.Append("Inputs: ");
            for (int i = 0; i < agent.recipe.recipe.recipeInput.Length; i++)
                sb.Append(agent.recipe.recipe.recipeInput[i].name + " " + agent.recipe.recipe.recipeInput[i].count * agent.recipe.InputMult[i] + " ");
            sb.Append("\n");
        }

        if (agent.recipe.recipe.recipeIncome * agent.recipe.IncomeMult != 0)
            sb.Append("Income: " + agent.recipe.recipe.recipeIncome * agent.recipe.IncomeMult + "\n");
        if (agent.recipe.recipe.recipeOutput.Length != 0)
        {
            sb.Append("Outputs: ");
            for (int i = 0; i < agent.recipe.recipe.recipeOutput.Length; i++)
                sb.Append(agent.recipe.recipe.recipeOutput[i].name + " " + agent.recipe.recipe.recipeOutput[i].count * agent.recipe.OutputMult[i] + " ");
            sb.Append("\n");
        }

        sb.Append("Stock: ");
        foreach (var a in agent.Resources)
            sb.Append(a.Key + " " + a.Value + " ");
        sb.Append("\n");
        if (agent.Offers.Count > 0)
        {
            sb.Append("Offers: ");
            foreach (var a in agent.Offers)
                sb.Append(a.Resource.name + " " + a.Resource.count + " | " + a.Cost + "\n");
        }
        AgentDataText.text = sb.ToString();
    }

    public void ClearDisplay()
    {
        AgentDataText.text = "";
    }

    public bool AgentExists(uint agentID) => ADbyID.ContainsKey(agentID);
}
