using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using System.Linq;
using System;

public class AgentData
{
    public SimAgent agent;
    public SimAgentBehaviorPattern Pattern;
    public Dictionary<string, uint> Resources;
    public uint Capital;
    public List<Offer> Offers;
    public Controller.Recipe recipe;
    //public uint TechLevel;
    //public uint TechCost;
    public uint ExpansionLevel;
    //public uint ExpansionCost;
    public uint Networking;
    public uint NetworkingCost;
    public uint NetworkingRemaining;
    public List<Offer> OfferScope;

    public AgentData() 
    { 
        Resources = new Dictionary<string, uint>();
        Offers = new List<Offer>();
        OfferScope = new List<Offer>();
    }

    public void Tick()
    {
        //Search
        NetworkingRemaining = Networking + Controller.instance.NetworkingBase;
        Dictionary<string, float> resourceDemand = new Dictionary<string, float>();
        float DemandSum = 0;
        for (int i = 0; i < recipe.recipe.recipeInput.Length; i++)
        {
            resourceDemand[recipe.recipe.recipeInput[i].name] = 5 * recipe.recipe.recipeInput[i].count - (Resources.ContainsKey(recipe.recipe.recipeInput[i].name) ? Resources[recipe.recipe.recipeInput[i].name] : 0);
            DemandSum += resourceDemand[recipe.recipe.recipeInput[i].name];
        }

        for (int i = 0; i < recipe.recipe.recipeInput.Length; i++)
        {
            agent.FillScopeByOfferParameters((uint)(resourceDemand[recipe.recipe.recipeInput[i].name]/ DemandSum * NetworkingRemaining), recipe.recipe.recipeInput[i].type);
        }
        //Purchase
        var sortedscope = agent.CurrentScope.OrderBy(x => x.Cost / x.Resource.count);

        foreach (var b in sortedscope)
        {
            int i = Array.IndexOf(recipe.recipe.recipeInput, recipe.recipe.recipeInput.First(x => x.name == b.Resource.name));
            if (Capital > b.Cost && !Resources.ContainsKey(b.Resource.name) || Resources[b.Resource.name] / (recipe.recipe.recipeInput.First(x => x.name == b.Resource.name).count * recipe.InputMult[i]) < 5)
            {
                b.Buy(this);
            }
        }
        //Production
        uint batches = recipe.CanProduce(this);
        recipe.Produce(this, batches);
        //Selling
        float recipecost = recipe.recipe.recipeCost * recipe.CostMult;
        bool willpost = true;
        for (int i = 0; i < recipe.recipe.recipeInput.Length; i++)
        {
            uint cnt = 0;
            uint cost = 0;
            foreach (var d in agent.CurrentScope.Where(x=>x.Resource.name == recipe.recipe.recipeInput[i].name))
            {
                cnt += d.Resource.count;
                cost += d.Cost;
            }
            if (cnt == 0) { willpost = false; break; }
            recipecost += cost / cnt * recipe.recipe.recipeInput[i].count;
        }
        if (willpost)
        {
            foreach (var e in recipe.recipe.recipeOutput)
            {
                if (Resources.ContainsKey(e.name) && Resources[e.name] > 0)
                {
                    uint resdiv = Resources[e.name] / (Networking + Controller.instance.NetworkingBase) * 2;
                    if (resdiv > 0)
                    {
                        for (int i = 0; i < (Networking + Controller.instance.NetworkingBase) / 2; i++)
                        {
                            if (Offers.Count() >= Networking + Controller.instance.NetworkingBase)
                            {
                                Offer old = Offers[0];
                                foreach (var d in Offers)
                                {
                                    if (old.Creation > d.Creation)
                                        old = d;
                                }
                                Debug.Log(old.Recall(this)?"Recalled":"Could not recall offer");
                            }
                            Controller.Resource resource = new Controller.Resource(e.type, resdiv);
                            Offer attemptedoffer = new Offer(this, resource, (uint)(recipecost / recipe.recipe.recipeOutput.Select(x=>(int)x.count).Sum() * 1.4 * resource.count));
                            if (!attemptedoffer.Publish())
                            {
                                Debug.Log(agent.name + " failed to publish an offer " + e.name + " " + attemptedoffer.Resource.count + "/" + Resources[e.name] + " " + attemptedoffer.Cost);
                            }
                        }
                    }
                }
            }
        }
        //Pattern.Tick(agent);
        OfferScope.Clear();
        agent.PostTick();
    }

    public void OnOfferBuy(Offer Offer)
    {
        //Pattern.OnOfferBuy(agent, Offer);
    }

    public void OnOfferBought(Offer Offer, uint buyer)
    {
        //Pattern.OnOfferBought(agent, Offer, buyer);
    }
}

public class SimAgent : MonoBehaviour
{
    public TMPro.TMP_Text CapitalText;
    public SpriteRenderer render;
    private AgentData agentData;
    public uint AgentId { get; private set; }
    public ReadOnlyDictionary<string, uint> Resources { get { return new ReadOnlyDictionary<string, uint>(agentData.Resources); } }
    public IList<Offer> Offers { get { return agentData.Offers.AsReadOnlyList(); } }
    public List<Offer> CurrentScope { get { return agentData.OfferScope; } }

    public uint Capital { get { return agentData.Capital; } }
    // Start is called before the first frame update
    public AgentData init(string name, SimAgentBehaviorPattern pattern, Controller.baseRecipe recipe, uint startingCapital, uint Networking, uint ExpansionLevel, Color color, uint id)
    {
        agentData = new AgentData();
        agentData.agent = this;
        this.name = name;
        this.AgentId = id;
        agentData.Pattern = pattern;
        agentData.Capital = startingCapital;
        agentData.recipe = new Controller.Recipe(recipe);
        agentData.Networking = Networking;
        agentData.ExpansionLevel = ExpansionLevel;
        render.color = color;

        CapitalText.text = Capital.ToString();
        return agentData;
    }

    public void PostTick()
    {
        CapitalText.text = Capital.ToString();
    }

    public bool CreateOffer(Controller.Resource resource, uint cost)
    {
        Offer Offer = new Offer(agentData, resource, cost);
        return Offer.Publish();
    }

    public uint FillScopeByOfferParameters(uint cnt, Controller.ResourceType resource)
    {
        return Controller.instance.FillScopeByOffer(this, cnt, resource);
    }
    public uint FillScopeByOffer(Controller.ResourceType resource)
    {
        return Controller.instance.FillScopeByOffer(this, resource);
    }

    void Awake()
    {
    }

    private void FixedUpdate()
    {
        var c = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        if (c.Length > 1)
        {
            var a = (c.Select(x => transform.position - x.transform.position).Aggregate((x, y) => x + y));
            if (a.magnitude == 0) a = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0);
            transform.position += a * 0.1f;
        }
    }
    public void OnMouseEnter()
    {
        Controller.instance.DisplayAgentData(this.agentData);
    }

    public void OnMouseExit()
    {
        Controller.instance.ClearDisplay();
    }
}
