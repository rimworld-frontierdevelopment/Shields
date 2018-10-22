using FrontierDevelopments.General;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Module.CrashLandingModule
{
    public class CrashPodHandler
    {
        private static bool Block(CrashLanding.CrashPod crashPod, int damage)
        {
            if (Mod.ShieldManager.Block(crashPod.Map, Common.ToVector3WithY(crashPod.Position, 0), damage))
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

        public static bool CrashPod_Impact_Prefix(CrashLanding.CrashPod __instance)
        {
            // harmony can sometimes register a bullet as a crash pod
            if (typeof(CrashLanding.CrashPod) != __instance.GetType()) return true;
            return !Block(__instance, Mod.Settings.SkyfallerDamage);
        }
        
        public static bool CrashPod_Part_Impact_Prefix(CrashLanding.CrashPod __instance)
        {
            // harmony can sometimes register a bullet as a crash pod
            if (typeof(CrashLanding.CrashPod) != __instance.GetType()) return true;
            return !Block(__instance, Mod.Settings.SkyfallerDamage / 6);
        }
    }
}