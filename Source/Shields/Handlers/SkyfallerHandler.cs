using System;
using System.Linq;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Handlers
{
    public class SkyfallerHandler
    {
        static SkyfallerHandler()
        {
            Log.Message("Frontier Developments Shields :: Skyfaller handler enabled");
        }

        private static bool HandlePod(DropPodIncoming pod)
        {
            try
            {
                if (Mod.ShieldManager.Block(pod.Map, Common.ToVector3WithY(pod.Position, 0), Mod.Settings.DropPodDamage))
                {
                    foreach (var pawn in pod.Contents.innerContainer.Where(p => p is Pawn))
                    {
                        // TODO create story?
                        pawn.Kill(new DamageInfo(new DamageDef(), 100));
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
                if (Mod.ShieldManager.Block(skyfaller.Map, Common.ToVector3WithY(skyfaller.Position, 0),
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
        
        [HarmonyPatch(typeof(Skyfaller), "Impact")]
        static class Patch_Skyfaller_Impact
        {
            static bool Prefix(Skyfaller __instance)
            {
                if (__instance.Map == null) return true;
                if (__instance.GetType() == typeof(DropPodIncoming))
                {
                    return HandlePod((DropPodIncoming) __instance);
                }
                else
                {
                    return HandleGeneric(__instance);
                }
            }
        }
    }
}
