using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WallStuff
{
    public static class WatchWallBuildingUtility
    {
        private static List<int> allowedDirections = new List<int>();

        public static bool CanWatchWallFromBed(Pawn pawn, Building_Bed bed, Thing toWatch)
        {
            if (!EverPossibleToWatchFrom(pawn.Position, toWatch.Position, pawn.Map, true))
            {
                return false;
            }
            if (toWatch.def.rotatable)
            {
                Rot4 rotation = bed.Rotation;
                CellRect cellRect = toWatch.OccupiedRect();
                if (rotation == Rot4.North && cellRect.maxZ < pawn.Position.z)
                {
                    return false;
                }
                if (rotation == Rot4.South && cellRect.minZ > pawn.Position.z)
                {
                    return false;
                }
                if (rotation == Rot4.East && cellRect.maxX < pawn.Position.x)
                {
                    return false;
                }
                if (rotation == Rot4.West && cellRect.minX > pawn.Position.x)
                {
                    return false;
                }
            }
            List<int> list = CalculateAllowedDirections(toWatch.def, toWatch.Rotation);
            for (int i = 0; i < list.Count; i++)
            {
                if (GetWatchCellRect(toWatch.def, toWatch.Position, toWatch.Rotation, list[i]).Contains(pawn.Position))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool TryFindBestWatchCell(Thing toWatch, Pawn pawn, bool desireSit, out IntVec3 result, out Building chair)
        {
            List<int> list = CalculateAllowedDirections(toWatch.def, toWatch.Rotation);
            IntVec3 intVec = IntVec3.Invalid;
            for (int i = 0; i < list.Count; i++)
            {
                var vecNorth = toWatch.Position + IntVec3.North.RotatedBy(toWatch.Rotation);
                CellRect watchCellRect = GetWatchCellRect(toWatch.def, vecNorth, toWatch.Rotation, list[i]);
                IntVec3 centerCell = watchCellRect.CenterCell;
                int num = watchCellRect.Area * 4;
                for (int j = 0; j < num; j++)
                {
                    IntVec3 intVec2 = centerCell + GenRadial.RadialPattern[j];
                    if (watchCellRect.Contains(intVec2))
                    {
                        bool flag = false;
                        Building building = null;
                        if (EverPossibleToWatchFrom(intVec2, vecNorth, toWatch.Map, false) && !intVec2.IsForbidden(pawn) && pawn.CanReserve(intVec2, 1, -1, null, false) && pawn.Map.pawnDestinationReservationManager.CanReserve(intVec2, pawn, false))
                        {
                            if (desireSit)
                            {
                                building = intVec2.GetEdifice(pawn.Map);
                                if (building != null && building.def.building.isSittable && pawn.CanReserve(building, 1, -1, null, false))
                                {
                                    flag = true;
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        if (flag)
                        {
                            if (desireSit)
                            {
                                Rot4 arg_15E_0 = building.Rotation;
                                Rot4 rot = new Rot4(list[i]);
                                if (arg_15E_0 != rot.Opposite)
                                {
                                    intVec = intVec2;
                                    goto IL_17F;
                                }
                            }
                            result = intVec2;
                            chair = building;
                            return true;
                        }
                    }
                    IL_17F:;
                }
            }
            if (intVec.IsValid)
            {
                result = intVec;
                chair = intVec.GetEdifice(pawn.Map);
                return true;
            }
            result = IntVec3.Invalid;
            chair = null;
            return false;
        }

        public static bool CanWatchFromBed(Pawn pawn, Building_Bed bed, Thing toWatch)
        {
            if (!EverPossibleToWatchFrom(pawn.Position, toWatch.Position, pawn.Map, true))
            {
                return false;
            }
            if (toWatch.def.rotatable)
            {
                Rot4 rotation = bed.Rotation;
                CellRect cellRect = toWatch.OccupiedRect();
                if (rotation == Rot4.North && cellRect.maxZ < pawn.Position.z)
                {
                    return false;
                }
                if (rotation == Rot4.South && cellRect.minZ > pawn.Position.z)
                {
                    return false;
                }
                if (rotation == Rot4.East && cellRect.maxX < pawn.Position.x)
                {
                    return false;
                }
                if (rotation == Rot4.West && cellRect.minX > pawn.Position.x)
                {
                    return false;
                }
            }
            List<int> list = CalculateAllowedDirections(toWatch.def, toWatch.Rotation);
            for (int i = 0; i < list.Count; i++)
            {
                var vecNorth = toWatch.Position + IntVec3.North.RotatedBy(toWatch.Rotation);
                if (GetWatchCellRect(toWatch.def, vecNorth, toWatch.Rotation, list[i]).Contains(pawn.Position))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool EverPossibleToWatchFrom(IntVec3 watchCell, IntVec3 buildingCenter, Map map, bool bedAllowed)
        {
            return (watchCell.Standable(map) || (bedAllowed && watchCell.GetEdifice(map) is Building_Bed)) && GenSight.LineOfSight(buildingCenter, watchCell, map, true, null, 0, 0);
        }

        private static List<int> CalculateAllowedDirections(ThingDef toWatchDef, Rot4 toWatchRot)
        {
            allowedDirections.Clear();
            if (toWatchDef.rotatable)
            {
                allowedDirections.Add(toWatchRot.AsInt);
            }
            else
            {
                allowedDirections.Add(0);
                allowedDirections.Add(1);
                allowedDirections.Add(2);
                allowedDirections.Add(3);
            }
            return allowedDirections;
        }

        private static CellRect GetWatchCellRect(ThingDef def, IntVec3 center, Rot4 rot, int watchRot)
        {
            Rot4 a = new Rot4(watchRot);
            if (def.building == null)
            {
                def = (def.entityDefToBuild as ThingDef);
            }
            CellRect result;
            if (a.IsHorizontal)
            {
                int num = center.x + GenAdj.CardinalDirections[watchRot].x * def.building.watchBuildingStandDistanceRange.min;
                int num2 = center.x + GenAdj.CardinalDirections[watchRot].x * def.building.watchBuildingStandDistanceRange.max;
                int num3 = center.z + def.building.watchBuildingStandRectWidth / 2;
                int num4 = center.z - def.building.watchBuildingStandRectWidth / 2;
                if (def.building.watchBuildingStandRectWidth % 2 == 0)
                {
                    if (a == Rot4.West)
                    {
                        num4++;
                    }
                    else
                    {
                        num3--;
                    }
                }
                result = new CellRect(Mathf.Min(num, num2), num4, Mathf.Abs(num - num2) + 1, num3 - num4 + 1);
            }
            else
            {
                int num5 = center.z + GenAdj.CardinalDirections[watchRot].z * def.building.watchBuildingStandDistanceRange.min;
                int num6 = center.z + GenAdj.CardinalDirections[watchRot].z * def.building.watchBuildingStandDistanceRange.max;
                int num7 = center.x + def.building.watchBuildingStandRectWidth / 2;
                int num8 = center.x - def.building.watchBuildingStandRectWidth / 2;
                if (def.building.watchBuildingStandRectWidth % 2 == 0)
                {
                    if (a == Rot4.North)
                    {
                        num8++;
                    }
                    else
                    {
                        num7--;
                    }
                }
                result = new CellRect(num8, Mathf.Min(num5, num6), num7 - num8 + 1, Mathf.Abs(num5 - num6) + 1);
            }
            return result;
        }

    }
}
