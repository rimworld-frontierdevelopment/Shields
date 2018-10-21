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
                crashPod.def.projectile.soundExplode.PlayOneShot(
                    SoundInfo.InMap(new TargetInfo(crashPod.Position, crashPod.Map)));
                crashPod.Destroy();
                Messages.Message("fd.shields.incident.crashpod.blocked.body".Translate(),
                    new GlobalTargetInfo(crashPod.Position, crashPod.Map), MessageTypeDefOf.NeutralEvent);
                return true;
            }

            return false;
        }

        public static class Patch_CrashPod_Impact
        {
            public static bool Prefix(CrashLanding.CrashPod __instance)
            {
                return !Block(__instance, Mod.Settings.SkyfallerDamage);
            }
        }
    }
}