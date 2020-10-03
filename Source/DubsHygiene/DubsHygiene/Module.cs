using DubsBadHygiene;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

            private static void ReplaceHeatsink(ThingDef def)
            {
                foreach (CompProperties comp in def.comps)
                {
                    if (comp.compClass == typeof(General.Comps.Comp_HeatSink))
                    {
                        comp.compClass = typeof(Comp_BadHygieneHeatsink);
                    }
                }
                
                def.comps.Add(new CompProperties_CompAirconUnit()
                {
                    compClass = typeof(Comp_DubsAirVent),
                    energyPerSecond = -25,
                    CoolingRate = 100,
                    Capacity = 100
                });
                
                def.comps.Add(new CompProperties_Pipe
                {
                    mode = PipeType.Air
                });
            }

            private static bool HasHeatsink(ThingDef def)
            {
                if (def.comps == null || def.comps.Count < 1) return false;
                return def.comps.Any(comp => comp.compClass == typeof(General.Comps.Comp_HeatSink));
            }

            private static void Patch()
            {
                DefDatabase<ThingDef>.AllDefs
                    .Where(HasHeatsink)
                    .Do(ReplaceHeatsink);
            }
        }
    }

    
}