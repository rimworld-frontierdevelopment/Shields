using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Harmony_Pawn
    {
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
        static class Patch_Pawn_GetGizmos
        {
            [HarmonyPostfix]
            private static IEnumerable<Gizmo> AddShieldDeployGizmo(IEnumerable<Gizmo> __result, Pawn __instance)
            {
                foreach (var gizmo in __result)
                {
                    yield return gizmo;
                }

                var shields = ShieldUtility.InventoryShields(__instance).OfType<IShield>().ToList();
                if (shields.Any())
                {
                    yield return new DeployShieldGizmo(__instance, shields);
                }
            }

            [HarmonyPostfix]
            static IEnumerable<Gizmo> AddDevAddToInventory(
                IEnumerable<Gizmo> __result, 
                Pawn __instance,
                List<ThingComp> ___comps)
            {
                foreach (var gizmo in __result)
                {
                    yield return gizmo;
                }

                if (Prefs.DevMode 
                    &&__instance.Faction == Faction.OfPlayer
                    && __instance.RaceProps.baseBodySize >= 2.0f)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV - Add shield",
                        defaultDesc = "DEV - Add shield to inventory",
                        action = () =>
                        {
                            var shield = __instance.Map.listerThings
                                .ThingsOfDef(LocalDefOf.MinifiedShieldGeneratorPortable).First();
                            shield.DeSpawn();
                            __instance.inventory.innerContainer.TryAdd(shield, 1, false);
                        }
                    };
                }
            }
        }
    }
}