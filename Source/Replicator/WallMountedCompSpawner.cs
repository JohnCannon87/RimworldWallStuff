﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Diagnostics;
using UnityEngine;

namespace WallStuff
{
    public class WallMountedCompSpawner : ThingComp
    {
        private int ticksUntilSpawn;
        private int thingToSpawnIndex = 0;
        private Boolean spawned = false;
        private ThingCountExposable thingToSpawn;

        WallMountedCompProperties_Spawner WallMountedPropsSpawner
        {
            get
            {                
                return (WallMountedCompProperties_Spawner)this.props;
            }
        }

        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                this.ResetCountdown();
                this.thingToSpawn = WallStuffSettings.listOfSpawnableThings[thingToSpawnIndex];
            }
            spawned = true;
        }

        public override void CompTick()
        {
            this.TickInterval(1);
        }

        public override void CompTickRare()
        {
            this.TickInterval(250);
        }

        private void TickInterval(int interval)
        {
            if (!this.parent.Spawned)
            {
                return;
            }
            Hive hive = this.parent as Hive;
            if (hive != null)
            {
                if (!hive.active)
                {
                    return;
                }
            }
            else if (this.parent.Position.Fogged(this.parent.Map))
            {
                return;
            }
            if (this.WallMountedPropsSpawner.requiresPower && !this.PowerOn)
            {
                return;
            }
            this.ticksUntilSpawn -= interval;
            this.CheckShouldSpawn();
        }

        private void CheckShouldSpawn()
        {
            if (this.ticksUntilSpawn <= 0)
            {
                this.TryDoSpawn();
                this.ResetCountdown();
            }
        }

        public bool TryDoSpawn()
        {
            //jcLog.Warning("TryDoSpawn");
            if (!this.parent.Spawned)
            {
                //jcLog.Warning("TryDoSpawn - 1");
                return false;
            }
            //jcLog.Warning("TryDoSpawn - 2");
            if (this.WallMountedPropsSpawner.spawnMaxAdjacent >= 0)
            {
                //jcLog.Warning("TryDoSpawn - 3");
                int num = 0;
                for (int i = 0; i < 9; i++)
                {
                    //jcLog.Warning("TryDoSpawn - For Loop: " + i);
                    IntVec3 c = this.parent.Position + GenAdj.AdjacentCellsAndInside[i];
                    if (c.InBounds(this.parent.Map))
                    {
                        //jcLog.Warning("TryDoSpawn - InBounds");
                        List<Thing> thingList = c.GetThingList(this.parent.Map);
                        for (int j = 0; j < thingList.Count; j++)
                        {
                            //jcLog.Warning("TryDoSpawn - For Loop 2: " + j);
                            if (thingList[j].def == this.thingToSpawn.thingDef)
                            {
                                //jcLog.Warning("TryDoSpawn - 4");
                                num += thingList[j].stackCount;
                                if (num >= this.WallMountedPropsSpawner.spawnMaxAdjacent)
                                {
                                    //jcLog.Warning("TryDoSpawn - 5");
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            //jcLog.Warning("TryDoSpawn - 6");
            IntVec3 center;
            if (this.TryFindSpawnCell(out center))
            {
                //jcLog.Warning("TryDoSpawn - TryFindSpawnCell");
                Thing thing = ThingMaker.MakeThing(this.thingToSpawn.thingDef, null);
                thing.stackCount = this.thingToSpawn.count;
                if (this.WallMountedPropsSpawner.inheritFaction && thing.Faction != this.parent.Faction)
                {
                    thing.SetFaction(this.parent.Faction, null);
                }
                Thing t;
                //jcLog.Warning("TryDoSpawn - TryPlaceThing");
                GenPlace.TryPlaceThing(thing, center, this.parent.Map, ThingPlaceMode.Direct, out t, null, null);
                //jcLog.Warning("TryDoSpawn - Placed Thing ??");
                if (this.WallMountedPropsSpawner.spawnForbidden)
                {
                    t.SetForbidden(true, true);
                }
                if (this.WallMountedPropsSpawner.showMessageIfOwned && this.parent.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(this.thingToSpawn.thingDef.LabelCap).CapitalizeFirst(), thing, MessageTypeDefOf.PositiveEvent, true);
                }
                return true;
            }
            //jcLog.Warning("TryDoSpawn - FAILED");
            return false;
        }

        private bool TryFindSpawnCell(out IntVec3 result)
        {
            //jcLog.Warning("TryFindSpawnCell");
            Rot4 thingRot = this.parent.Rotation;
            ///IntVec3 thingCent = this.parent.Position + IntVec3.North.RotatedBy(thingRot);
            IntVec3 thingCent = this.parent.Position;
            IntVec2 thingsSize = thingRot.FacingCell.ToIntVec2;
            //jcLog.Warning("TryFindSpawnCell - Center " + thingCent);
            //jcLog.Warning("TryFindSpawnCell - Rot " + thingRot);
            //jcLog.Warning("TryFindSpawnCell - Size " + thingsSize);
            IEnumerable<IntVec3> adjCells = GenAdj.CellsAdjacentAlongEdge(thingCent, thingRot, thingsSize, LinkDirections.Down);
            //jcLog.Warning("TryFindSpawnCell Cells Found - " + adjCells.Count());

            result = thingCent + IntVec3.North.RotatedBy(thingRot);
            //jcLog.Warning("TryFindSpawnCell - Spawn Cell ? " + result);
            return true;
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (spawned)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.ChangeSpawnThing();
                act.defaultLabel = "Change Spawn Item";
                act.defaultDesc = "On";
                act.activateSound = SoundDef.Named("Click");
                act.icon = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight", true);
                yield return act;
            }

            if (Prefs.DevMode && this.thingToSpawn != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn " + this.thingToSpawn.thingDef.label,
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        this.TryDoSpawn();
                        this.ResetCountdown();
                    }
                };
            }
        }

        private void ChangeSpawnThing()
        {
            thingToSpawnIndex = (thingToSpawnIndex + 1) % WallStuffSettings.listOfSpawnableThings.Count;
            this.thingToSpawn = WallStuffSettings.listOfSpawnableThings[thingToSpawnIndex];
            this.ResetCountdown();
        }

        private void ResetCountdown()
        {
            this.ticksUntilSpawn = this.WallMountedPropsSpawner.spawnIntervalRange.RandomInRange;
        }

        public override void PostExposeData()
        {
            string str = (!this.WallMountedPropsSpawner.saveKeysPrefix.NullOrEmpty()) ? (this.WallMountedPropsSpawner.saveKeysPrefix + "_") : null;
            Scribe_Values.Look<int>(ref this.ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
            Scribe_Values.Look<int>(ref this.thingToSpawnIndex, str + "thingToSpawnIndex", 0, false);
            this.thingToSpawn = WallStuffSettings.listOfSpawnableThings[thingToSpawnIndex];
        }

        public override string CompInspectStringExtra()
        {
            if (this.WallMountedPropsSpawner.writeTimeLeftToSpawn && (!this.WallMountedPropsSpawner.requiresPower || this.PowerOn))
            {
                return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(this.thingToSpawn.thingDef, null, this.thingToSpawn.count)) + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod();
            }
            return null;
        }
    }
}