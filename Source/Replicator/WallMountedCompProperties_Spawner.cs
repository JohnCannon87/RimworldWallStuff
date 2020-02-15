﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace WallStuff
{
    class WallMountedCompProperties_Spawner : CompProperties
    {
        public ThingDef thingToSpawn;

        public IntRange spawnIntervalRange = new IntRange(100, 100);

        public int spawnMaxAdjacent = -1;

        public bool spawnForbidden;

        public bool requiresPower;

        public bool writeTimeLeftToSpawn;

        public bool showMessageIfOwned;

        public string saveKeysPrefix;

        public bool inheritFaction;

        public WallMountedCompProperties_Spawner()
        {
            this.compClass = typeof(WallMountedCompSpawner);
        }
    }
}
