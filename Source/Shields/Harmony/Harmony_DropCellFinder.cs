using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_DropCellFinder
    {
        private static bool CheckShielded(Map map, IntVec3 c)
        {
            return map.GetComponent<ShieldManager>().Shielded(PositionUtility.ToVector3(c), false);
        }
        
        [HarmonyPatch(typeof(DropCellFinder), "CanPhysicallyDropInto")]
        static class Patch_CanPhysicallyDropInto
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var patchPhase = 0;
                Label? successLabel = null;
                var shieldTestLabel = il.DefineLabel();

                foreach(var instruction in instructions)
                {
                    switch (patchPhase)
                    {
                        // find GetRoof
                        case 0:
                            
                            if (instruction.opcode == OpCodes.Call 
                                && instruction.operand == AccessTools.Method(typeof(GridsUtility), nameof(GridsUtility.GetRoof)))
                            {
                                patchPhase = 1;
                            }
                            break;
                        // find the success label, replace it with shield test
                        case 1:
                            if (instruction.opcode == OpCodes.Brfalse)
                            {
                                successLabel = instruction.operand as Label?;
                                instruction.operand = shieldTestLabel;
                                patchPhase = 2;
                            }
                            break;
                        // find thick roof result
                        case 2:
                            if(instruction.opcode == OpCodes.Ldfld 
                               && instruction.operand as FieldInfo == AccessTools.Field(typeof(RoofDef), nameof(RoofDef.isThickRoof)))
                            {
                                patchPhase = 3;
                            }
                            break;
                        // replace the label
                        case 3:
                            if (instruction.opcode == OpCodes.Brfalse)
                            {
                                instruction.operand = shieldTestLabel;
                                patchPhase = 4;
                            }
                            break;
                        // find original return true reference
                        case 4:
                            if (instruction.opcode == OpCodes.Ldc_I4_0)
                            {
                                yield return new CodeInstruction(OpCodes.Ldarg_1)
                                {
                                    labels = new List<Label>(new Label[] { shieldTestLabel })
                                };
                                yield return new CodeInstruction(OpCodes.Ldarg_0);
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_DropCellFinder), nameof(CheckShielded)));
                                yield return new CodeInstruction(OpCodes.Brfalse, successLabel);
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Ret);

                                // done patching
                                patchPhase = -1;
                            }
                            break;
                    }
                    yield return instruction;
                }
            }
        }
    }
}