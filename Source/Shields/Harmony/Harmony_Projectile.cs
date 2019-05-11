using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
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
                        Common.ToVector3(origin),
                        Common.ToVector3(currentPosition),
                        // TODO calculate mortar damage better
                        damages)) return currentPosition;
                    return null;
                }
            }
            else
            {
                return shieldManager.Block(
                           Common.ToVector3(origin),
                           Common.ToVector3(currentPosition),
                           Common.ToVector3(nextPosition),
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
            }
        }
    }
}