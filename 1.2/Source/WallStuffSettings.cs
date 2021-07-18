using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace WallStuff
{
    class WallStuffSettings : ModSettings
    {
        internal static List<ThingCountExposable> listOfSpawnableThings = null;
        internal static float heaterPower;
        internal static float coolerPower;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref listOfSpawnableThings, "listOfSpawnableThings");
            Scribe_Values.Look(ref heaterPower, "heaterPower");
            Scribe_Values.Look(ref coolerPower, "coolerPower");

            // Remove all null entries in the oreDictionary
            // This is most likely due to removing a mod, which will trigger a game reset
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<ThingCountExposable> dict = new List<ThingCountExposable>();
                bool warning = false;
                for (int i = 0; i < listOfSpawnableThings.Count; i++)
                {
                    if (listOfSpawnableThings[i] != null)
                    {
                        dict.Add(new ThingCountExposable(listOfSpawnableThings[i].thingDef, listOfSpawnableThings[i].count));
                    }
                    else if (!warning)
                    {
                        warning = true;
                    }
                }
                listOfSpawnableThings = dict;
                if (heaterPower == 0f)
                {
                    heaterPower = 21f;
                }
                if (coolerPower == 0f)
                {
                    coolerPower = -15f;
                }
            }
        }
    }
}
