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
        
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.Tick))]
        static class Patch_Tick
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var patchPhase = 0;
                var shieldTestLabel = il.DefineLabel();

                foreach (var instruction in instructions)
                {
                    switch (patchPhase)
                    {
                        case 0:
                        {
                            if (instruction.opcode == OpCodes.Call
                                && instruction.operand as MethodInfo == AccessTools.Method(typeof(Projectile), "CheckForFreeInterceptBetween"))
                            {
                                patchPhase = 1;
                            }
                            break;
                        }
                        case 1:
                        {
                            if (instruction.opcode == OpCodes.Brfalse_S)
                            {
                                instruction.operand = shieldTestLabel;
                                patchPhase = 2;
                            }
                            break;
                        }
                        case 2:
                        {
                            if (instruction.opcode == OpCodes.Ret)
                            {
                                patchPhase = 3;
                            }
                            break;
                        }
                        case 3:
                        {
                            var keepGoing = il.DefineLabel();
                            var destroy = il.DefineLabel();
                            
                            instruction.labels.Add(keepGoing);

                            yield return new CodeInstruction(OpCodes.Ldarg_0){labels = new List<Label>(new[] {shieldTestLabel})}; // projectile
                            yield return new CodeInstruction(OpCodes.Ldloc_0); // currentPosition
                            yield return new CodeInstruction(OpCodes.Ldloc_1); // nextPosition
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld,AccessTools.Field(typeof(Projectile), "ticksToImpact"));
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Projectile), "origin"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_Projectile), nameof(TryBlockProjectile)));
                            yield return new CodeInstruction(OpCodes.Brfalse, keepGoing);
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_Projectile), nameof(ShouldImpact)));
                            yield return new CodeInstruction(OpCodes.Brfalse, destroy);
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldnull);
                            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Projectile), "Impact"));
                            yield return new CodeInstruction(OpCodes.Ret);
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = new List<Label>(new[] { destroy })};
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Projectile), nameof(Projectile.Destroy)));
                            yield return new CodeInstruction(OpCodes.Ret);

                            patchPhase = -1;
                            break;
                        }
                    }

                    yield return instruction;
                }

                if (patchPhase > 0)
                {
                    Log.Error("Patch for Projectile.Tick failed. Reached patchPhase: " + patchPhase);
                }
            }
        }
    }
}