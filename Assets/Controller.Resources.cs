using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.VisualScripting;
using System.Security.Cryptography;
using Unity.Mathematics;

public partial class Controller : MonoBehaviour
{
    public float minCostMult { get; private set; }
    public float maxCostMult { get; private set; }
    public float minIncomeMult { get; private set; }
    public float maxIncomeMult { get; private set; }
    public float minInputMult { get; private set; }
    public float maxInputMult { get; private set; }
    public float minOutputMult { get; private set; }
    public float maxOutputMult { get; private set; }
    public uint NetworkingBase { get; private set; }

    public List<ResourceType> Resources;
    public Mosframe.DynamicVScrollView scrollView;
    public TMPro.TMP_InputField newResourceNameField;
    [Obsolete("Belongs to the 4th revision")]
    public TMPro.TMP_InputField newResourceCountField;
    public UnityEngine.UI.Button NewResorceButton;

    public void AddResource()
    {
        NewResorceButton.interactable = false;
        Resources.Add(new ResourceType(newResourceNameField.text));
        scrollView.totalItemCount++;
    }

    public void CheckButton()
    {
        NewResorceButton.interactable = newResourceNameField.text.Length > 0 && !Resources.Any(a => a.name == newResourceNameField.text);
    }



    public class ResourceType
    {
        public string name;

        public ResourceType(string name)
        {
            this.name = name;
        }
    }
    public struct Resource
    {
        public ResourceType type;
        public string name { get { return type.name; } }
        public uint count;

        public Resource(ResourceType type, uint count)
        {
            this.type = type;
            this.count = count;
        }
    }

    public class baseRecipe
    {
        public baseRecipe(string recipeName, Resource[] recipeInput, Resource[] recipeOutput,uint recipeCost, uint RecipeIncome)
        {
            this.recipeName = recipeName;
            this.recipeInput = recipeInput;
            this.recipeOutput = recipeOutput;
            this.recipeCost = recipeCost;
            this.recipeIncome = recipeIncome;
        }
        public string recipeName { get; private set; }
        public Resource[] recipeInput { get; private set; }
        public Resource[] recipeOutput { get; private set; }
        public uint recipeCost { get; private set; }
        public uint recipeIncome { get; private set; }

    }

    public class Recipe
    {
        public baseRecipe recipe { get; private set; }
        public float CostMult { get; private set; }
        public float IncomeMult { get; private set; }
        public float[] InputMult { get; private set; }
        public float[] OutputMult { get; private set; }
        public uint RerollCost { get; private set; }
        public Recipe(baseRecipe recipe)
        {
            this.recipe = recipe;
            InputMult = new float[recipe.recipeInput.Length];
            OutputMult = new float[recipe.recipeOutput.Length];
            Reroll();
        }
        public void Reroll()
        {
            CostMult = UnityEngine.Random.Range(Controller.instance.minCostMult, Controller.instance.maxCostMult);
            IncomeMult = UnityEngine.Random.Range(Controller.instance.minIncomeMult, Controller.instance.maxIncomeMult);
            for (int i = 0; i < recipe.recipeInput.Length; i++)
                InputMult[i] = UnityEngine.Random.Range(Controller.instance.minInputMult, Controller.instance.maxInputMult);
            for (int i = 0; i < recipe.recipeOutput.Length; i++)
                OutputMult[i] = UnityEngine.Random.Range(Controller.instance.minOutputMult, Controller.instance.maxOutputMult);
        }

        public uint CanProduce(AgentData agent)
        {
            uint possiblecount = agent.ExpansionLevel;
            if (recipe.recipeCost > 0)
                possiblecount = Math.Min(possiblecount, (agent.Capital / (uint)math.ceil(recipe.recipeCost * CostMult)));
            for (int i = 0;i < recipe.recipeInput.Length;i++)
            {
                if (possiblecount == 0 || !agent.Resources.ContainsKey(recipe.recipeInput[i].name)) return 0;
                possiblecount = Math.Min(possiblecount, agent.Resources[recipe.recipeInput[i].name]/(uint)math.ceil(recipe.recipeInput[i].count * InputMult[i]));
            }
            return possiblecount;
        }

        public void Produce(AgentData agent, uint batchcount)
        {
            uint truebatchcount = math.min(batchcount, CanProduce(agent));
            if (truebatchcount > 0)
            {
                agent.Capital -= (uint)math.round(recipe.recipeCost * CostMult * truebatchcount);
                agent.Capital += (uint)math.round(recipe.recipeIncome * IncomeMult * truebatchcount);
                for (int i = 0; i < recipe.recipeInput.Length; i++)
                {
                    agent.Resources[recipe.recipeInput[i].name] -= (uint)math.round(recipe.recipeInput[i].count * InputMult[i]);
                }

                for (int i = 0; i < recipe.recipeOutput.Length; i++)
                {
                    if (agent.Resources.ContainsKey(recipe.recipeOutput[i].name))
                        agent.Resources[recipe.recipeOutput[i].name] += (uint)math.round(recipe.recipeOutput[i].count * OutputMult[i]);
                    else
                        agent.Resources[recipe.recipeOutput[i].name] = (uint)math.round(recipe.recipeOutput[i].count * OutputMult[i]);
                }
            }
        }
    }
}
