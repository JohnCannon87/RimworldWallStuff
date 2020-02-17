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
            if (tmpCandidates.Count == 0)
            {
                return null;
            }
            Predicate<Thing> predicate = (Thing t) => this.CanInteractWith(pawn, t, inBed);
            if (partySpot.IsValid)
            {
                Predicate<Thing> oldValidator = predicate;
                predicate = ((Thing x) => GatheringsUtility.InGatheringArea(x.Position, partySpot, pawn.Map) && oldValidator(x));
            }
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            List<Thing> searchSet = tmpCandidates;
            Predicate<Thing> validator = predicate;
            Thing thing = searchSet[0];
            Thing result = GenClosest.ClosestThing_Global(position, searchSet, 9999f, validator, null);
            tmpCandidates.Clear();
            return result;
        }

        protected override void GetSearchSet(Pawn pawn, List<Thing> outCandidates)
        {
            outCandidates.Clear();
            if (this.def.thingDefs == null)
            {
                return;
            }
            if (this.def.thingDefs.Count == 1)
            {
                outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(this.def.thingDefs[0]));
            }
            else
            {
                for (int i = 0; i < this.def.thingDefs.Count; i++)
                {
                    ThingDef thingDef = this.def.thingDefs[i];
                    List<Thing> thingsFound = pawn.Map.listerThings.ThingsOfDef(thingDef);
                    outCandidates.AddRange(thingsFound);
                }
            }
        }

        protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
        {
            if (!base.CanInteractWith(pawn, t, inBed))
            {
                return false;
            }
            if (inBed)
            {
                Building_Bed bed = pawn.CurrentBed();
                if (t.def.building.isEdifice){
                    return WatchBuildingUtility.CanWatchFromBed(pawn, bed, t);
                } else
                {
                    return WatchWallBuildingUtility.CanWatchFromBed(pawn, bed, t);
                }
            }
            return true;
        }

        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            IntVec3 c;
            Building t2;
            if (t.def.building.isEdifice)
            {
                if (!WatchWallBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
                {
                    return null;
                }
            } else
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
                {
                    return null;
                }
            }
            return new Job(this.def.jobDef, t, c, t2);
        }
    }
}
