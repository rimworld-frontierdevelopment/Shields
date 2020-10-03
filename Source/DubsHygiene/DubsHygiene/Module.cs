using DubsBadHygiene;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace FrontierDevelopments.Shields.BadHygiene
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            Log.Message("Frontier Developments Shields :: Loading Dubs Bad Hygiene support");
            
            var harmony = new HarmonyLib.Harmony("FrontierDevelopment.Shields.BadHygiene");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        static class Patch_GenerateImpliedDefs_PostResolve
        {
            private static bool EnableThisMod()
            {
                if (!Mod.Settings.EnableDubsBadHygieneSupport)
                    return false;

                if (!Mod.Settings.EnableCentralizedClimateControlSupport)
                    return true;

                foreach (var modInfo in ModsConfig.ActiveModsInLoadOrder)
                {
                    switch (modInfo.PackageId.ToLower())
                    {
                        case "dubwise.dubsbadhygiene": return true;
                        case "mlie.centralizedclimatecontrol": return false;
                    }
                }

                return false;
            }

            [HarmonyPostfix]
            public static void PatchShieldsForDubsSupport()
            {
                if (EnableThisMod())
                {
                    Log.Message("Frontier Developments Shields :: Using Dubs Bad Hygiene cooling. To support Centralize Climate Control disable Dubs support or load CCC first in the mod list.");
                    Patch();
                }
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
                        compClass = typeof(Comp_DubsAirVent),
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

    
}