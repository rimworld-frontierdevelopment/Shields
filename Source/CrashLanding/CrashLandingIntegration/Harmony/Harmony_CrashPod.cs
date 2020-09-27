using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrashLanding;
using FrontierDevelopments.General;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Module.CrashLandingModule
{
    public class Harmony_CrashPod
    {
        private static IEnumerable<MethodInfo> SelectImpactMethods(bool getParts)
        {
            return typeof(CrashPod)
                .AllSubclasses()
                .Where(type => type.Name.Contains("_part") == getParts)
                .SelectMany(type => type.GetMethods(BindingFlags.Instance |  BindingFlags.NonPublic))
                .Where(method => method.Name == "Impact");
        }

        private static bool Block(Bullet crashPod, int damage)
        {
            var crashPodBlocked = new ShieldQuery(crashPod.Map)
                .IsActive()
                .Intersects(PositionUtility.ToVector3WithY(crashPod.Position, 0))
                .Block(damage) != null;
            if (crashPodBlocked)
            {
                Messages.Message("fd.shields.incident.crashpod.blocked.body".Translate(),
                    new GlobalTargetInfo(crashPod.Position, crashPod.Map), MessageTypeDefOf.NeutralEvent);
                crashPod.def.projectile.soundExplode.PlayOneShot(
                    SoundInfo.InMap(new TargetInfo(crashPod.Position, crashPod.Map)));
                crashPod.Destroy();
                return true;
            }

            return false;
        }

        [HarmonyPatch]
        static class Patch_CrashPod_Impact
        {
            [HarmonyTargetMethods]
            private static IEnumerable<MethodInfo> FindAnonymousFunction(HarmonyLib.Harmony instance)
            {
                return SelectImpactMethods(false);
            }

            [HarmonyPrefix]
            public static bool Block_CrashPod(CrashPod __instance)
            {
                // harmony can sometimes register a bullet as a crash pod
                return !Block(__instance, Mod.Settings.SkyfallerDamage);
            }
        }
        
        [HarmonyPatch]
        static class Patch_CrashPod_Part_Impact
        {
            [HarmonyTargetMethods]
            private static IEnumerable<MethodInfo> FindAnonymousFunction(HarmonyLib.Harmony instance)
            {
                return SelectImpactMethods(true);
            }

            [HarmonyPrefix]
            public static bool Block_CrashPod_Part(Bullet __instance)
            {
                // harmony can sometimes register a bullet as a crash pod
                return !Block(__instance, Mod.Settings.SkyfallerDamage / 6);
            }
        }
    }
}