using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CombatExtended;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields;
using FrontierDevelopments.Shields.Harmony;
using Harmony;
using UnityEngine;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Harmony_ProjectileCE : Harmony_Projectile
    {
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

        [HarmonyPatch(typeof(ProjectileCE), nameof(ProjectileCE.Tick))]
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
                                && instruction.operand as MethodInfo == AccessTools.Method(typeof(ProjectileCE), "CheckForCollisionBetween"))
                            {
                                patchPhase = 1;
                            }
                            break;
                        }
                        case 1:
                        {
                            if (instruction.opcode == OpCodes.Brfalse)
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
                            var ticksToImpact = AccessTools.Field(typeof(ProjectileCE), "ticksToImpact");
                            
                            var keepGoing = il.DefineLabel();
                            var destroy = il.DefineLabel();
                            
                            instruction.labels.Add(keepGoing);

                            yield return new CodeInstruction(OpCodes.Ldarg_0){labels = new List<Label>(new[] {shieldTestLabel})}; // projectile
                            // currentPosition
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ProjectileCE), "get_LastPos"));
                            // nextPosition
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(ProjectileCE), nameof(ProjectileCE.ExactPosition)).GetGetMethod());
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, ticksToImpact);
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ProjectileCE), "origin"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_ProjectileCE), nameof(TryBlockProjectileCE)));
                            yield return new CodeInstruction(OpCodes.Brfalse, keepGoing);
                            
                            // move it back one
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, ticksToImpact);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            yield return new CodeInstruction(OpCodes.Add);
                            yield return new CodeInstruction(OpCodes.Stfld, ticksToImpact);
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            // TODO check for overhead, simulate mortar explosion
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_ProjectileCE), nameof(ShouldImpact)));
                            yield return new CodeInstruction(OpCodes.Brfalse, destroy);
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldnull);
                            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ProjectileCE), "Impact"));
                            yield return new CodeInstruction(OpCodes.Ret);
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = new List<Label>(new[] { destroy })};
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ProjectileCE), nameof(ProjectileCE.Destroy)));
                            yield return new CodeInstruction(OpCodes.Ret);

                            patchPhase = -1;
                            break;
                        }
                    }

                    yield return instruction;
                }
            }
        }

        [HarmonyPatch(typeof(ProjectileCE), "ImpactSomething")]
        static class Patch_CheckCellForCollision
        {
            [HarmonyPrefix]
            static bool BlockMortarImpacts(ProjectileCE __instance, int ___ticksToImpact, Vector2 ___origin)
            {
                if (!__instance.def.projectile.flyOverhead) return true;

                var shouldBlock = TryBlockOverheadProjectileCE(__instance, __instance.ExactPosition, ___ticksToImpact, ___origin);
                if(shouldBlock)
                    __instance.Destroy();
                return !shouldBlock;
            }
        }
    }
}