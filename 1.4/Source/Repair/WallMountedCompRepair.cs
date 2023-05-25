using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Diagnostics;
using UnityEngine;

namespace WallStuff
{
    public class WallMountedCompRepair : ThingComp
    {
        private Boolean spawned = false;
        private Boolean overpowerMode = false;
        private int ticksSinceMacroTick = 0;
        private const int MacroTickInterval = 2500;
        private float protectionRadius = 6f;

        WallMountedCompProperties_Repair WallMountedPropsRepair
        {
            get
            {                
                return (WallMountedCompProperties_Repair)this.props;
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
            spawned = true;
        }

        public override void CompTick()
        {
        }

        public override void CompTickRare()
        {
            this.ticksSinceMacroTick = this.ticksSinceMacroTick + 250;

            if (ticksSinceMacroTick >= MacroTickInterval)
            {
                this.MacroTick();
                this.ticksSinceMacroTick = 0;
            }            
        }

        private void MacroTick()
        {
            IEnumerable<IntVec3> repairSquares = GenRadial.RadialCellsAround(this.parent.Position, this.protectionRadius, true);

            foreach (IntVec3 square in repairSquares)
            {
                List<Thing> things = this.parent.Map.thingGrid.ThingsListAt(square);

                foreach (Thing thing in things)
                {
                    // If it's a weapon or a piece of apparel just repair it at the defined rate
                    if(thing.def.IsWithinCategory(ThingCategoryDefOf.Weapons) || thing.def.IsWithinCategory(ThingCategoryDefOf.Apparel))
                    {
                        RepairThing(thing);
                    }

                    //If it's a Pawn get all their equipment and apparel
                    if (thing is Pawn pawn)
                    {
                        RepairPawnEquipment(pawn);
                        RepairPawnApparel(pawn);                        
                    }
                }
            }
        }

        private void RepairPawnApparel(Pawn pawn)
        {
            if(pawn.apparel != null && pawn.apparel.WornApparel != null)
            {
                List<Apparel> pawnApparel = pawn.apparel.WornApparel;

                //Repair all apparel
                foreach (Apparel apparel in pawnApparel)
                {
                    RepairThing(apparel);
                }
            }
        }

        private void RepairPawnEquipment(Pawn pawn)
        {
            if(pawn.equipment != null)
            {
                List<ThingWithComps> pawnEquipment = pawn.equipment.AllEquipmentListForReading;

                // Repair all equipment
                foreach (ThingWithComps equipment in pawnEquipment)
                {
                    RepairThing(equipment);
                }
            }
            
        }

        private void RepairThing(Thing thing)
        {
            if(thing.HitPoints < thing.MaxHitPoints)
            {
                int repairRateForThing = (int)Math.Round(thing.MaxHitPoints / 100 * this.GetRepairRate());
                thing.HitPoints = Math.Min(thing.MaxHitPoints, thing.HitPoints + repairRateForThing);
            }            
        }

        private double GetRepairRate()
        {
            if (this.overpowerMode)
            {
                return 4.2;
            }
            else
            {
                return 0.6;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (spawned)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.ChangePowerMode();
                act.defaultLabel = "Change Power Mode";
                act.defaultDesc = "On";
                act.activateSound = SoundDef.Named("Click");
                act.icon = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight", true);
                yield return act;
            }
        }

        private void ChangePowerMode()
        {
            this.overpowerMode = !this.overpowerMode;
            float powerUsage = 0f;
            if(overpowerMode)
            {
                powerUsage = WallMountedPropsRepair.OverpowerUsage;
            }
            else
            {
                powerUsage = WallMountedPropsRepair.NormalPowerUsage;
            }
            this.parent.TryGetComp<CompPowerTrader>().PowerOutput = powerUsage;
        }

        public override void PostExposeData()
        {
            string str = (!this.WallMountedPropsRepair.saveKeysPrefix.NullOrEmpty()) ? (this.WallMountedPropsRepair.saveKeysPrefix + "_") : null;
            Scribe_Values.Look<Boolean>(ref this.overpowerMode, str + "overpowerMode", false, false);
            Scribe_Values.Look<int>(ref this.ticksSinceMacroTick, str + "ticksSinceMacroTick", 0, false);
        }

        public override string CompInspectStringExtra()
        {
            if (this.WallMountedPropsRepair.requiresPower || this.PowerOn)
            {
                return "Overpower mode " + this.GetOverpowerString() + ", Power usage is: " + this.GetPowerUsage();
            }
            return null;
        }

        private int GetPowerUsage()
        {
            if (overpowerMode)
            {
                return WallMountedPropsRepair.OverpowerUsage;
            }
            else
            {
                return WallMountedPropsRepair.NormalPowerUsage;
            }
        }

        private string GetOverpowerString()
        {
            if (overpowerMode)
            {
                return "On";
            }
            else
            {
                return "Off";
            }
        }
    }
}