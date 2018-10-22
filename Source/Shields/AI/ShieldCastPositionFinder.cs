using RimWorld;
using System.Collections.Generic;
using FrontierDevelopments.General;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields.AI
{
    
    // Copy of Verse.AI.CastPositionFinder
    public static class ShieldCastPositionFinder
    {
        private static IntVec3 bestSpot = IntVec3.Invalid;
        private static float bestSpotPref = 1f / 1000f;
        private static ShieldCastPositionRequest req;
        private static IntVec3 casterLoc;
        private static IntVec3 targetLoc;
        private static Verb verb;
        private static float rangeFromTarget;
        private static float rangeFromTargetSquared;
        private static float optimalRangeSquared;
        private static float rangeFromCasterToCellSquared;
        private static float rangeFromTargetToCellSquared;
        private static int inRadiusMark;
        private static ByteGrid avoidGrid;
        private static float maxRangeFromCasterSquared;
        private static float maxRangeFromTargetSquared;
        private static float maxRangeFromLocusSquared;
        private const float BaseAIPreference = 0.3f;
        private const float MinimumPreferredRange = 5f;
        private const float OptimalRangeFactor = 0.8f;
        private const float OptimalRangeFactorImportance = 0.3f;
        private const float CoverPreferenceFactor = 0.55f;

        public static bool TryFindCastPosition(ShieldCastPositionRequest newReq, out IntVec3 dest)
        {
            req = newReq;
            casterLoc = req.caster.Position;
            targetLoc = req.target.Position;
            Log.Message("target is at" + targetLoc);
            verb = req.verb;
            avoidGrid = newReq.caster.GetAvoidGrid(false);
            if (verb == null)
            {
                Log.Error(req.caster.ToString() + " tried to find casting position without a verb.",
                    false);
                dest = IntVec3.Invalid;
                return false;
            }

            if (req.maxRegions > 0)
            {
                Region region =
                    casterLoc.GetRegion(req.caster.Map, RegionType.Set_Passable);
                if (region == null)
                {
                    Log.Error("TryFindCastPosition requiring region traversal but root region is null.", false);
                    dest = IntVec3.Invalid;
                    return false;
                }

                inRadiusMark = Rand.Int;
                RegionTraverser.MarkRegionsBFS(region, (RegionEntryPredicate) null, newReq.maxRegions,
                    inRadiusMark, RegionType.Set_Passable);
                if ((double) req.maxRangeFromLocus > 0.00999999977648258)
                {
                    Region locusReg = req.locus.GetRegion(req.caster.Map,
                        RegionType.Set_Passable);
                    if (locusReg == null)
                    {
                        Log.Error("locus " + (object) req.locus + " has no region", false);
                        dest = IntVec3.Invalid;
                        return false;
                    }

                    if (locusReg.mark != inRadiusMark)
                    {
                        inRadiusMark = Rand.Int;
                        RegionTraverser.BreadthFirstTraverse(region, (RegionEntryPredicate) null,
                            (RegionProcessor) (r =>
                            {
                                r.mark = inRadiusMark;
                                ++req.maxRegions;
                                return r == locusReg;
                            }), 999999, RegionType.Set_Passable);
                    }
                }
            }

            CellRect cellRect = CellRect.WholeMap(req.caster.Map);
            if ((double) req.maxRangeFromCaster > 0.00999999977648258)
            {
                int num = Mathf.CeilToInt(req.maxRangeFromCaster);
                CellRect otherRect = new CellRect(casterLoc.x - num,
                    casterLoc.z - num, num * 2 + 1, num * 2 + 1);
                cellRect.ClipInsideRect(otherRect);
            }

            int num1 = Mathf.CeilToInt(req.maxRangeFromTarget);
            CellRect otherRect1 = new CellRect(targetLoc.x - num1,
                targetLoc.z - num1, num1 * 2 + 1, num1 * 2 + 1);
            cellRect.ClipInsideRect(otherRect1);
            if ((double) req.maxRangeFromLocus > 0.00999999977648258)
            {
                int num2 = Mathf.CeilToInt(req.maxRangeFromLocus);
                CellRect otherRect2 = new CellRect(targetLoc.x - num2,
                    targetLoc.z - num2, num2 * 2 + 1, num2 * 2 + 1);
                cellRect.ClipInsideRect(otherRect2);
            }

            bestSpot = IntVec3.Invalid;
            bestSpotPref = 1f / 1000f;
            maxRangeFromCasterSquared =
                req.maxRangeFromCaster * req.maxRangeFromCaster;
            maxRangeFromTargetSquared =
                req.maxRangeFromTarget * req.maxRangeFromTarget;
            maxRangeFromLocusSquared =
                req.maxRangeFromLocus * req.maxRangeFromLocus;
            rangeFromTarget =
                (req.caster.Position - req.target.Position).LengthHorizontal;
            rangeFromTargetSquared =
                (float) (req.caster.Position - req.target.Position)
                .LengthHorizontalSquared;
            optimalRangeSquared =
                (float) ((double) verb.verbProps.range * 0.800000011920929 *
                         ((double) verb.verbProps.range * 0.800000011920929));
            EvaluateCell(req.caster.Position);
            if ((double) bestSpotPref >= 1.0)
            {
                dest = req.caster.Position;
                return true;
            }

            float slope = -1f / CellLine
                              .Between(req.target.Position, req.caster.Position)
                              .Slope;
            CellLine cellLine = new CellLine(req.target.Position, slope);
            bool flag = cellLine.CellIsAbove(req.caster.Position);
            CellRect.CellRectIterator iterator1 = cellRect.GetIterator();
            while (!iterator1.Done())
            {
                IntVec3 current = iterator1.Current;
                if (cellLine.CellIsAbove(current) == flag && cellRect.Contains(current))
                    EvaluateCell(current);
                iterator1.MoveNext();
            }

            if (bestSpot.IsValid && (double) bestSpotPref > 0.330000013113022)
            {
                dest = bestSpot;
                return true;
            }

            CellRect.CellRectIterator iterator2 = cellRect.GetIterator();
            while (!iterator2.Done())
            {
                IntVec3 current = iterator2.Current;
                if (cellLine.CellIsAbove(current) != flag && cellRect.Contains(current))
                    EvaluateCell(current);
                iterator2.MoveNext();
            }

            if (bestSpot.IsValid)
            {
                dest = bestSpot;
                return true;
            }

            dest = casterLoc;
            return false;
        }

        private static void EvaluateCell(IntVec3 cell)
        {
            
            if ((double) maxRangeFromTargetSquared > 0.00999999977648258 &&
                (double) maxRangeFromTargetSquared < 250000.0 &&
                (double) (cell - req.target.Position).LengthHorizontalSquared >
                (double) maxRangeFromTargetSquared)
            {
                if (!DebugViewSettings.drawCastPositionSearch)
                    return;
                req.caster.Map.debugDrawer.FlashCell(cell, 0.0f, "range target", 50);
            }
            else if ((double) maxRangeFromLocusSquared > 0.01 &&
                     (double) (cell - req.locus).LengthHorizontalSquared >
                     (double) maxRangeFromLocusSquared)
            {
                if (!DebugViewSettings.drawCastPositionSearch)
                    return;
                req.caster.Map.debugDrawer.FlashCell(cell, 0.1f, "range home", 50);
            }
            else
            {
                if ((double) maxRangeFromCasterSquared > 0.00999999977648258)
                {
                    rangeFromCasterToCellSquared =
                        (float) (cell - req.caster.Position).LengthHorizontalSquared;
                    if ((double) rangeFromCasterToCellSquared >
                        (double) maxRangeFromCasterSquared)
                    {
                        if (!DebugViewSettings.drawCastPositionSearch)
                            return;
                        req.caster.Map.debugDrawer.FlashCell(cell, 0.2f, "range caster", 50);
                        return;
                    }
                }

                if (!cell.Walkable(req.caster.Map))
                    return;
                if (req.maxRegions > 0 &&
                    cell.GetRegion(req.caster.Map, RegionType.Set_Passable).mark !=
                    inRadiusMark)
                {
                    if (!DebugViewSettings.drawCastPositionSearch)
                        return;
                    req.caster.Map.debugDrawer.FlashCell(cell, 0.64f, "reg radius", 50);
                }
                else if (!req.caster.Map.reachability.CanReach(
                    req.caster.Position, (LocalTargetInfo) cell, PathEndMode.OnCell,
                    TraverseParms.For(req.caster, Danger.Some, TraverseMode.ByPawn, false)))
                {
                    if (!DebugViewSettings.drawCastPositionSearch)
                        return;
                    req.caster.Map.debugDrawer.FlashCell(cell, 0.4f, "can't reach", 50);
                }
                else
                {
                    float num1 = CastPositionPreference(cell);
                    if (avoidGrid != null)
                    {
                        byte num2 = avoidGrid[cell];
                        num1 *= Mathf.Max(0.1f, (float) ((37.5 - (double) num2) / 37.5));
                    }

                    if (DebugViewSettings.drawCastPositionSearch)
                        req.caster.Map.debugDrawer.FlashCell(cell, num1 / 4f, num1.ToString("F3"), 50);
                    if ((double) num1 < (double) bestSpotPref)
                        return;
                    if (!verb.CanHitTargetFrom(cell, (LocalTargetInfo) req.target))
                    {
                        if (!DebugViewSettings.drawCastPositionSearch)
                            return;
                        req.caster.Map.debugDrawer.FlashCell(cell, 0.6f, "can't hit", 50);
                    }
                    else if (!req.caster.Map.pawnDestinationReservationManager.CanReserve(cell,
                        req.caster, false))
                    {
                        if (!DebugViewSettings.drawCastPositionSearch)
                            return;
                        req.caster.Map.debugDrawer.FlashCell(cell, num1 * 0.9f, "resvd", 50);
                    }
                    else if (PawnUtility.KnownDangerAt(cell, req.caster.Map,
                        req.caster))
                    {
                        if (!DebugViewSettings.drawCastPositionSearch)
                            return;
                        req.caster.Map.debugDrawer.FlashCell(cell, 0.9f, "danger", 50);
                    }
                    else if (req.avoidShields && Mod.ShieldManager.Shielded(req.caster.Map, Common.ToVector3(cell), Common.ToVector3(targetLoc)))
                    {
                        if (!DebugViewSettings.drawCastPositionSearch)
                            return;
                        req.caster.Map.debugDrawer.FlashCell(cell, 0.9f, "shielded", 50);
                    }
                    else
                    {
                        bestSpot = cell;
                        bestSpotPref = num1;
                    }
                }
            }
        }

        private static float CastPositionPreference(IntVec3 c)
        {
            bool flag = true;
            List<Thing> thingList = req.caster.Map.thingGrid.ThingsListAtFast(c);
            for (int index = 0; index < thingList.Count; ++index)
            {
                Thing thing = thingList[index];
                Fire fire = thing as Fire;
                if (fire != null && fire.parent == null)
                    return -1f;
                if (thing.def.passability == Traversability.PassThroughOnly)
                    flag = false;
            }

            float num1 = 0.3f;
            if (req.caster.kindDef.aiAvoidCover)
                num1 += 8f - CoverUtility.TotalSurroundingCoverScore(c, req.caster.Map);
            if (req.wantCoverFromTarget)
                num1 += CoverUtility.CalculateOverallBlockChance((LocalTargetInfo) c,
                            req.target.Position, req.caster.Map) * 0.55f;
            float p = (req.caster.Position - c).LengthHorizontal;
            if ((double) rangeFromTarget > 100.0)
            {
                p -= rangeFromTarget - 100f;
                if ((double) p < 0.0)
                    p = 0.0f;
            }

            float num2 = num1 * Mathf.Pow(0.967f, p);
            float num3 = 1f;
            rangeFromTargetToCellSquared =
                (float) (c - req.target.Position).LengthHorizontalSquared;
            float num4 = (float) (0.699999988079071 + 0.300000011920929 *
                                  (double) (1f - Mathf.Abs(rangeFromTargetToCellSquared -
                                                           optimalRangeSquared) /
                                            optimalRangeSquared));
            float num5 = num3 * num4;
            if ((double) rangeFromTargetToCellSquared < 25.0)
                num5 *= 0.5f;
            float num6 = num2 * num5;
            if ((double) rangeFromCasterToCellSquared >
                (double) rangeFromTargetSquared)
                num6 *= 0.4f;
            if (!flag)
                num6 *= 0.2f;
            return num6;
        }
    }
}