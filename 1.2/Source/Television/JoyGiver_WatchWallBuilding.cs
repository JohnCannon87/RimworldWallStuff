using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace WallStuff
{
    class JoyGiver_WatchWallBuilding : JoyGiver_WatchBuilding
    {
        private static List<Thing> tmpCandidates = new List<Thing>();

        public override Job TryGiveJob(Pawn pawn)
        {
            Thing thing = this.FindBestGame(pawn, false, IntVec3.Invalid);
            if (thing != null)
            {
                return this.TryGivePlayJob(pawn, thing);
            }
            return null;
        }

        private Thing FindBestGame(Pawn pawn, bool inBed, IntVec3 partySpot)
        {
            tmpCandidates.Clear();
            this.GetSearchSet(pawn, tmpCandidates);
            //jcLog.Warning("Found & Returned " + tmpCandidates.Count + " Things");
            if (tmpCandidates.Count == 0)
            {
                return null;
            }
            Predicate<Thing> predicate = (Thing t) => this.CanInteractWith(pawn, t, inBed);
            //jcLog.Warning("Is Party Spot Valid? " + partySpot.IsValid);
            if (partySpot.IsValid)
            {
                Predicate<Thing> oldValidator = predicate;
                predicate = ((Thing x) => PartyUtility.InPartyArea(x.Position, partySpot, pawn.Map) && oldValidator(x));
            }
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            List<Thing> searchSet = tmpCandidates;
            //PathEndMode peMode = PathEndMode.OnCell;
            //TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            //jcLog.Warning("Finding Closest Thing"); ;
            Thing thing = searchSet[0];
            //jcLog.Warning("Thing Spawned " + thing.Spawned);
            //jcLog.Warning("num3 " + (9999f * 9999f));
            //jcLog.Warning("num4 " + (float)(position - thing.Position).LengthHorizontalSquared);
            //Thing result = GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator, null);
            Thing result = GenClosest.ClosestThing_Global(position, searchSet, 9999f, validator, null);
            //jcLog.Warning("Found: " + result.def.defName);
            tmpCandidates.Clear();
            return result;
        }

        protected override void GetSearchSet(Pawn pawn, List<Thing> outCandidates)
        {
            //jcLog.Warning("GetSearchSet");
            outCandidates.Clear();
            if (this.def.thingDefs == null)
            {
                //jcLog.Warning("NoThingDefs");
                return;
            }
            if (this.def.thingDefs.Count == 1)
            {
                //jcLog.Warning("OneThingDef");
                outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(this.def.thingDefs[0]));
            }
            else
            {
                //jcLog.Warning("MultipleThingDefs");
                for (int i = 0; i < this.def.thingDefs.Count; i++)
                {
                    ThingDef thingDef = this.def.thingDefs[i];
                    //jcLog.Warning("Checking Thing Def: " + thingDef.defName);
                    List<Thing> thingsFound = pawn.Map.listerThings.ThingsOfDef(thingDef);
                    //jcLog.Warning("Found " + thingsFound.Count + " Things");
                    outCandidates.AddRange(thingsFound);
                }
            }
        }

        protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
        {
            if (!base.CanInteractWith(pawn, t, inBed))
            {
                //jcLog.Warning("CanInteractWithFalse");
                return false;
            }
            //jcLog.Warning("CanInteractWithTrue");
            if (inBed)
            {
                //jcLog.Warning("CanInteractWith");
                Building_Bed bed = pawn.CurrentBed();
                if (t.def.building.isEdifice){
                    //jcLog.Warning("CanInteractWith-ItsAnEdifice");
                    return WatchBuildingUtility.CanWatchFromBed(pawn, bed, t);
                } else
                {
                    //jcLog.Warning("CanInteractWith-ItsNotAnEdifice");
                    return WatchWallBuildingUtility.CanWatchFromBed(pawn, bed, t);
                }
            }
            return true;
        }

        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            IntVec3 c;
            Building t2;
            //jcLog.Warning("TryGivePlayJob");
            if (t.def.building.isEdifice)
            {
                //jcLog.Warning("TryGivePlayJob-ItsAnEdifice");
                if (!WatchWallBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
                {
                    //jcLog.Warning("TryGivePlayJob-ItsAnEdifice Couldn't Find Watch Cell");
                    return null;
                }
            } else
            {
                //jcLog.Warning("TryGivePlayJob-ItsNotAnEdifice");
                if (!WatchBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
                {
                    //jcLog.Warning("TryGivePlayJob-ItsNotAnEdifice Couldn't Find Watch Cell");
                    return null;
                }
            }
            return new Job(this.def.jobDef, t, c, t2);
        }
    }
}
