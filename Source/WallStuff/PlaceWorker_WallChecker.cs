using System.Linq;
using Verse;

namespace WallStuff
{
    public class PlaceWorker_WallChecker : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing( BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null )
        {
            var things = map.thingGrid.ThingsListAt( center );
            if (things.Exists( s => s is IWallAttachable ))
            {
                return ResourceBank.WallAlreadyOccupied;
            }
            return true;
        }
    }
}