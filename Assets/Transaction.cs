using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

public class Offer
{
    private AgentData agent;
    public bool open {  get; private set; }
    public uint agentID { get { return agent.agent.AgentId; } }
    public Controller.Resource Resource { get; private set; }
    public uint Creation { get; private set; }
    public uint Cost { get; private set; }
    public bool CanFulfill(AgentData buyer) => buyer != agent && buyer.Capital > Cost;
    public bool CanPublish() => agent.Resources[Resource.name] > Resource.count;

    public bool Buy(AgentData buyer)
    {
        if (buyer != agent && buyer.Capital > Cost)
        {
            if (buyer.Resources.Keys.Contains(Resource.name))
                buyer.Resources[Resource.name] += Resource.count;
            else
                buyer.Resources[Resource.name] = Resource.count;
            agent.Capital += Cost;
            buyer.Capital -= Cost;
            buyer.OnOfferBuy(this);
            agent.OnOfferBought(this,buyer.agent.AgentId);
            agent.Offers.Remove(this);
            Controller.instance.CloseOffer(this);
            open = false;
            return true;
        }
        return false;
    }

    public Offer(AgentData agent, Controller.Resource resource, uint cost)
    {
        this.agent = agent;
        this.Resource = resource;
        Cost = cost;
    }

    public bool Publish()
    {
        if (Controller.instance.OpenOffer(this))
        {
            Creation = Controller.instance.currentTick;
            open = true;
            return true;
        }
        return false;
    }

    public bool Recall(AgentData publisher)
    {
        if (publisher == agent && Controller.instance.CloseOffer(this))
        {
            open = false;
            return true;
        }
        return false;
    }
}
