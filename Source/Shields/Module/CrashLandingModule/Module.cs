using System;
using System.Linq;
using System.Reflection;
using FrontierDevelopments.Shields.Module.RimworldModule;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Module.CrashLandingModule
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Crash Landing"))
            {
                try
                {
                    ((Action) (() =>
                            {
                                var harmony = HarmonyInstance.Create("frontierdevelopment.shields");
                                var baseType = typeof(CrashLanding.CrashPod);
                                var types = baseType.AllSubclassesNonAbstract();
                                var blockingTypes = "";
                                
                                foreach (Type current in types)
                                {
                                    blockingTypes += current.Name + " ";
                                    harmony.Patch(
                                        current.GetMethod("Impact", BindingFlags.NonPublic | BindingFlags.Instance),
                                        new HarmonyMethod(typeof(CrashPodHandler.Patch_CrashPod_Impact).GetMethod("Prefix")));
                                }
                                
                                Log.Message("Frontier Developments Shields :: Crash Landing support enabled. Blocking (" + blockingTypes.Trim() + ")");
                            }
                        ))();
                }
                catch (Exception) {}
            }
        }

        [HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PostResolve")]
        class Patch_GenerateImpliedDefs_PostResolve
        {
            [HarmonyPriority(Priority.Last)]
            static void Postfix()
            {
                var toBlacklist = DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                {
                    var fullName = thingDef.thingClass.FullName;
                    return fullName != null && fullName.Contains("CrashLanding") &&
                           thingDef.defName.Contains("Bullet");
                }).Select(def => def.defName)
                    .ToList();

                if (toBlacklist.Count > 0)
                {
                    var blacklisted = "";
                    
                    toBlacklist.ForEach(def =>
                    {
                        blacklisted += " " + def;
                        ProjectileHandler.BlacklistedDefs.Add(def);
                    });
                    Log.Message("Frontier Developments Shields :: Crash Landing blacklisting projectiles for (" + blacklisted + ")");
                }
            }
        }
    }
}