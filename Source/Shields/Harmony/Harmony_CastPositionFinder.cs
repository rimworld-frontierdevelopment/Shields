using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields
{
    public class Harmony_CastPositionFinder
    {
        private static float CalculateShieldPreference(float score, Map map, IntVec3 start, IntVec3 end)
        {
            if (Mod.ShieldManager.Shielded(map, Common.ToVector3(start), Common.ToVector3(end)))
            {
                score = (float)Math.Sqrt(score);
            }

            if (Mod.ShieldManager.Shielded(map, Common.ToVector3(end), Common.ToVector3(start)))
            {
                score = (float) Math.Pow(score, 2);
            }
            return score;
        }
        
        [HarmonyPatch(typeof(CastPositionFinder), "CastPositionPreference")]
        static class Patch_CastPositionFinder_CastPositionPreference
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
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Thing), "get_Map"));
                        yield return new CodeInstruction(OpCodes.Ldloc, caster.LocalIndex);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Thing), "get_Position"));
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

