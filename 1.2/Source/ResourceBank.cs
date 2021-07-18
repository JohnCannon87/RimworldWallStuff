using System.Security.Policy;
using UnityEngine;
using Verse;

// ReSharper disable All

namespace WallStuff
{
    [StaticConstructorOnStartup]
    public static class ResourceBank
    {
        public const string modName = "WallStuff";
        public static string NeedsWall = "Can only be placed on a wall";
        public static string WallAlreadyOccupied = "WallStuff_WallAlreadyOccupied".Translate();
    }
}