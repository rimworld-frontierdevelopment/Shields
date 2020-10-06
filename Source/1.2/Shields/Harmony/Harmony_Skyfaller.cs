using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private const int ShieldAutosavePreDelay = 30;

        private static readonly List<string> whitelistedDefs = new List<string>();

        private const int MinimumTicksBetweenSkyfallerAutosave = GenDate.TicksPerHour;
        private static readonly FieldInfo autosaverTicksSinceLast = AccessTools.Field(typeof(Autosaver), "ticksSinceSave");

        private static int TicksSinceLastAutosave
        {
            get => (int) autosaverTicksSinceLast.GetValue(Find.Autosaver);
            set => autosaverTicksSinceLast.SetValue(Find.Autosaver, value);
        }

        public static void WhitelistDef(string defName)
        {
            whitelistedDefs.Add(defName);
        }

        public static void KillPawns(IEnumerable<Pawn> pawns, Map map, IntVec3 position)
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

        private static bool HandleShuttle(
            ShuttleIncoming shuttle,
            IShieldQueryWithIntersects shields,
            float damage,
            string messageBody = "fd.shields.incident.shuttle.blocked.body")
        {
            var faction = shuttle.Faction ?? Faction.Empire;
            if (shields.HostileTo(faction).Block(damage) != null)
            {
                // get around a bug in 1.2 where the shuttle has no contents
                ActiveDropPodInfo contents = null;
                try
                {
                    contents = shuttle.Contents;
                }
                catch (InvalidCastException) {}

                if(contents != null)
                    KillPawns(contents.GetDirectlyHeldThings().OfType<Pawn>(), shuttle.Map, shuttle.Position);

                Messages.Message(messageBody.Translate(),
                    new GlobalTargetInfo(shuttle.Position, shuttle.Map),
                    MessageTypeDefOf.NeutralEvent);
                shuttle.Destroy();
                return false;
            }
            return true;
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

        public static bool? HandleOther(Skyfaller skyfaller, IShieldQueryWithIntersects shields)
        {
            // for allowing mod integrations to patch this
            return null;
        }

        private static bool AutosaveIfNeeded()
        {
            if (Mod.Settings.AutosaveOnSkyfallers && TicksSinceLastAutosave >= MinimumTicksBetweenSkyfallerAutosave)
            {
                TicksSinceLastAutosave = 0;
                LongEventHandler.QueueLongEvent(() => Find.Autosaver.DoAutosave(), "Autosaving", false, null);
                return true;
            }

            return false;
        }

        private static void TryAutosave(Skyfaller skyfaller, Map map)
        {
            if (!Mod.Settings.AutosaveOnSkyfallers) return;

            var shielded = new ShieldQuery(map)
                .Intersects(PositionUtility.ToVector3WithY(skyfaller.Position, 0))
                .Get()
                .Any();
            if (shielded)
            {
                if (AutosaveIfNeeded())
                {
                    Log.Message("Frontier Development Shields :: " + skyfaller.Label + " can fall within a shield, autosaving");
                }
            }
        }

        [HarmonyPatch(typeof(Skyfaller), nameof(Skyfaller.Tick))]
        static class Patch_Tick
        {
            static bool Prefix(Skyfaller __instance)
            {
                if (!Mod.Settings.BlockSkyfallers) return true;
                if (__instance.Map != null 
                    && !__instance.def.skyfaller.reversed
                    && !whitelistedDefs.Contains(__instance.def.defName))
                {
                    if (__instance.ticksToImpact == ShieldAutosavePreDelay)
                    {
                        TryAutosave(__instance, __instance.Map);
                    }
                    else if(__instance.ticksToImpact == ShieldHitPreDelay)
                    {
                        var shields = new ShieldQuery(__instance.Map)
                            .IsActive()
                            .Intersects(PositionUtility.ToVector3WithY(__instance.Position, 0));

                        var other = HandleOther(__instance, shields);
                        if (other != null) return other.Value;

                        switch (__instance)
                        {
                            case ShuttleIncoming shuttle:
                                return HandleShuttle(shuttle, shields, Mod.Settings.SkyfallerDamage);
                            case IActiveDropPod incoming:
                                return HandlePod(__instance, incoming.Contents, shields, Mod.Settings.DropPodDamage);
                            default:
                                return HandleGeneric(__instance, shields, Mod.Settings.SkyfallerDamage);
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Skyfaller), nameof(Skyfaller.SpawnSetup))]
        static class Patch_SpawnSetup
        {
            [HarmonyPostfix]
            static void HandleAutosaveIfCanImpactShields(Skyfaller __instance, Map map)
            {
                TryAutosave(__instance, map);
            }
        }
    }
}
