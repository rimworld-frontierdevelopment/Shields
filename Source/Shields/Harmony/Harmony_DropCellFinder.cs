using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Harmony_DropCellFinder
    {
        [HarmonyPatch(typeof(DropCellFinder), "CanPhysicallyDropInto")]
        static class Patch_DropCellFinder_CanPhysicallyDropInto
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var patchPhase = 0;
                Label? successLabel = null;
                var shieldTestLabel = il.DefineLabel();

                foreach(var instruction in instructions)
                {
                    // find original return true reference
                    if (patchPhase == 4 && instruction.opcode == OpCodes.Ldc_I4_0)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1)
                        {
                            labels = new List<Label>(new Label[] {shieldTestLabel})
                        };
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DropCellFinder_CanPhysicallyDropInto), "CheckShielded", new Type[]{ typeof(Map), typeof(IntVec3) }));
                        yield return new CodeInstruction(OpCodes.Brfalse, successLabel);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Ret);

                        // done patching
                        patchPhase = -1;
                    }

                    // replace the label
                    if (patchPhase == 3 && instruction.opcode == OpCodes.Brfalse)
                    {
                        instruction.operand = shieldTestLabel;
                        patchPhase = 4;
                    }

                    // find thick roof result
                    if (patchPhase == 2 && instruction.opcode == OpCodes.Ldfld)
                    {
                        var fieldInfo = instruction.operand as FieldInfo;
                        if (fieldInfo != null && fieldInfo.Name.Equals("isThickRoof"))
                        {
                            patchPhase = 3;
                        }
                    }

                    // find the success label, replace it with shield test
                    if (patchPhase == 1 && instruction.opcode == OpCodes.Brfalse)
                    {
                        successLabel = instruction.operand as Label?;
                        instruction.operand = shieldTestLabel;
                        patchPhase = 2;
                    }

                    // find GetRoof
                    if (patchPhase == 0 && instruction.opcode == OpCodes.Call)
                    {
                        var methodInfo = instruction.operand as MethodInfo;
                        if (methodInfo != null && methodInfo.Name.Equals("GetRoof"))
                        {
                            patchPhase = 1;
                        }
                    }

                    yield return instruction;
                }
            }

            static bool CheckShielded(Map map, IntVec3 c)
            {
                Log.Message("check shielded at " + c);
                return Mod.ShieldManager.Shielded(map, Common.ToVector3(c), false);
            }
        }
    }
}