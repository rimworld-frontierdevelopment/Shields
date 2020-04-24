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
        private static readonly List<string> BlacklistedDefs = new List<string>();

        public static void BlacklistDef(string def)
        {
            BlacklistedDefs.Add(def);
        }

        private static bool TryBlockProjectile(
            Projectile projectile,
            Vector3 currentPosition,
            Vector3 nextPosition,
            int ticksToImpact,
            Vector3 origin)
        {

            var damages = new ShieldDamages(
                new ShieldDamage(
                    projectile.def.projectile.damageDef,
                    projectile.def.projectile.GetDamageAmount(1f)));

            return TryBlock(
                projectile,
                currentPosition,
                nextPosition,
                ticksToImpact,
                origin,
                projectile.def.projectile.flyOverhead,
                damages) != null;
        }

        protected static Vector3? TryBlock(
            Thing projectile,
            Vector3 currentPosition,
            int ticksToImpact,
            Vector3 origin,
            bool flyOverhead,
            ShieldDamages damages)
        {
            var shieldManager = projectile.Map.GetComponent<ShieldManager>();
            if (BlacklistedDefs.Contains(projectile.def.defName)) return null;

            if (flyOverhead)
            {
                if (ticksToImpact <= 1)
                {
                    // fix for fire foam projectiles having 99999 damage
                    if (projectile.def.defName == "Bullet_Shell_Firefoam")
                    {
                        damages.OverrideDamage = 10;
                    }

                    if (shieldManager.Block(
                        PositionUtility.ToVector3(origin),
                        PositionUtility.ToVector3(currentPosition),
                        // TODO calculate mortar damage better
                        damages)) return currentPosition;
                }
            }
            return null;
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
            var shieldManager = projectile.Map.GetComponent<ShieldManager>();
            if (BlacklistedDefs.Contains(projectile.def.defName)) return null;

            if (flyOverhead)
            {
                if (ticksToImpact <= 1)
                {
                    // fix for fire foam projectiles having 99999 damage
                    if (projectile.def.defName == "Bullet_Shell_Firefoam")
                    {
                        damages.OverrideDamage = 10;
                    }

                    if (shieldManager.Block(
                        PositionUtility.ToVector3(origin),
                        PositionUtility.ToVector3(currentPosition),
                        // TODO calculate mortar damage better
                        damages)) return currentPosition;
                    return null;
                }
            }
            else
            {
                return shieldManager.Block(
                           PositionUtility.ToVector3(origin),
                           PositionUtility.ToVector3(currentPosition),
                           PositionUtility.ToVector3(nextPosition),
                           damages);
            }

            return null;
        }

        private static bool ShouldImpact(Projectile projectile)
        {
            if (projectile.def.projectile.flyOverhead) return false;
            var type = projectile.GetType();
            return typeof(Projectile_Explosive).IsAssignableFrom(type);
        }
        
        [HarmonyPatch(typeof(Projectile), "CheckForFreeInterceptBetween")]
        static class CheckForFreeInterceptBetween
        {
            static bool Prefix(Projectile __instance, ref bool __result, Vector3 lastExactPos, Vector3 newExactPos, int ___ticksToImpact, Vector3 ___origin)
            {
                if (lastExactPos == newExactPos) { __result = false; return false; }

                if (TryBlockProjectile(__instance, lastExactPos, newExactPos, ___ticksToImpact, ___origin)) { __result = true; __instance.Destroy(); return false; }

                if (ShouldImpact(__instance)) { __result = true; __instance.Destroy(); return false; }

                return true;
            }
        }

    }
}