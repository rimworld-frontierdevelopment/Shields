using System.Linq;
using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SRTS;
using Verse;

namespace FrontierDevelopments.Shields.SrtsExpandedIntegration.Harmony
{
    public class Harmony_STRSIncoming
    {
        [HarmonyPatch(typeof(Harmony_Skyfaller), nameof(Harmony_Skyfaller.HandleOther))]
        static class Patch_HandleOther
        {
            [HarmonyPostfix]
            private static bool? Allow_Friendly_SRTSIncoming(bool? result, Skyfaller skyfaller, IShieldQueryWithIntersects shields)
            {
                if (result == null)
                {
                    switch (skyfaller)
                    {
                        case SRTSIncoming incoming:
                            // as of this writing, ships are only controlled by the player and the ship has no faction assigned 
                            // try to get the ship faction for future proofing but fallback to the player
                            var faction = incoming.Faction ?? Faction.OfPlayer;
                            
                            var blocked = shields
                                .FriendlyTo(faction, true)
                                // TODO calculate damage based on mass and cargo
                                .Block(Mod.Settings.SkyfallerDamage);
                            if (blocked)
                            {
                                Harmony_Skyfaller.KillPawns(
                                    incoming.Contents.GetDirectlyHeldThings().OfType<Pawn>(),
                                    incoming.Map,
                                    incoming.Position);
                                Messages.Message("fd.shields.incident.ship.blocked.body".Translate(),
                                    new GlobalTargetInfo(incoming.Position, incoming.Map),
                                    MessageTypeDefOf.NeutralEvent);
                                incoming.Destroy();
                                return false;
                            }
                            return true;
                    }
                }
                return result;
            }
        }
    }
}