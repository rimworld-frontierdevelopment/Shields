using System;
using System.Linq;
using System.Reflection;
using CrashLanding;
using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Module.CrashLandingModule
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            Log.Message("Frontier Developments Shields :: Loading Crash Landing support");
            
            var harmony = new HarmonyLib.Harmony("FrontierDevelopment.Shields.CrashLanding");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            Harmony_Projectile.BlacklistType(typeof(CrashPod));

            // var baseType = Type.GetType("CrashLanding.CrashPod, CrashLanding");
            // var types = baseType.AllSubclassesNonAbstract();
            // var blockingTypes = "";
            //
            // foreach (var current in types)
            // {
            //     blockingTypes += current.Name + " ";
            //
            //     if (current.Name.Contains("_part"))
            //     {
            //         harmony.Patch(
            //             current.GetMethod("Impact", BindingFlags.NonPublic | BindingFlags.Instance),
            //             new HarmonyMethod(typeof(CrashPodHandler).GetMethod(nameof(CrashPodHandler.CrashPod_Part_Impact_Prefix))));
            //     }
            //     else
            //     {
            //         harmony.Patch(
            //             current.GetMethod("Impact", BindingFlags.NonPublic | BindingFlags.Instance),
            //             new HarmonyMethod(typeof(CrashPodHandler).GetMethod(nameof(CrashPodHandler.CrashPod_Impact_Prefix))));
            //     }
            // }
        }

        // [HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PostResolve")]
        // class Patch_GenerateImpliedDefs_PostResolve
        // {
        //     [HarmonyPriority(Priority.Last)]
        //     static void Postfix()
        //     {
        //         try
        //         {
        //             var toBlacklist = DefDatabase<ThingDef>.AllDefs
        //                 .Where(thingDef => thingDef != null)
        //                 .Where(thingDef =>
        //                 {
        //                     var fullName = thingDef.thingClass?.FullName;
        //                     var defName = thingDef.defName;
        //
        //                     if (fullName != null && defName != null)
        //                     {
        //                         return fullName.Contains("CrashLanding") && defName.Contains("Bullet");
        //                     }
        //                     return false;
        //                 }).Select(def => def.defName)
        //                 .ToList();
        //             if (toBlacklist.Count > 0)
        //             {
        //                 var blacklisted = "";
        //                 toBlacklist.ForEach(def =>
        //                 {
        //                     blacklisted += " " + def;
        //                     Harmony_Projectile.BlacklistDef(def);
        //                 });
        //                 Log.Message("Frontier Developments Shields :: Crash Landing blacklisting projectiles for (" +
        //                             blacklisted + ")");
        //             }
        //         }
        //         catch (Exception e)
        //         {
        //             Log.Error(e.Message);
        //         }
        //     }
        // }
    }
}