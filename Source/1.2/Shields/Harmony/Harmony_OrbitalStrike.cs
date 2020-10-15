using System;
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
    class Harmony_OrbitalStrike
    {
        private static readonly MethodInfo canStartFleeingBecauseOfPawnAction = AccessTools.Method(typeof(Pawn_MindState), "CanStartFleeingBecauseOfPawnAction");

        protected static bool ShouldStopLowDamage(IntVec3 center, Map map)
        {
            return new FieldQuery(map)
                .IsActive()
                .Intersects(PositionUtility.ToVector3(center))
                .Block(Mod.Settings.DropPodDamage / 8f);
        }

        protected static bool ShouldStopHighDamage(IntVec3 center, Map map)
        {
            return new FieldQuery(map)
                .IsActive()
                .Intersects(PositionUtility.ToVector3(center))
                .Block(Mod.Settings.SkyfallerDamage);
        }

        protected static bool ShouldStopHighDamageWithSmokeExplosion(IntVec3 center, Thing instigator)
        {
            var result = ShouldStopHighDamage(center, instigator.Map);
            if (result)
            {
                ExplosionUtility.DoSmokeExplosion(instigator, center, instigator.Map, DamageDefOf.Bomb.soundExplosion);
            }

            return result;
        }

        protected static bool IsShielded(IntVec3 position, Map map)
        {
            return new FieldQuery(map)
                .IsActive()
                .Intersects(PositionUtility.ToVector3(position))
                .Get()
                .Any();
        }

        // search for:
        //
        //    IntVec3 c = GenRadial.RadialCellsAround(...).Where(...);
        //    ...
        //    FireUtility.TryStartFireIn(...);
        //
        // transform it into:
        //    
        //    IntVec3 c = GenRadial.RadialCellsAround(...).Where(...);
        //    if(!Harmony_OrbitalStrike.ShouldStop(...)) {
        //      FireUtility.TryStartFireIn(...);
        //    }
        protected class Transpiler_TryStartFireIn
        {
            private readonly Label _skipFire;
            
            private bool _targetCellFound = false;
            private bool _addedSkipFireLabel = false;

            private bool Success => _targetCellFound && _addedSkipFireLabel;

            public Transpiler_TryStartFireIn(ILGenerator il)
            {
                _skipFire = il.DefineLabel();
            }

            private IEnumerable<CodeInstruction> AddShieldCheck(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var targetCell = il.DeclareLocal(typeof(IntVec3));
                
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Call
                        && (MethodInfo) instruction.operand == AccessTools.Method(
                            typeof(GenCollection),
                            nameof(GenCollection.RandomElementByWeight), 
                            generics: new Type[] {typeof(IntVec3)}))
                    {
                        _targetCellFound = true;

                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Stloc, targetCell);

                        // the shield check
                        yield return new CodeInstruction(OpCodes.Ldloc, targetCell);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Bombardment), nameof(Bombardment.Map)).GetGetMethod());
                        yield return new CodeInstruction(
                            OpCodes.Call,
                            AccessTools.Method(
                                typeof(Harmony_OrbitalStrike), 
                                nameof(ShouldStopLowDamage)));
                        yield return new CodeInstruction(OpCodes.Brtrue, _skipFire);

                        // restore the stack to its original state
                        yield return new CodeInstruction(OpCodes.Ldloc, targetCell);
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }

            private IEnumerable<CodeInstruction> AddShieldCheckLabel(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    // bool RimWorld.FireUtility::TryStartFireIn(valuetype Verse.IntVec3, class Verse.Map, float32)
                    if (instruction.opcode == OpCodes.Call
                        && (MethodInfo)instruction.operand == AccessTools.Method(
                            typeof(FireUtility), 
                            nameof(FireUtility.TryStartFireIn)))
                    {
                        _addedSkipFireLabel = true;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Nop)
                        {
                            labels = new List<Label>(new []{ _skipFire })
                        };
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }

            public IEnumerable<CodeInstruction> Apply(string methodName, IReadOnlyCollection<CodeInstruction> instructions, ILGenerator il)
            {
                var patched = AddShieldCheck(AddShieldCheckLabel(instructions), il).ToList();
                if (Success)
                {
                    return patched;
                }

                Log.Warning("Failed patching " + methodName + ", blocking fires is disabled");
                return instructions;
            }
        }

        [HarmonyPatch(typeof(OrbitalStrike), nameof(OrbitalStrike.Tick))]
        private static class Patch_Tick
        {
            private static bool CanStartFleeingBecauseOfPawnAction(Pawn p)
            {
                return !p.Downed
                       && !p.Drafted
                       && p.jobs.curJob.def != JobDefOf.Flee 
                       && p.jobs.curJob.startTick != Find.TickManager.TicksGame;
            }

            [HarmonyPostfix]
            static void HavePawnsFleeFromOrbitalStrikes(OrbitalStrike __instance)
            {
                if (__instance.Destroyed) return;
                RegionTraverser.BreadthFirstTraverse(__instance.Position, __instance.Map, (from, to) => true, region =>
                {
                    region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn)
                        .OfType<Pawn>()
                        .Where(p => p.Position.DistanceTo(__instance.Position) <= 20)
                        .Where(CanStartFleeingBecauseOfPawnAction)
                        .Do(pawn => pawn.mindState.StartFleeingBecauseOfPawnAction(__instance));
                    return false;
                }, 25, RegionType.Set_All);
            }
        }
    }
}