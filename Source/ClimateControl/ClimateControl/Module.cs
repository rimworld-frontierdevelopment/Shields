using System.Collections.Generic;
using System.Reflection;
using CentralizedClimateControl;
using HarmonyLib;
using RimWorld;
using Verse;
using Mod = FrontierDevelopments.Shields.Mod;

namespace FrontierDevelopments.ClimateControl
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            Log.Message("Frontier Developments Shields :: Loading Centralized Climate Control (Continued) support");
            
            var harmony = new HarmonyLib.Harmony("FrontierDevelopment.Shields.ClimateControl");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static bool EnableThisMod()
        {
            if (!Mod.Settings.EnableCentralizedClimateControlSupport)
                return false;

            if (!Mod.Settings.EnableDubsBadHygieneSupport)
                return true;

            foreach (var modInfo in ModsConfig.ActiveModsInLoadOrder)
            {
                switch (modInfo.PackageId.ToLower())
                {
                    case "mlie.centralizedclimatecontrol": return true;
                    case "dubwise.dubsbadhygiene": return false;
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        class Patch_GenerateImpliedDefs_PostResolve
        {
            [HarmonyPostfix]
            static void PatchShieldsForClimateSupport()
            {
                if (EnableThisMod())
                {
                    Log.Message("Frontier Developments Shields :: Using Centralized Climate Control cooling. To support Dubs disable CCC support or load Dubs before CCC in the mod list.");
                    Patch();
                }
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