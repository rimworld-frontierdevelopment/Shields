using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_Skyfaller
    {
        private const int ShieldHitPreDelay = 20;

        private static readonly List<string> whitelistedDefs = new List<string>();

        public static void WhitelistDef(string defName)
        {
            whitelistedDefs.Add(defName);
        }

        private static void KillPawn(Pawn pawn, IntVec3 position, Map map)
        {
            // spawn on map for just an instant
            GenPlace.TryPlaceThing(pawn, position, map, ThingPlaceMode.Near);
            pawn.inventory.DestroyAll();
            pawn.Kill(new DamageInfo(DamageDefOf.Crush, 100));
            pawn.Corpse.Destroy();
            TaleRecorder.RecordTale(LocalDefOf.KilledByImpactingOnShield, pawn, position, map);
        }

        private static bool HandlePod(DropPodIncoming pod)
        {
            try
            {
                if (pod.Map.GetComponent<ShieldManager>().Block(PositionUtility.ToVector3WithY(pod.Position, 0), Mod.Settings.DropPodDamage))
                {
                    // OfType<Pawn>() causes weirdness here
                    foreach (var pawn in pod.Contents.innerContainer.Where(p => p is Pawn).Select(p => (Pawn)p))
                    {
                        KillPawn(pawn, pod.Position, pod.Map);
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
                if (skyfaller.Map.GetComponent<ShieldManager>().Block(PositionUtility.ToVector3WithY(skyfaller.Position, 0),
                    Mod.Settings.SkyfallerDamage))
                {
                    skyfaller.def.skyfaller.impactSound?.PlayOneShot(
                        SoundInfo.InMap(new TargetInfo(skyfaller.Position, skyfaller.Map)));
                    Messages.Message("fd.shields.incident.skyfaller.blocked.body".Translate(), new GlobalTargetInfo(skyfaller.Position, skyfaller.Map), MessageTypeDefOf.NeutralEvent);
                    skyfaller.Destroy();
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
                if (__instance.Map != null 
                    && __instance.ticksToImpact == ShieldHitPreDelay
                    && !whitelistedDefs.Contains(__instance.def.defName))
                {
                    switch (__instance)
                    {
                        case DropPodIncoming incoming:
                            return HandlePod(incoming);
                        case DropPodLeaving _:
                            return true;
                        default:
                            return HandleGeneric(__instance);
                            
                    }
                }
                return true;
            }
        }
    }
}
