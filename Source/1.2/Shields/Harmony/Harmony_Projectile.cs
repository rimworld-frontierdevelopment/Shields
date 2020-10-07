using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_Projectile
    {
        private static readonly MethodInfo impactMethod = AccessTools.Method(typeof(Projectile), "Impact");

        private static readonly List<string> BlacklistedDefs = new List<string>();
        private static readonly List<Type> BlacklistedTypes = new List<Type>();

        public static void BlacklistDef(string def)
        {
            BlacklistedDefs.Add(def);
        }
        
        public static void BlacklistType(Type type)
        {
            BlacklistedTypes.Add(type);
        }

        private static bool IsBlacklisted(Thing thing)
        {
            return BlacklistedDefs.Contains(thing.def.defName) || BlacklistedTypes.Contains(thing.GetType());
        }

        private static void Impact(Projectile projectile)
        {
            impactMethod.Invoke(projectile, new object[] { null });
        }

        private static bool TryBlockProjectile(
            Projectile projectile,
            Vector3 currentPosition,
            Vector3 nextPosition,
            int ticksToImpact,
            Vector3 origin)
        {
            // allow projectiles that have no damage components through
            if (projectile.def?.projectile?.damageDef == null) return false;

            var damages = new ShieldDamages(
                new ShieldDamage(
                    projectile.def.projectile.damageDef,
                    projectile.def.projectile.GetDamageAmount(1f)));
            return TryBlock(
                projectile,
                // TODO might be able to calculate overhead projectiles in 3D
                new Vector3(currentPosition.x, 0, currentPosition.z),
                new Vector3(nextPosition.x, 0, nextPosition.z),
                ticksToImpact,
                origin,
                projectile.def.projectile.flyOverhead,
                damages) != null;
        }

        protected static Vector3? TryBlock(
            Thing projectile,
            Vector3 currentPosition,
            Vector3 nextPosition,
            int ticksToImpact,
            Vector3 origin,
            bool flyOverhead,
            ShieldDamages damages)
        {
            if (IsBlacklisted(projectile)) return null;

            if (flyOverhead)
            {
                if (ticksToImpact <= 1)
                {
                    // fix for fire foam projectiles having 99999 damage
                    if (projectile.def.defName == "Bullet_Shell_Firefoam")
                    {
                        damages.OverrideDamage = 10;
                    }

                    var blocked = TryBlockOverhead(projectile, origin, currentPosition, damages) != null;
                    if (blocked) return currentPosition;
                }
            }
            else
            {
                return new ShieldQuery(projectile.Map)
                    .IsActive()
                    .Intersects(
                        PositionUtility.ToVector3(currentPosition),
                        PositionUtility.ToVector3(nextPosition))
                    .Block(damages);
            }

            return null;
        }
        
        protected static Vector3? TryBlockOverhead(
            Thing projectile,
            Vector3 origin,
            Vector3 currentPosition,
            ShieldDamages damages)
        {
            return new ShieldQuery(projectile.Map)
                .IsActive()
                .Intersects(origin, true)
                .Intersects(currentPosition)
                // TODO calculate mortar damage better
                .Block(damages);
        }

        private static bool ShouldImpact(Projectile projectile)
        {
            if (projectile.def.projectile.flyOverhead) return false;
            var type = projectile.GetType();
            return typeof(Projectile_Explosive).IsAssignableFrom(type);
        }

        [HarmonyPatch(typeof(Projectile), "CheckForFreeInterceptBetween")]
        static class Patch_CheckForFreeInterceptBetween
        {
            [HarmonyPostfix]
            static bool AddShieldCheck(
                bool __result,
                Projectile __instance,
                Vector3 lastExactPos,
                Vector3 newExactPos,
                Vector3 ___origin,
                int ___ticksToImpact)
            {
                if(__result == false && TryBlockProjectile(__instance,
                    lastExactPos,
                    newExactPos,
                    ___ticksToImpact,
                    ___origin))
                {
                    if (ShouldImpact(__instance))
                    {
                        Impact(__instance);
                        return true;
                    }
                    __instance.Destroy();
                    return true;
                }

                return __result;
            }
        }
    }
}