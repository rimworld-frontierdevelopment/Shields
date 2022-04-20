using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_CellFinderLoose
    {
        private static float HandleShieldCheck(Map map, IntVec3 cell, Thing threat, float num)
        {
            if (Mod.Settings.EnableAIFleeToShields) return num;
            if (threat == null) return num;
            if (IsShieldedFrom(map, cell, threat))
            {
                return num * 2f;
            }

            return num;
        }

        private static bool IsShieldedFrom(Map map, IntVec3 cell, Thing threat)
        {
            return new FieldQuery(map)
                .IsActive()
                .Intersects(
                    PositionUtility.ToVector3(threat.Position).Yto0(),
                    PositionUtility.ToVector3(cell).Yto0())
                .Get()
                .Any();
        }

        [HarmonyPatch]
        static class Patch_GetFleeDestToolUser
        {
            [HarmonyTargetMethod]
            static MethodInfo Target()
            {
                bool ParamsMatch(ParameterInfo[] parameters)
                {
                    return parameters.Length == 1 
                           && parameters.First().ParameterType == typeof(Region);
                }

                return typeof(CellFinderLoose).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic)
                    .SelectMany(AccessTools.GetDeclaredMethods)
                    .Where(method => method.Name.Contains(nameof(CellFinderLoose.GetFleeDestToolUser)))
                    .Where(method => method.ReturnType == typeof(bool))
                    .First(method => ParamsMatch(method.GetParameters()));
            }

            private class Transpiler_AddShieldCheck
            {
                private bool FoundNum { get; set; }
                private bool AddedShieldCheck { get; set; }

                private bool Success => FoundNum && AddedShieldCheck;

                private readonly ILGenerator _il;
                private object _num;

                public Transpiler_AddShieldCheck(ILGenerator il)
                {
                    _il = il;
                }

                // IL_0133: callvirt instance bool Verse.PawnDestinationReservationManager::CanReserve(valuetype Verse.IntVec3, class Verse.Pawn, bool)
                private static bool IsReservationManagerInstruction(CodeInstruction instruction)
                {
                    return instruction.opcode == OpCodes.Callvirt
                           && (MethodInfo) instruction.operand == AccessTools.Method(
                               typeof(PawnDestinationReservationManager),
                               nameof(PawnDestinationReservationManager.CanReserve));
                }

                private static bool IsNum(CodeInstruction current, CodeInstruction last)
                {
                    return current.opcode == OpCodes.Stloc_S
                           && last?.opcode == OpCodes.Mul;
                }

                private IEnumerable<CodeInstruction> FindNum(IEnumerable<CodeInstruction> instructions)
                {
                    CodeInstruction last = null;
                    foreach (var current in instructions)
                    {
                        if (!FoundNum && IsNum(current, last))
                        {
                            FoundNum = true;
                            _num = current.operand;
                        }

                        last = current;
                        yield return current;
                    }
                }

                private bool ShouldAddCheck(CodeInstruction current, CodeInstruction last)
                {
                    return current.opcode == OpCodes.Ldloc_S && current.operand == _num &&
                           last?.opcode == OpCodes.Stloc_S && last.operand == _num;
                }

                private IEnumerable<CodeInstruction> AddShieldCheck(IEnumerable<CodeInstruction> instructions)
                {
                    CodeInstruction last = null;
                    foreach (var current in instructions)
                    {
                        if (AddedShieldCheck == false && ShouldAddCheck(current, last))
                        {
                            AddedShieldCheck = true;

                            yield return new CodeInstruction(OpCodes.Ldloc_1); // map
                            yield return new CodeInstruction(OpCodes.Ldloc_3); // cell
                            yield return new CodeInstruction(OpCodes.Ldloc, 4); // threat thing 
                            yield return new CodeInstruction(OpCodes.Ldloc_S, _num);
                            yield return new CodeInstruction(
                                OpCodes.Call,
                                AccessTools.Method(
                                    typeof(Harmony_CellFinderLoose),
                                    nameof(HandleShieldCheck)));
                            yield return new CodeInstruction(OpCodes.Stloc_S, _num);
                        }
                        yield return current;
                        last = current;
                    }
                }

                public IEnumerable<CodeInstruction> Apply(IReadOnlyCollection<CodeInstruction> instructions)
                {
                    var patched = AddShieldCheck(FindNum(instructions)).ToList();
                    if (Success)
                    {
                        return patched;
                    }

                    Log.Warning("Failed patching CellFinderLoose.GetFleeDestToolUser, pawns won't flee to shielded areas");
                    return instructions;
                }
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> AddShieldCheck(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                return new Transpiler_AddShieldCheck(il).Apply(instructions.ToList());
            }
        }
    }
}