using CombatExtended;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields;
using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Harmony_ProjectileCE : Harmony_Projectile
    {
        private static readonly MethodInfo impactMethod = AccessTools.Method(typeof(ProjectileCE), "Impact");

        private static bool ShouldImpact(ProjectileCE projectile)
        {
            if (projectile.def.projectile.flyOverhead) return false;
            var type = projectile.GetType();
            return typeof(ProjectileCE_Explosive).IsAssignableFrom(type);
        }

        private static ShieldDamages CalculateDamages(ProjectileCE projectile)
        {
            var ap = projectile.def.projectile.GetArmorPenetration(1f);
            var damages = new ShieldDamages(
                new ShieldDamage(
                    projectile.def.projectile.damageDef,
                    projectile.def.projectile.GetDamageAmount(ap)));
            (projectile.def.projectile as ProjectilePropertiesCE)?.secondaryDamage.ForEach(second =>
            {
                damages.Add(new ShieldDamage(
                    second.def,
                    second.amount * ap));
            });
            return damages;
        }

        private static bool TryBlockOverheadProjectileCE(
            ProjectileCE projectile,
            Vector3 currentPosition,
            int ticksToImpact,
            Vector2 origin)
        {
            return TryBlock(
                projectile,
                currentPosition,
                ticksToImpact,
                PositionUtility.ToVector3(origin),
                // TODO might be able to calculate the exact path with 3d CE projectiles
                projectile.def.projectile.flyOverhead,
                CalculateDamages(projectile)) != null;
        }

        private static bool TryBlockProjectileCE(
            ProjectileCE projectile,
            Vector3 currentPosition,
            Vector3 nextPosition,
            int ticksToImpact,
            Vector2 origin)
        {
            return TryBlock(
                projectile,
                currentPosition,
                nextPosition,
                ticksToImpact,
                PositionUtility.ToVector3(origin),
                // TODO might be able to calculate the exact path with 3d CE projectiles
                projectile.def.projectile.flyOverhead,
                CalculateDamages(projectile)) != null;
        }

        [HarmonyPatch(typeof(ProjectileCE), "CheckForCollisionBetween")]
        private static class Patch_CheckForCollisionBetween
        {
            [HarmonyPrefix]
            private static bool CheckShieldCollision(ProjectileCE __instance, bool __result, Vector2 ___origin, int ___ticksToImpact)
            {
                if (__instance == null) return true;

                var current = __instance.ExactPosition;
                var last = current - __instance.ExactMinusLastPos;

                var shouldBlock = TryBlockProjectileCE(
                    __instance,
                    last,
                    current,
                    ___ticksToImpact,
                    ___origin);

                if (shouldBlock)
                {
                    if (ShouldImpact(__instance))
                    {
                        __instance.ExactPosition = last;
                        impactMethod.Invoke(__instance, new object[] { null });
                    }
                    else
                    {
                        __instance.Destroy();
                    }
                }
                return !shouldBlock;
            }
        }

        [HarmonyPatch(typeof(ProjectileCE), "ImpactSomething")]
        private static class Patch_CheckCellForCollision
        {
            [HarmonyPrefix]
            private static bool BlockMortarImpacts(ProjectileCE __instance, int ___ticksToImpact, Vector2 ___origin)
            {
                if (!__instance.def.projectile.flyOverhead) return true;

                bool shouldBlock = TryBlockOverheadProjectileCE(__instance, __instance.ExactPosition, ___ticksToImpact, ___origin);
                if (shouldBlock)
                    __instance.Destroy();

                return !shouldBlock;
            }
        }
    }
}