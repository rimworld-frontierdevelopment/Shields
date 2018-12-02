using System;
using System.Linq;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Module.RimworldModule
{
    public class Harmony_Skyfaller
    {
        private const int ShieldHitPreDelay = 20;

        private static bool HandlePod(DropPodIncoming pod)
        {
            try
            {
                if (pod.Map.GetComponent<ShieldManager>().Block(Common.ToVector3WithY(pod.Position, 0), Mod.Settings.DropPodDamage))
                {
                    foreach (var pawn in pod.Contents.innerContainer.Where(p => p is Pawn).Select(p => (Pawn)p))
                    {
                        pawn.Kill(new DamageInfo(new DamageDef(), 100));
                        TaleRecorder.RecordTale(LocalDefOf.KilledByImpactingOnShield, pawn, pod.Position, pod.Map);
                    }
                    pod.Destroy();
                    Messages.Message("fd.shields.incident.droppod.blocked.body".Translate(), new GlobalTargetInfo(pod.Position, pod.Map), MessageTypeDefOf.NeutralEvent);
                    return false;
                }
            }
            catch (InvalidOperationException) {}
            return true;
        }

        private static bool HandleGeneric(Skyfaller skyfaller)
        {
            try
            {
                if (skyfaller.Map.GetComponent<ShieldManager>().Block(Common.ToVector3WithY(skyfaller.Position, 0),
                    Mod.Settings.SkyfallerDamage))
                {
                    skyfaller.def.skyfaller.impactSound?.PlayOneShot(
                        SoundInfo.InMap(new TargetInfo(skyfaller.Position, skyfaller.Map)));
                    skyfaller.Destroy();
                    Messages.Message("fd.shields.incident.skyfaller.blocked.body".Translate(), new GlobalTargetInfo(skyfaller.Position, skyfaller.Map), MessageTypeDefOf.NeutralEvent);
                    return false;
                }
            }
            catch (InvalidOperationException) {}
            return true;
        }

        [HarmonyPatch(typeof(Skyfaller), nameof(Skyfaller.Tick))]
        static class Patch_Tick
        {
            static bool Prefix(Skyfaller __instance)
            {
                if (__instance.Map != null && __instance.ticksToImpact == ShieldHitPreDelay)
                {
                    if (__instance.GetType() == typeof(DropPodIncoming))
                    {
                        return HandlePod((DropPodIncoming) __instance);
                    }
                    else
                    {
                        return HandleGeneric(__instance);
                    }
                }
                return true;
            }
        }
    }
}
