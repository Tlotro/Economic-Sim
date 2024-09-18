using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Controller
{

    private Dictionary<ResourceType,List<Offer>> OpenOffers;
    // Start is called before the first frame updat

    public bool OpenOffer(Offer Offer)
    {
        if (Offer.Resource.count > 0 && ADbyID[Offer.agentID].Resources[Offer.Resource.name] >= Offer.Resource.count)
        {
            ADbyID[Offer.agentID].Resources[Offer.Resource.name] -= Offer.Resource.count;
            ADbyID[Offer.agentID].Offers.Add(Offer);
            if (OpenOffers.ContainsKey(Offer.Resource.type))
                OpenOffers[Offer.Resource.type].Add(Offer);
            else
            {
                OpenOffers[Offer.Resource.type] = new List<Offer>();
                OpenOffers[Offer.Resource.type].Add(Offer);
            }
            return true;
        }
        return false;
    }

    public bool CloseOffer(Offer Offer)
    {
         if (ADbyID[Offer.agentID].Resources.ContainsKey(Offer.Resource.name))
             ADbyID[Offer.agentID].Resources[Offer.Resource.name] += Offer.Resource.count;
         else
             ADbyID[Offer.agentID].Resources[Offer.Resource.name] = Offer.Resource.count;

         ADbyID[Offer.agentID].Offers.Remove(Offer);
         OpenOffers[Offer.Resource.type].Remove(Offer);
         return true;
    }
}
