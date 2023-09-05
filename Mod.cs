using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace TheisenExampleNS
{
    public class TheisenExample : Mod
    {
        public override void Ready()
        {
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.CookingIdea, "TheisenExample_blueprint_golden_berry", 1);
            Logger.Log("Ready!");
        }
    }
}