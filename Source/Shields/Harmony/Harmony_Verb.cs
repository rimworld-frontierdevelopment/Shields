using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    
    public class Harmony_Verb
    {
        private static List<Type> uncheckedTypes = new List<Type>();

        public static void BlacklistType(Type type)
        {
            uncheckedTypes.Add(type);
        }
        
        static bool ShieldBlocks(Thing caster, Verb verb, IntVec3 source, LocalTargetInfo target)
        {
            if (!verb.verbProps.requireLineOfSight) return false;
            if (uncheckedTypes.Exists(a => a.IsInstanceOfType(verb))) return false;
            return caster.Map.GetComponent<ShieldManager>().Shielded(Common.ToVector3(source), Common.ToVector3(target.Cell), caster.Faction);
        }

        [HarmonyPatch(typeof(Verb), nameof(Verb.CanHitTargetFrom) , new [] { typeof(IntVec3), typeof(LocalTargetInfo) })]
        static class Patch_CanHitTargetFrom
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var patchPhase = 0;
            
                foreach (var instruction in instructions)
                {
                    switch (patchPhase)
                    {
                        // find end of loop
                        case 0:
                            if (instruction.opcode == OpCodes.Blt)
                            {
                                patchPhase = 1;
                            }
                            break;
                        case 1:
                            var continueLabel = il.DefineLabel();
                            instruction.labels.Add(continueLabel);
                    
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Verb), nameof(Verb.caster)));
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_Verb), nameof(ShieldBlocks)));
                            yield return new CodeInstruction(OpCodes.Brfalse, continueLabel);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            yield return new CodeInstruction(OpCodes.Ret);

                            patchPhase = -1;
                            break;
                    }
                    yield return instruction;
                }
            }
        }
    }
}