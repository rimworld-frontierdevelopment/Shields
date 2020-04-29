using CentralizedClimateControl;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FrontierDevelopments.ClimateControl
{
    public class Module
    {
        [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        private class Patch_GenerateImpliedDefs_PostResolve
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                Patch();
            }
        }

        private struct VentPair
        {
            public string defName;
            public float exhaust;
        }

        // TODO this didn't work in an XML patch. can that be fixed?
        public static void Patch()
        {
            var toPatch = new List<VentPair>();
            toPatch.Add(new VentPair
            {
                defName = "Building_ShieldGenerator",
                exhaust = 1000
            });
            toPatch.Add(new VentPair
            {
                defName = "Building_ShieldGeneratorLarge",
                exhaust = 2500
            });

            foreach (var ventPair in toPatch)
            {
                var def = DefDatabase<ThingDef>.GetNamed(ventPair.defName);
                foreach (var comp in def.comps)
                {
                    if (comp.compClass == typeof(General.Comps.Comp_HeatSink))
                    {
                        comp.compClass = typeof(Comp_ClimateControlHeatsink);
                    }
                }
                def.comps.Add(new CompProperties_AirFlow()
                {
                    compClass = typeof(Comp_AirFlowConsumer),
                    flowType = AirFlowType.Any,
                    baseAirExhaust = ventPair.exhaust
                });
            }
        }
    }
}