using System;
using FrontierDevelopments.General;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Module.CrashLandingModule
{
    public class CrashPodHandler
    {
        private static Type crashPodType = Type.GetType("CrashLanding.CrashPod, CrashLanding");
        
        private static bool Block(Bullet crashPod, int damage)
        {
            if (crashPod.Map.GetComponent<ShieldManager>().Block(Common.ToVector3WithY(crashPod.Position, 0), damage))
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

        public static bool CrashPod_Impact_Prefix(Bullet __instance)
        {
            // harmony can sometimes register a bullet as a crash pod
            if (!crashPodType.IsAssignableFrom(__instance.GetType())) return true;
            return !Block(__instance, Mod.Settings.SkyfallerDamage);
        }
        
        public static bool CrashPod_Part_Impact_Prefix(Bullet __instance)
        {
            // harmony can sometimes register a bullet as a crash pod
            if (!crashPodType.IsAssignableFrom(__instance.GetType())) return true;
            return !Block(__instance, Mod.Settings.SkyfallerDamage / 6);
        }
    }
}