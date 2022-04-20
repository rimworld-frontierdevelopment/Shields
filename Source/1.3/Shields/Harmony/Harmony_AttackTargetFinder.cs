using System;
using System.Linq;
using FrontierDevelopments.General;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_AttackTargetFinder
    {
        [HarmonyPatch(typeof(AttackTargetFinder), "GetShootingTargetScore")]
        public static class Patch_GetShootingTargetScore
        {
            [HarmonyPostfix]
            static float AdjustScoreForShieldedTargets(float __result, IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
            {
                if (!Mod.Settings.EnableAIAttackTargetFinder) return __result;
                var targetShielded = new FieldQuery(searcher.Thing.Map)
                    .IsActive()
                    .Intersects(
                        PositionUtility.ToVector3(searcher.Thing.Position),
                        PositionUtility.ToVector3(target.Thing.Position))
                    .Get()
                    .Any();
                if (targetShielded)
                {
                    return __result / 2;
                }
                return __result;
            }
        }

        [HarmonyPatch(typeof(AttackTargetFinder), "CanShootAtFromCurrentPosition")]
        public static class Patch_CanShootAtFromCurrentPosition
        {
            [HarmonyPostfix]
            static bool CanShootAtShieldFromCurrentPosition(bool __result, IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
            {
                if (!Mod.Settings.EnableAIAttackTargetFinder) return __result;
                if (__result == false)
                {
                    try
                    {
                        var result = new FieldQuery(searcher.Thing.Map)
                            .IsActive()
                            .HostileTo(searcher.Thing.Faction)
                            .Intersects(
                                PositionUtility.ToVector3(searcher.Thing.Position),
                                PositionUtility.ToVector3(target.Thing.Position))
                            .GetWithIntersects()
                            .First();
                        if (result.Second == null) return false;
                        var distance = Vector3.Distance(searcher.Thing.TrueCenter(), result.Second.Value);
                        var range = verb.verbProps.range;
                        var isTarget = result.First.Emitters
                            .Select(emitter => emitter.Thing)
                            .Any(thing => thing == target.Thing);
                        if (distance <= range && isTarget)
                        {
                            return true;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
                return __result;
            }
        }
    }
}