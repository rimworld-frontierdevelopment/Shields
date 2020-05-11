using DubsBadHygiene;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FrontierDevelopments.BadHygiene
{
    [HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PostResolve")]
    internal static class Patch_GenerateImpliedDefs_PostResolve
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Patch();
        }

        public static void Patch()
        {
            List<string> defs = new List<string>() { "Building_ShieldGenerator", "Building_ShieldGeneratorLarge" };

            foreach (string def in defs)
            {
                var tDef = DefDatabase<ThingDef>.GetNamed(def);

                foreach (CompProperties comp in tDef.comps)
                {
                    if (comp.compClass == typeof(General.Comps.Comp_HeatSink))
                    {
                        comp.compClass = typeof(Comp_BadHygieneHeatsink);
                    }
                }

                tDef.comps.Add(new CompProperties_CompAirconUnit()
                {
                    compClass = typeof(Comp_ShieldCooling),
                    energyPerSecond = -25,
                    CoolingRate = 100,
                    Capacity = 100
                });

                var compProperties_Pipe = new CompProperties_Pipe
                {
                    mode = PipeType.Air
                };
                tDef.comps.Add(compProperties_Pipe);
            }
        }
    }
}