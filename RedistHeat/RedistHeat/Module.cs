using System.Collections.Generic;
using Harmony;
using RedistHeat;
using RimWorld;
using Verse;

namespace FrontierDevelopments.RedistHeat
{
    public class Module
    {
        [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        class Patch_GenerateImpliedDefs_PostResolve
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                Patch();
            }
        }
        
        // TODO this didn't work in an XML patch. can that be fixed?
        private static void Patch()
        {
            var toPatch = new List<string>(new[] 
                {
                    "Building_ShieldGenerator",
                    "Building_ShieldGeneratorLarge"
                });

            foreach (var defName in toPatch)
            {
                var def = DefDatabase<ThingDef>.GetNamed(defName);
                foreach (var comp in def.comps)
                {
                    if (comp.compClass == typeof(FrontierDevelopments.General.CompProperties.CompProperties_HeatSink))
                    {
                        comp.compClass = typeof(CompProperties_RedistHeatSink);
                    }
                }
                def.comps.Add(new CompAirTraderProperties());
                def.placeWorkers.Add(typeof(PlaceWorker_DuctBase));
            }
        }
    }
}