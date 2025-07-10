using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Reflection;

namespace WallStuff
{
	[HarmonyPatch]
	public static class SuperChargerPatch
	{
		// This tells Harmony to find the private method using reflection
		static MethodBase TargetMethod()
		{
			return AccessTools.Method(typeof(JobGiver_GetNeuralSupercharge), "ClosestSupercharger");
		}

		[HarmonyPrefix]
		public static bool ClosestSuperchargerHarmonyPatch(ref Thing __result, Pawn pawn)
		{
            Thing wallMountedSuperCharger = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.WallMountedNeuralSupercharger), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, Validator);
			if (wallMountedSuperCharger == null)
			{
				return true;
			}
			__result = wallMountedSuperCharger;
			return false;
			bool Validator(Thing x)
			{
				CompNeuralSupercharger compNeuralSupercharger = x.TryGetComp<CompNeuralSupercharger>();
				if (compNeuralSupercharger.Charged && !x.IsForbidden(pawn))
				{
					return compNeuralSupercharger.CanAutoUse(pawn);
				}
				return false;
            }
		}
	}
}
