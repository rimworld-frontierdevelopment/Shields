using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    class Harmony_PowerBeam : Harmony_OrbitalStrike
    {
        [HarmonyPatch(typeof(PowerBeam), "StartRandomFireAndDoFlameDamage")]
        static class Patch_StartRandomFire
        {
            private static void La()
            {
                if (1 > Rand.Int) return;
                Log.Message("do a thing");
                return;
            }
            
            // search for:
            //
            //   IntVec3 c = GenRadial.RadialCellsAround(...).Where<IntVec3>(...);
            //   FireUtility.TryStartFireIn(c, ...);
            //
            // transform it into:
            //
            //   IntVec3 c = GenRadial.RadialCellsAround(...).Where<IntVec3>(...);
            //   if(Harmony_OrbitalStrike.ShouldStop(...)) return;
            //   FireUtility.TryStartFireIn(c, ...);
            //
            private class Transpiler_ProtectShieldedThings
            {
                private bool Success { get; set; }

                private IEnumerable<CodeInstruction> AddShieldCheck(IEnumerable<CodeInstruction> instructions, ILGenerator il)
                {
                    var continueLabel = il.DefineLabel();

                    foreach (var instruction in instructions)
                    {
                        if (Success == false && instruction.opcode == OpCodes.Stloc_0)
                        {
                            Success = true;
                            yield return instruction;

                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(PowerBeam), nameof(PowerBeam.Map)).GetGetMethod());
                            yield return new CodeInstruction(
                                OpCodes.Call,
                                AccessTools.Method(
                                    typeof(Harmony_OrbitalStrike), 
                                    nameof(ShouldStopLowDamage)));
                            yield return new CodeInstruction(OpCodes.Brfalse, continueLabel);
                            yield return new CodeInstruction(OpCodes.Ret);
                            
                            // restore the stack to its original state
                            yield return new CodeInstruction(OpCodes.Nop)
                            {
                                labels = new List<Label>(new []{ continueLabel })
                            };
                        }
                        else
                        {
                            yield return instruction;
                        }
                    }
                }

                public IEnumerable<CodeInstruction> Apply(
                    IReadOnlyCollection<CodeInstruction> instructions,
                    ILGenerator il)
                {
                    var patched = AddShieldCheck(instructions, il).ToList();
                    if (Success)
                    {
                        return patched;
                    }
                    
                    Log.Warning("Failed patching PowerBeam.StartRandomFireAndDoFlameDamage, protecting pawns and stopping in range is disabled");
                    return instructions;
                }
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> AddShieldCheck(
                IEnumerable<CodeInstruction> instructions,
                ILGenerator il)
            {
                return new Transpiler_ProtectShieldedThings().Apply(instructions.ToList(), il);
            }
        }
    }
}