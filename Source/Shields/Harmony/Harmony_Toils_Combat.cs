using System;
using FrontierDevelopments.Shields.AI;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields
{
    public class Harmony_Toils_Combat
    {
        [HarmonyPatch(typeof(Toils_Combat), "GotoCastPosition")]
        static class Patch_Toils_Combat_GotoCastPosition
        {
            static bool Prefix(out Toil __result, TargetIndex targetInd, bool closeIfDowned = false, float maxRangeFactor = 1f)
            {
                Log.Message("detoured go to cast position");
                __result = GotoCastPosition(targetInd, closeIfDowned, maxRangeFactor);
                return false;
            }
        }

        private static Toil GotoCastPosition(TargetIndex targetInd, bool closeIfDowned = false, float maxRangeFactor = 1f)
        {
            Toil toil = new Toil();
            toil.initAction = (Action) (() =>
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(targetInd).Thing;
                Pawn pawn = thing as Pawn;
                IntVec3 dest;
                
                Log.Message("cast from " + actor.Position + ", to " + thing.Position);
                
                if (!ShieldCastPositionFinder.TryFindCastPosition(new ShieldCastPositionRequest()
                {
                    caster = toil.actor,
                    target = thing,
                    verb = curJob.verbToUse,
                    maxRangeFromTarget = !closeIfDowned || pawn == null || !pawn.Downed ? Mathf.Max(curJob.verbToUse.verbProps.range * maxRangeFactor, 1.42f) : Mathf.Min(curJob.verbToUse.verbProps.range, (float) pawn.RaceProps.executionRange),
                    wantCoverFromTarget = false,
                    // TODO this should also include friendlies
                    avoidShields = actor.Faction == Faction.OfPlayer
                }, out dest))
                {
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                }
                else
                {
                    toil.actor.pather.StartPath((LocalTargetInfo) dest, PathEndMode.OnCell);
                    actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, dest);
                }
            });
            toil.FailOnDespawnedOrNull<Toil>(targetInd);
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }
    }
}

