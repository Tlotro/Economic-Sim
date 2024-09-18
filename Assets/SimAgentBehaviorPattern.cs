using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimAgentBehaviorPattern
{
    /// <summary>
    /// this is triggered every time an offer published by this agent is fullfilled, you can monitor, who does what and plan accordingly.
    /// </summary>
    /// <param name="Self"></param>
    /// <param name="Offer"></param>
    /// <param name="Buyer"></param>
    public abstract void OnOfferBought(SimAgent Self, Offer Offer, uint Buyer);
    /// <summary>
    /// this is triggered every time this agent buys an offer, you can monitor, who does what and plan accordingly.
    /// </summary>
    /// <param name="Self"></param>
    /// <param name="Offer"></param>
    public abstract void OnOfferBuy(SimAgent Self, Offer Offer);
    /// <summary>
    /// Activates every cycle. This is where your main things like scheduling production, submitting and checkig transactions
    /// </summary>
    public abstract void Tick(SimAgent Self);
}
