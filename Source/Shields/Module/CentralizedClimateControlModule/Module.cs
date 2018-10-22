using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FrontierDevelopments.General.Comps;
using FrontierDevelopments.Shields.Buildings;
using FrontierDevelopments.Shields.Module.CrashLandingModule;
using Harmony;
using Verse;

namespace FrontierDevelopments.Shields.Module.CentralizedClimateControlModule
{
    [StaticConstructorOnStartup]
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Centralized Climate Control"))
            {
                try
                {
                    ((Action) (() =>
                            {
                                var harmony = HarmonyInstance.Create("frontierdevelopment.shields");

                                harmony.Patch(
                                    typeof(Comp_HeatSink).GetMethod("AmbientTemp",
                                        BindingFlags.NonPublic | BindingFlags.Instance),
                                    new HarmonyMethod(
                                        typeof(HeatsinkPatch.Patch_Heatsink_AmbientTemp).GetMethod("Prefix")));

                                Log.Message(
                                    "Frontier Developments Shields :: Centralized Climate Control support enabled");
                            }
                        ))();
                }
                catch (Exception) {}
            }
        }
    }
}