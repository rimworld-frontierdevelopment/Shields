using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields
{
    [HarmonyPatch(typeof(Verb), "CanHitTargetFrom" , new Type[] { typeof(IntVec3), typeof(LocalTargetInfo) })]
    public class Harmony_Verb_CanHitCellFromCellIgnoringRange
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var patchPhase = 0;
            
            foreach (var instruction in instructions)
            {
                if (patchPhase == 1)
                {
                    var continueLabel = il.DefineLabel();
                    instruction.labels.Add(continueLabel);
                    
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Verb), "caster"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_Verb_CanHitCellFromCellIgnoringRange), "ShieldBlocks", new Type[]{ typeof(Thing), typeof(IntVec3), typeof(LocalTargetInfo) }));
                    yield return new CodeInstruction(OpCodes.Brfalse, continueLabel);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ret);

                    patchPhase = -1;
                }
                
                // find end of loop
                if (patchPhase == 0 && instruction.opcode == OpCodes.Blt)
                {
                    patchPhase = 1;
                }
                
                yield return instruction;
            }
        }

        static bool ShieldBlocks(Thing caster, IntVec3 source, LocalTargetInfo target)
        {
            if (caster.Faction != Faction.OfPlayer) return false;
            return Mod.ShieldManager.Shielded(caster.Map, Common.ToVector3(source), Common.ToVector3(target.Cell));
        }
    }
}