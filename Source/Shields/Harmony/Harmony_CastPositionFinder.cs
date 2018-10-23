using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields
{
    [HarmonyPatch(typeof(CastPositionFinder), "TryFindCastPosition")]
    public class Harmony_CastPositionFinder_TryFindCastPosition
    {
        static bool Prefix(CastPositionRequest newReq)
        {
            Log.Message("TryFindCastPosition from " + newReq.caster.Position, true);
            return true;
        }
    }
    
    [HarmonyPatch(typeof(CastPositionFinder), "EvaluateCell")]
    public class Harmony_CastPositionFinder_EvaluateCell_Patch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var patchPhase = 0;

            Label? keepGoingLabel = null;
            var shieldTestLabel = il.DefineLabel();

            foreach (var instruction in instructions)
            {
                // insert code here
                if (patchPhase == 3)
                {
                    yield return new CodeInstruction(OpCodes.Ldsflda, AccessTools.Field(typeof(CastPositionFinder), "req"))
                    {
                        labels = new List<Label>(new Label[] {shieldTestLabel})
                    };
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CastPositionRequest), "caster"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(CastPositionFinder), "targetLoc"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_CastPositionFinder_EvaluateCell_Patch), "ShieldBlocks", new Type[]{ typeof(Pawn), typeof(IntVec3), typeof(IntVec3) }));
                    yield return new CodeInstruction(OpCodes.Brfalse, keepGoingLabel);
                    yield return new CodeInstruction(OpCodes.Ret);
                    
                    // bump to allow returning the current instruction
                    patchPhase++;
                }
                
                // find the next ret
                if (patchPhase == 2 && instruction.opcode == OpCodes.Ret)
                {
                    patchPhase = 3;
                }
                
                // grab the label here for where to jump to on success
                if (patchPhase == 1 && instruction.opcode == OpCodes.Brfalse)
                {
                    keepGoingLabel = instruction.operand as Label?;
                    instruction.operand = shieldTestLabel;
                    patchPhase = 2;
                }
                
                // look for PawnUtility.KnownDangerAt 
                if (patchPhase == 0 && instruction.opcode == OpCodes.Call)
                {
                    var methodInfo = instruction.operand as MethodInfo;
                    if (methodInfo != null && methodInfo.Name.Equals("KnownDangerAt"))
                    {
                        patchPhase = 1;
                    }
                } 
                
                yield return instruction;
            }
        }

        static bool ShieldBlocks(Pawn caster, IntVec3 cell, IntVec3 targetLoc)
        {
            var position = caster.Position + cell;
            if (caster.IsColonist && Mod.ShieldManager.Shielded(caster.Map, Common.ToVector3(position), Common.ToVector3(targetLoc)))
            {
                return true;
            }
            return false;
        }
    }
}
