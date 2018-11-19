using System;
using System.Reflection;
using FrontierDevelopments.Core;
using Harmony;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class RedistHeatSupport
    {
        public const string RedistHeatSupportAssembly =
            "Integrations/FrontierDevelopments-Shields-RedistHeat.dll";
        
        public static bool Load(HarmonyInstance harmony)
        {
            const string redistHeatName = "redistHeat";
            // TODO Morgloz released 50 tagged as version 47. This check probably doesn't help...
//            var redistHeatVersion = new Version(47, 0, 0, 0);
        
            try
            {
                var redistHeatAssembly = Assembly.Load(redistHeatName);
                if (redistHeatAssembly != null)
                {
//                    var version = new AssemblyName(redistHeatAssembly.FullName).Version;
//                    if (version == redistHeatVersion)
//                    {
                        var assembly = AssemblyUtility.FindModAssembly(Mod.ModName, RedistHeatSupportAssembly);
                        if (assembly != null)
                        {
                            harmony.PatchAll(assembly);
                            Log.Message("Frontier Developments Shields :: enabled RedistHeat support");
                            return true;
                        }
                        else
                        {
                            Log.Warning("Frontier Developments Shields :: unable to load RedistHeat support assembly");
                        }
//                    }
//                    else
//                    {
//                        Log.Warning("Frontier Developments Shields :: RedistHeat " + version + 
//                                    "is loaded and " + redistHeatVersion + " is required, not enabling support");
//                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("Frontier Developments Shields :: exception while loading RedistHeat support: " + e.Message);
            }

            return false;
        }
    }
}