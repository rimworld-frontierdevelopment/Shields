using System;
using System.Reflection;
using FrontierDevelopments.Core;
using Harmony;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ClimateControlSupport
    {
        public const string ClimateControlSupportAssembly =
            "Integrations/FrontierDevelopements-Shields-ClimateControl.dll";
        
        public static void Load(HarmonyInstance harmony)
        {
            const string centralizedClimateControlName = "CentralizedClimateControl";
            var centralizedClimateControlVersion = new Version(1, 5, 0, 0);
        
            try
            {
                var climateControlAssembly = Assembly.Load(centralizedClimateControlName);
                if (climateControlAssembly != null)
                {
                    var version = new AssemblyName(climateControlAssembly.FullName).Version;
                    if (version == centralizedClimateControlVersion)
                    {
                        var assembly = AssemblyUtility.FindModAssembly(Mod.ModName, ClimateControlSupportAssembly);
                        if (assembly != null)
                        {
                            harmony.PatchAll(assembly);
                            Log.Message("Frontier Developments Shields :: enabled Centralized Climate Control support");
                        }
                        else
                        {
                            Log.Warning("Frontier Developments Shields :: unable to load Centralized Climate Control support assembly");
                        }
                    }
                    else
                    {
                        Log.Warning("Frontier Developments Shields :: Centralized Climate Control " + version + 
                                    "is loaded and " + centralizedClimateControlVersion + " is required, not enabling support");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("Frontier Developments Shields :: exception while loading Centralized Climate Control support: " + e.Message);
            }
        }
    }
}