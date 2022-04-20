using System.Linq;
using System.Reflection;
using CentralizedClimateControl;
using FrontierDevelopments.General.Comps;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.ClimateControl
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
        
        private static void ReplaceHeatsink(ThingDef def)
        {
            float exhaust = 1000;
            
            foreach (CompProperties comp in def.comps)
            {
                if (comp.compClass == typeof(Comp_HeatSink))
                {
                    comp.compClass = typeof(Comp_ClimateControlHeatsink);

                    exhaust = ((CompProperties_HeatSink) comp).grams / 0.8f;
                }
            }
            
            def.comps.Add(new CompProperties_AirFlow()
            {
                compClass = typeof(Comp_AirFlowConsumer),
                flowType = AirFlowType.Any,
                baseAirExhaust = exhaust
            });
        }

        private static bool HasHeatsink(ThingDef def)
        {
            if (def.comps == null || def.comps.Count < 1) return false;
            return def.comps.Any(comp => comp.compClass == typeof(Comp_HeatSink));
        }

        private static void Patch()
        {
            DefDatabase<ThingDef>.AllDefs
                .Where(HasHeatsink)
                .Do(ReplaceHeatsink);
        }
    }
}