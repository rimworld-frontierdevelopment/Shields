using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using HarmonyLib;
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

        private static void KillPawns(IEnumerable<Pawn> pawns, Map map, IntVec3 position)
        {
            pawns.ToList().Do(pawn => KillPawn(pawn, map, position));
        }

        private static void KillPawn(Pawn pawn, Map map, IntVec3 position)
        {
            // spawn on map for just an instant
            GenPlace.TryPlaceThing(pawn, position, map, ThingPlaceMode.Near);
            pawn.inventory.DestroyAll();
            pawn.Kill(new DamageInfo(DamageDefOf.Crush, 100));
            pawn.Corpse.Destroy();
            TaleRecorder.RecordTale(LocalDefOf.KilledByImpactingOnShield, pawn, position, map);
        }

        private static bool HandlePod(
            Skyfaller pod,
            ActiveDropPodInfo podInfo,
            IShieldQueryWithIntersects shields,
            float damage,
            string messageBody = "fd.shields.incident.droppod.blocked.body")
        {
            if (shields.Block(damage) != null)
            {
                KillPawns(podInfo.GetDirectlyHeldThings().OfType<Pawn>(), pod.Map, pod.Position);
                Messages.Message(messageBody.Translate(),
                    new GlobalTargetInfo(pod.Position, pod.Map),
                    MessageTypeDefOf.NeutralEvent);
                pod.Destroy();
                return false;
            }
            return true;
        }

        private static bool HandleGeneric(
            Skyfaller skyfaller,
            IShieldQueryWithIntersects shields,
            float damage,
            string messageBody = "fd.shields.incident.skyfaller.blocked.body")
        {
            if (shields.Block(damage) != null)
            {
                skyfaller.def.skyfaller.impactSound?.PlayOneShot(
                    SoundInfo.InMap(new TargetInfo(skyfaller.Position, skyfaller.Map)));
                Messages.Message(messageBody.Translate(),
                    new GlobalTargetInfo(skyfaller.Position, skyfaller.Map),
                    MessageTypeDefOf.NeutralEvent);
                skyfaller.Destroy();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Skyfaller), nameof(Skyfaller.Tick))]
        static class Patch_Tick
        {
            static bool Prefix(Skyfaller __instance)
            {
                if (__instance.Map != null 
                    && !__instance.def.skyfaller.reversed
                    && __instance.ticksToImpact == ShieldHitPreDelay
                    && !whitelistedDefs.Contains(__instance.def.defName))
                {
                    var shields = new ShieldQuery(__instance.Map)
                        .IsActive()
                        .Intersects(PositionUtility.ToVector3WithY(__instance.Position, 0));

                    switch (__instance)
                    {
                        case IActiveDropPod incoming:
                            return HandlePod(__instance, incoming.Contents, shields, Mod.Settings.DropPodDamage);
                        default:
                            return HandleGeneric(__instance, shields, Mod.Settings.SkyfallerDamage);
                    }
                }
                return true;
            }
        }
    }
}
