using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WallStuff
{
    [StaticConstructorOnStartup]
    public static class MultiAnalyzerPatchApplier
    {
        static MultiAnalyzerPatchApplier()
        {
            // Check the mod setting
            if (WallStuffSettings.enableMultiAnalyzerResearchPatch)
            {
                Log.Message("Applying MultiAnalyzer to WallMountedMultiAnalyzer patch.");
                ApplyMultiAnalyzerPatch();
            }
            else
            {
                Log.Message("MultiAnalyzer patch is disabled.");
            }
        }

        private static void ApplyMultiAnalyzerPatch()
        {
            // Iterate through all ResearchProjectDefs
            foreach (var researchProject in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                // Check if the research project has requiredResearchFacilities
                if (researchProject.requiredResearchFacilities != null && researchProject.requiredResearchFacilities.Contains(ThingDef.Named("MultiAnalyzer")))
                {
                    // Replace MultiAnalyzer with WallMountedMultiAnalyzer
                    researchProject.requiredResearchFacilities.Remove(ThingDef.Named("MultiAnalyzer"));
                    researchProject.requiredResearchFacilities.Add(ThingDef.Named("WallMountedMultiAnalyzer"));

                    Log.Message($"Patched {researchProject.defName}: Replaced MultiAnalyzer with WallMountedMultiAnalyzer.");
                }
            }
        }
    }

}
