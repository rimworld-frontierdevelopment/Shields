using System.Linq;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Harmony_ThingOwner
    {
        [HarmonyPatch(typeof(ThingOwner), "NotifyRemoved")]
        static class ThingOwnerRemoved
        {
            [HarmonyPostfix]
            static void NotifyShieldRemoved(ThingOwner __instance, Thing item)
            {
                switch (__instance?.Owner)
                {
                    case Pawn_InventoryTracker inventory:
                        var pawn = inventory.pawn;
                        if (pawn.Spawned)
                        {
                            ShieldDeploymentUtility.DeployedShields(pawn)
                                .Where(shield => ShieldDeploymentUtility.ItemProvidesShield(item, shield))
                                .Do(shield => ShieldDeploymentUtility.UndeployShield(pawn, shield));
                        }
                        break;
                }
            }
        }
    }
}