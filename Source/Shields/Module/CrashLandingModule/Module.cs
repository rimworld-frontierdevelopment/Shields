using System;
using System.Linq;
using System.Reflection;
using FrontierDevelopments.Shields.Harmony;
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
                                var baseType = Type.GetType("CrashLanding.CrashPod, CrashLanding");
                                var types = baseType.AllSubclassesNonAbstract();
                                var blockingTypes = "";
                                
                                foreach (var current in types)
                                {
                                    blockingTypes += current.Name + " ";

                                    if (current.Name.Contains("_part"))
                                    {
                                        harmony.Patch(
                                            current.GetMethod("Impact", BindingFlags.NonPublic | BindingFlags.Instance),
                                            new HarmonyMethod(typeof(CrashPodHandler).GetMethod(nameof(CrashPodHandler.CrashPod_Part_Impact_Prefix))));
                                    }
                                    else
                                    {
                                        harmony.Patch(
                                            current.GetMethod("Impact", BindingFlags.NonPublic | BindingFlags.Instance),
                                            new HarmonyMethod(typeof(CrashPodHandler).GetMethod(nameof(CrashPodHandler.CrashPod_Impact_Prefix))));
                                    }
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
                try
                {
                    var toBlacklist = DefDatabase<ThingDef>.AllDefs
                        .Where(thingDef => thingDef != null)
                        .Where(thingDef =>
                        {
                            var fullName = thingDef.thingClass?.FullName;
                            var defName = thingDef.defName;

                            if (fullName != null && defName != null)
                            {
                                return fullName.Contains("CrashLanding") && defName.Contains("Bullet");
                            }
                            Log.Warning("Frontier Developments Shields :: Invalid def found " + fullName + " " + defName + " (Not a shield bug! Def fullName or defName is null. Report this to the mod the def is from)");
                            return false;
                        }).Select(def => def.defName)
                        .ToList();
                    if (toBlacklist.Count > 0)
                    {
                        var blacklisted = "";
                        toBlacklist.ForEach(def =>
                        {
                            blacklisted += " " + def;
                            Harmony_Projectile.BlacklistDef(def);
                        });
                        Log.Message("Frontier Developments Shields :: Crash Landing blacklisting projectiles for (" +
                                    blacklisted + ")");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
        }
    }
}