using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Module.RimworldModule
{
    public class Harmony_Projectile
    {
        public static readonly List<string> BlacklistedDefs = new List<string>();

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.Tick))]
        static class Patch_Projectile_Tick
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
                                && (instruction.operand as MethodInfo).Name == "CheckForFreeInterceptBetween")
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
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Projectile), "destination"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Projectile_Tick), nameof(ShieldBlocks)));
                            yield return new CodeInstruction(OpCodes.Brfalse, keepGoing);
                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Projectile_Tick), nameof(ShouldImpact)));
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

            private static bool ShieldBlocks(
                Projectile projectile,
                Vector3 currentPosition,
                Vector3 nextPosition,
                int ticksToImpact,
                Vector3 origin,
                Vector3 destination)
            {
                var shieldManager = projectile.Map.GetComponent<ShieldManager>();
                if (BlacklistedDefs.Contains(projectile.def.defName)) return false;
                
                if (projectile.def.projectile.flyOverhead)
                {
                    if (ticksToImpact <= 1)
                    {
                        var damage = projectile.def.projectile.GetDamageAmount(1f);
                    
                        // fix for fire foam projectiles having 99999 damage
                        if (projectile.def.defName == "Bullet_Shell_Firefoam")
                        {
                            damage = 10;
                        }

                        // check to ensure the projectile won't roll the power required int heal the shields
                        if (damage >= 500)
                        {
                            Log.Warning("damage for " + projectile.Label + " too high at " + damage + ", reducing to 500. (This is probably a bug from the projectile having no damage set)");
                            damage = 500;
                        }

                        return shieldManager.Block(
                                   Common.ToVector3(nextPosition),
                                   Common.ToVector3(origin),
                                   destination,
                                   // TODO calculate mortar damage better
                                   damage) != null;
                    }
                }
                else
                {
                    return shieldManager.Block(
                               Common.ToVector3(origin),
                               Common.ToVector3(currentPosition),
                               Common.ToVector3(nextPosition),
                               projectile.def.projectile.GetDamageAmount(1f)) != null;
                }
                return false;
            }

            private static bool ShouldImpact(Projectile projectile)
            {
                if (projectile.def.projectile.flyOverhead) return false;
                var type = projectile.GetType();
                return typeof(Projectile_Explosive).IsAssignableFrom(type);
            }
        }
    }
}