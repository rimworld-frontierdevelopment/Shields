using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_Bombardment
    {
        private const float PawnFleeDistance = 24f;

        private static bool ShouldStop(Map map, IntVec3 center)
        {
            return new ShieldQuery(map)
                .IsActive()
                .Intersects(PositionUtility.ToVector3(center))
                .Block(Mod.Settings.SkyfallerDamage) != null;
        }

        private static bool IsShielded(Map map, IntVec3 position)
        {
            return new ShieldQuery(map)
                .IsActive()
                .Intersects(PositionUtility.ToVector3(position))
                .Get()
                .Any();
        }
        
        [HarmonyPatch(typeof(Bombardment), "GetNextExplosionCell")]
        static class Patch_CreateRandomExplosion
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
                            if (instruction.opcode == OpCodes.Call && instruction.operand as MethodInfo == AccessTools.Property(typeof(Thing), nameof(Thing.Map)).GetGetMethod())
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
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_Bombardment), nameof(ShouldStop)));
                            yield return new CodeInstruction(OpCodes.Brfalse, continueLabel);
                            yield return new CodeInstruction(OpCodes.Ret);
                            
                            patchPhase = -1;
                            break;
                        }
                    }
                    
                    yield return instruction;
                }
                
                if (patchPhase > 0)
                {
                    Log.Error("Patch for Bombardment.GetNextExplosionCell failed. Reached patchPhase: " + patchPhase);
                }
            }
        }

        [HarmonyPatch(typeof(Bombardment), "StartRandomFire")]
        static class Patch_StartRandomFire
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
                            if (instruction.opcode == OpCodes.Call && instruction.operand as MethodInfo == AccessTools.Property(typeof(Thing), nameof(Thing.Map)).GetGetMethod())
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
                            var localCoord = il.DeclareLocal(typeof(IntVec3));
        
                            yield return new CodeInstruction(OpCodes.Stloc, localMap.LocalIndex);
                            yield return new CodeInstruction(OpCodes.Stloc, localCoord.LocalIndex);
                            yield return new CodeInstruction(OpCodes.Ldloc, localMap.LocalIndex);
                            yield return new CodeInstruction(OpCodes.Ldloc, localCoord.LocalIndex);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_Bombardment), nameof(IsShielded)));
                            yield return new CodeInstruction(OpCodes.Brfalse, continueLabel);
                            yield return new CodeInstruction(OpCodes.Ret);
                            yield return new CodeInstruction(OpCodes.Ldloc, localCoord.LocalIndex)  { labels =  new List<Label>(new [] { continueLabel })};
                            yield return new CodeInstruction(OpCodes.Ldloc, localMap.LocalIndex);
                            patchPhase = -1;
                            break;
                        }
                    }
        
                    yield return instruction;
                }

                if (patchPhase > 0)
                {
                    Log.Error("Patch for Bombardment.StartRandomFire failed. Reached patchPhase: " + patchPhase);
                }
            }
        }

        [HarmonyPatch(typeof(Bombardment), nameof(Bombardment.Tick))]
        static class Patch_Tick
        {
            [HarmonyPostfix]
            static void Postfix(Bombardment __instance)
            {
                RegionTraverser.BreadthFirstTraverse(__instance.Position, __instance.Map, (from, to) => true, region =>
                {
                    region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn)
                        .Select(p => (Pawn)p)
                        .Where(p => !p.Downed && !p.Dead && !p.Drafted)
                        .Where(p => p.CurJobDef != JobDefOf.Flee && !p.Downed)
                        .Where(p => p.Position.DistanceTo(__instance.Position) <= PawnFleeDistance)
                        .ToList()
                        .ForEach(pawn =>
                        {
                            var threats = new List<Thing> { __instance };
                            var fleeDest1 = CellFinderLoose.GetFleeDest(pawn, threats, pawn.Position.DistanceTo(__instance.Position) + Bombardment.EffectiveAreaRadius);
                            pawn.jobs.StartJob(new Job(JobDefOf.Flee, fleeDest1, (LocalTargetInfo) __instance), JobCondition.InterruptOptional, null, false, true, null, new JobTag?());
                        });
                    return false;
                }, 25, RegionType.Set_All);
            }
        }
    }
}
