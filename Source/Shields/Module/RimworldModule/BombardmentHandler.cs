using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Module.RimworldModule
{
    public class BombardmentHandler
    {
        private static bool ShouldStop(Map map, IntVec3 center)
        {
            return Mod.ShieldManager.Block(map, Common.ToVector3(center), Mod.Settings.SkyfallerDamage);
        }

        private static bool IsShielded(Map map, IntVec3 position)
        {
            return Mod.ShieldManager.Shielded(map, Common.ToVector3(position));
        }
        
        [HarmonyPatch(typeof(Bombardment), "CreateRandomExplosion")]
        static class Patch_Bombardment_CreateRandomExplosion
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var patchPhase = 0;
                
                foreach (var instruction in instructions)
                {
                    switch (patchPhase)
                    {
                        // search for call to get_Map
                        case 0:
                        {
                            if (instruction.opcode == OpCodes.Call && (instruction.operand as MethodInfo).Name == "get_Map")
                            {
                                patchPhase = 1;
                            }
                            break;
                        }
                        // find loading map into locals next
                        case 1:
                        {
                            if (instruction.opcode == OpCodes.Stloc_3)
                            {
                                patchPhase = 2;
                            }
                            break;
                        }
                        // insert check call
                        case 2:
                        {
                            var continueLabel = il.DefineLabel();
                            instruction.labels.Add(continueLabel);
                            
                            yield return new CodeInstruction(OpCodes.Ldloc_3);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BombardmentHandler), nameof(ShouldStop)));
                            yield return new CodeInstruction(OpCodes.Brfalse, continueLabel);
                            yield return new CodeInstruction(OpCodes.Ret);
                            
                            patchPhase = -1;
                            break;
                        }
                    }
                    
                    yield return instruction;
                }
            }
        }

        [HarmonyPatch(typeof(Bombardment), "StartRandomFire")]
        static class Patch_Bombardment_StartRandomFire
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var patchPhase = 0;

                foreach (var instruction in instructions)
                {
                    switch (patchPhase)
                    {
                        // find get_Map
                        case 0:
                        {
                            if (instruction.opcode == OpCodes.Call && (instruction.operand as MethodInfo).Name == "get_Map")
                            {
                                patchPhase = 1;
                            }
                            break;
                        }
                        // add check
                        case 1:
                        {
                            var continueLabel = il.DefineLabel();
                            var localMap = il.DeclareLocal(typeof(Map));

                            yield return new CodeInstruction(OpCodes.Stloc, localMap.LocalIndex);
                            yield return new CodeInstruction(OpCodes.Ldloc, localMap.LocalIndex);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BombardmentHandler), nameof(IsShielded)));
                            yield return new CodeInstruction(OpCodes.Brfalse, continueLabel);
                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ret);
                            yield return new CodeInstruction(OpCodes.Ldloc, localMap.LocalIndex) { labels =  new List<Label>(new [] { continueLabel })};
                            patchPhase = -1;
                            break;
                        }
                    }

                    yield return instruction;
                }
            }
        }
    }
}
