using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_CastPositionFinder
    {
        private static float CalculateShieldPreference(float score, Map map, IntVec3 start, IntVec3 end)
        {
            var shieldManager = map.GetComponent<ShieldManager>();
            
            if (shieldManager.Shielded(PositionUtility.ToVector3(start), PositionUtility.ToVector3(end)))
            {
                score = (float)Math.Sqrt(score);
            }

            if (shieldManager.Shielded(PositionUtility.ToVector3(end), PositionUtility.ToVector3(start)))
            {
                score = (float) Math.Pow(score, 2);
            }
            return score;
        }
        
        [HarmonyPatch(typeof(CastPositionFinder), "CastPositionPreference")]
        static class Patch_CastPositionPreference
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var skipReturn = true;
                
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Ret && !skipReturn)
                    {
                        var caster = il.DeclareLocal(typeof(Pawn));
                        
                        // score exists on the stack already, don't need to add it
                        yield return new CodeInstruction(OpCodes.Ldsflda, AccessTools.Field(typeof(CastPositionFinder), "req"));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CastPositionRequest), "caster"));
                        yield return new CodeInstruction(OpCodes.Stloc, caster.LocalIndex);
                        yield return new CodeInstruction(OpCodes.Ldloc, caster.LocalIndex);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Thing), nameof(Thing.Map)).GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Ldloc, caster.LocalIndex);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Thing), nameof(Thing.Position)).GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_CastPositionFinder), nameof(CalculateShieldPreference)));
                        skipReturn = true;
                    }
                    else
                    {
                        if (instruction.opcode == OpCodes.Ldc_R4)
                        {
                            var result = instruction.operand as float?;
                            if (result != null)
                            {
                                skipReturn = result < 0;
                            }
                            else
                            {
                                skipReturn = false;
                            }
                        }
                        else
                        {
                            skipReturn = false;
                        }
                    }
                    
                    yield return instruction;
                }
            }
        }
    }
}

