using System;
using System.Linq;
using System.Reflection;
using CombatExtended;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields;
using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Harmony_ProjectileCE : Harmony_Projectile
    {
        private static readonly MethodInfo impactMethod = AccessTools.Method(typeof(ProjectileCE), "Impact");

        private static bool IsExplosive(ProjectileCE projectile) =>
            projectile is ProjectileCE_Explosive || projectile.AllComps.OfType<CompExplosiveCE>().Any();

        private static bool IsOverhead(ProjectileCE projectile) =>
            projectile.def.projectile.flyOverhead;

        private static bool ShouldImpact(ProjectileCE projectile)
        {
            return !IsOverhead(projectile) && IsExplosive(projectile);
        }

        private static ShieldDamages CalculateDamages(ThingDef def)
        {
            var ap = def.projectile.GetArmorPenetration(1f);
            var damages = new ShieldDamages(
                new ShieldDamage(
                    def.projectile.damageDef,
                    def.projectile.GetDamageAmount(ap)));
            (def.projectile as ProjectilePropertiesCE)?.secondaryDamage.ForEach(second =>
            {
                damages.Add(new ShieldDamage(
                    second.def,
                    second.amount * ap));
            });
            return damages;
        }

        private static bool TryBlockProjectileCE(
            ProjectileCE projectile,
            Vector3 currentPosition,
            Vector3 nextPosition,
            int ticksToImpact,
            Vector2 origin,
            Action<IShieldField, Vector3> onBlock = null)
        {
            return TryBlock(
                projectile,
                currentPosition,
                nextPosition,
                ticksToImpact,
                PositionUtility.ToVector3(origin),
                false, // not needed since CE mortars are calculated in 3D
                CalculateDamages(projectile.def),
                onBlock);
        }

        private static bool Impact(ProjectileCE projectile, IShieldField shield, Vector3 position)
        {
            var traverse = new Traverse(projectile);
            var ticksToImpact = traverse.Field("ticksToImpact");
            var startingTicksToImpact = traverse.Property("StartingTicksToImpact").GetValue<float>();

            // rewind the projectile 1 tick to get the last position before the CheckForCollisionBetween we are in
            ticksToImpact.SetValue(ticksToImpact.GetValue<int>() + 1);

            while (shield.Collision(projectile.ExactPosition.Yto0()))
            {
                var amount = ticksToImpact.GetValue<int>();
                if (amount > startingTicksToImpact)
                {
                    Log.Message("Unable to find good place to detonate " + projectile.ThingID + ", destroying without detonating");
                    return false;
                }
                ticksToImpact.SetValue(amount + 1);
            }
            impactMethod.Invoke(projectile, new object[] { null });
            return true;
        }

        [HarmonyPatch(typeof(ProjectileCE), "CheckForCollisionBetween")]
        static class Patch_CheckForCollisionBetween
        {
            [HarmonyPrefix]
            static bool CheckShieldCollision(ProjectileCE __instance, bool __result, Vector2 ___origin, int ___ticksToImpact)
            {
                var current = __instance.ExactPosition;
                var last = current - __instance.ExactMinusLastPos;

                return !TryBlockProjectileCE(
                    __instance,
                    last,
                    current,
                    ___ticksToImpact,
                    ___origin, (shield, point) =>
                    {
                        if (!ShouldImpact(__instance) || !Impact(__instance, shield, point))
                            __instance.Destroy();
                    });
            }
        }

        [HarmonyPatch(typeof(ProjectileCE), "ImpactSomething")]
        static class Patch_CheckCellForCollision
        {
            [HarmonyPrefix]
            static bool PreventBlockedProjectilesFromImpacting(ProjectileCE __instance)
            {
                return !__instance.Destroyed;
            }
        }
    }
}