using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SRTS;
using Verse;

namespace FrontierDevelopments.Shields.SrtsExpandedIntegration.Harmony
{
    public class Harmony_SRTSStatic
    {
        private class Patcher
        {
            private LocalBuilder faction;

            private bool storedFaction = false;
            private int swappedTargetCounts = 0;

            public Patcher(ILGenerator il)
            {
                faction = il.DeclareLocal(typeof(Faction));
            }

            public IEnumerable<CodeInstruction> Apply(IEnumerable<CodeInstruction> instructions)
            {
                var original = instructions.ToList();
                var patched = SwapTargeterCalls(AddStoreShipFaction(original)).ToList();
                
                if (storedFaction && swappedTargetCounts > 0)
                {
                    return patched;
                }
                else
                {
                    Log.Warning("FrontierDevelopments Shields SRTS Expanded Support: Unable to add support for ships landing in shields");
                    return original;
                }
            }

            private IEnumerable<CodeInstruction> AddStoreShipFaction(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Ldfld
                        && (instruction.operand as FieldInfo)?.FieldType == typeof(Caravan))
                    {
                        storedFaction = true;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Dup);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_SRTSStatic), nameof(Harmony_SRTSStatic.GetCaravanFaction)));
                        yield return new CodeInstruction(OpCodes.Stloc, faction);
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }

            private IEnumerable<CodeInstruction> SwapTargeterCalls(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Call
                        && (MethodInfo) instruction.operand == AccessTools.Method(
                            typeof(TargetingParameters),
                            nameof(TargetingParameters.ForDropPodsDestination)))
                    {
                        swappedTargetCounts++;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldloc, faction);
                        yield return new CodeInstruction(
                            OpCodes.Call,
                            AccessTools.Method(
                                typeof(ShieldTargetingParams),
                                nameof(ShieldTargetingParams.ForShip)));
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }

        private static Faction GetCaravanFaction(Caravan caravan)
        {
            if (caravan == null)
            {
                return Faction.OfPlayer;
            }

            return caravan.Faction;
        }

        [HarmonyPatch]
        static class Patch_Targeter
        {
            [HarmonyTargetMethod]
            private static MethodInfo Target(HarmonyLib.Harmony instance)
            {
                return typeof(SRTSStatic)
                    .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static)
                    .SelectMany(AccessTools.GetDeclaredMethods)
                    .Where(method => method.Name.Contains("GetMapParent"))
                    .First(method => method.ReturnType == typeof(void) && method.GetParameters().Length == 0);
            }

            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> UseShieldTargeter(IEnumerable<CodeInstruction> instructions,
                ILGenerator il)
            {
                var patcher = new Patcher(il);
                return patcher.Apply(instructions);
            }
        }
    }
}