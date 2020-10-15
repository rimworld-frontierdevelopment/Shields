using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
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

        private static bool Impact(Projectile projectile, IShieldField shield, Vector3 point)
        {
            impactMethod.Invoke(projectile, new object[] { null });
            return true;
        }

        protected static SoundDef GetExplosionSound(ProjectileProperties props)
        {
            return props.soundExplode ?? props.damageDef.soundExplosion;
        }

        private static SoundDef GetExplosionSound(CompProperties_Explosive props)
        {
            return props.explosionSound ?? props.explosiveDamageType.soundExplosion;
        }

        private static SoundDef GetExplosionSound(Projectile projectile)
        {
            switch (projectile)
            {
                case Projectile_Explosive explosive:
                    return GetExplosionSound(explosive.def.projectile);
                default:
                    return projectile.AllComps
                        .OfType<CompExplosive>()
                        .Select(compExplosive => GetExplosionSound((CompProperties_Explosive) compExplosive.props))
                        .FirstOrDefault();
            }
        }

        private static bool TryBlockProjectile(
            Projectile projectile,
            Vector3 currentPosition,
            Vector3 nextPosition,
            int ticksToImpact,
            Vector3 origin,
            Action<IShieldField, Vector3> onBlock = null)
        {
            // allow projectiles that have no damage components through
            if (projectile.def?.projectile?.damageDef == null) return false;

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
                damages,
                onBlock);
        }

        protected static bool TryBlock(
            Thing projectile,
            Vector3 currentPosition,
            Vector3 nextPosition,
            int ticksToImpact,
            Vector3 origin,
            bool flyOverhead,
            ShieldDamages damages,
            Action<IShieldField, Vector3> onBlock = null)
        {
            if (IsBlacklisted(projectile)) return false;

            if (flyOverhead)
            {
                if (ticksToImpact <= 1)
                {
                    // fix for fire foam projectiles having 99999 damage
                    if (projectile.def.defName == "Bullet_Shell_Firefoam")
                    {
                        damages.OverrideDamage = 10;
                    }

                    var blocked = TryBlockOverhead(projectile, origin, currentPosition, damages, onBlock);
                    return blocked;
                }
            }
            else
            {
                return new FieldQuery(projectile.Map)
                    .IsActive()
                    .Intersects(
                        currentPosition,
                        nextPosition)
                    .Block(damages, onBlock);
            }

            return false;
        }
        
        protected static bool TryBlockOverhead(
            Thing projectile,
            Vector3 origin,
            Vector3 currentPosition,
            ShieldDamages damages,
            Action<IShieldField, Vector3> onBlock = null)
        {
            return new FieldQuery(projectile.Map)
                .IsActive()
                .Intersects(origin, true)
                .Intersects(currentPosition)
                // TODO calculate mortar damage better
                .Block(damages, onBlock);
        }

        private static bool IsExplosive(Projectile projectile) =>
            projectile is Projectile_Explosive || projectile.AllComps.OfType<CompExplosive>().Any();

        private static bool IsOverhead(Projectile projectile) =>
            projectile.def.projectile.flyOverhead;

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
                if (__result == false)
                {
                    return TryBlockProjectile(__instance,
                        lastExactPos.Yto0(),
                        newExactPos.Yto0(),
                        ___ticksToImpact,
                        ___origin,
                        (shield, point) =>
                        {
                            if (IsOverhead(__instance))
                            {
                                ExplosionUtility.DoSmokeExplosion(__instance, GetExplosionSound(__instance));
                                __instance.Destroy();
                            }
                            else
                            {
                                // check if it is an explosive projectile to impact
                                if (!IsExplosive(__instance) || !Impact(__instance, shield, point))
                                {
                                    // this is a regular projectile, remove it
                                    __instance.Destroy();
                                }
                            }
                        });
                }

                return __result;
            }
        }
    }
}