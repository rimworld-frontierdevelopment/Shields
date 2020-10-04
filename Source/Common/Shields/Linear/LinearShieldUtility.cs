using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Linear
{
    public static class LinearShieldUtility
    {
        public static ILinearShield Find(Thing thing)
        {
            switch (thing)
            {
                case ILinearShield shield:
                    return shield;
                case ThingWithComps thingWithComps:
                    return thingWithComps.AllComps.OfType<ILinearShield>().FirstOrDefault();
            }

            return null;
        }

        public static IEnumerable<IntVec3> CellsBetween(IntVec3 one, IntVec3 two)
        {
            return GenSight.PointsOnLineOfSight(one, two)
                .Where(cell => cell != one && cell != two);
        }

        public static IEnumerable<IntVec3> BlockingBetween(ILinearShield one, ILinearShield two)
        {
            return CellsBetween(one.Position, two.Position).Where(cell => IsCellBlocked(cell, one.Map));
        }
        
        private static bool IsCellBlocked(IntVec3 cell, Map map)
        {
            var impassable = cell.GetThingList(map)
                .Any(thing => thing.def.passability != Traversability.Standable);
            return impassable || (!cell.GetEdifice(map)?.CanBeSeenOver() ?? false);
        }

        public static bool CanLinkWith(ILinearShield one, ILinearShield two)
        {
            return one.CanLinkWith(two) && two.CanLinkWith(one) && !BlockingBetween(one, two).Any();
        }
        
        public static float CellProtectionFactorCost(float distance)
        {
            return Mathf.Pow(distance / 2, 1.5f);
        }

        public static float CalculateFieldEfficiency(float distance, int oneEfficientRange = 0, int twoEfficientRange = 0)
        {
            var adjustedDistance = distance - (oneEfficientRange + twoEfficientRange) / 2f;
            if (adjustedDistance <= 0) return 1f;
            var result =  adjustedDistance / Mathf.Pow(adjustedDistance, 1 + adjustedDistance / 100);
            return result > 1.0f ? 1.0f : result;
        }

        public static void DrawFieldBetween(List<IntVec3> cellsCovered, Map map, Color color)
        {
            GenDraw.DrawFieldEdges(cellsCovered.ToList(), color);
            cellsCovered
                .Where(cell => IsCellBlocked(cell, map))
                .Do(blocked => GenDraw.DrawFieldEdges(new List<IntVec3>(){ blocked }, Color.red));
        }

        public static void DrawFieldBetween(IntVec3 one, IntVec3 two, Map map, Color color)
        {
            DrawFieldBetween(LinearShieldUtility.CellsBetween(one, two).ToList(), map, color);
        }
    }
}