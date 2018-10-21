using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields
{
    public class Harmony_DropCellFinder
    {
        public static bool IsGoodDropSpot(IntVec3 c, Map map, bool allowFogged, bool canRoofPunch)
        {
            return DropCellFinder.IsGoodDropSpot(c, map, allowFogged, canRoofPunch)
               && !Mod.ShieldManager.Shielded(map, Common.ToVector2(c));
        }

        public static bool AnyAdjacentGoodDropSpot(IntVec3 c, Map map, bool allowFogged, bool canRoofPunch)
        {
          if (!IsGoodDropSpot(c + IntVec3.North, map, allowFogged, canRoofPunch)
              && !IsGoodDropSpot(c + IntVec3.East, map, allowFogged, canRoofPunch)
              && !IsGoodDropSpot(c + IntVec3.South, map, allowFogged, canRoofPunch))
            return IsGoodDropSpot(c + IntVec3.West, map, allowFogged, canRoofPunch);
          return true;
        }
      
        public static bool TryFindDropSpotNear(IntVec3 center, Map map, out IntVec3 result, bool allowFogged, bool canRoofPunch)
        {
          if (DebugViewSettings.drawDestSearch)
            map.debugDrawer.FlashCell(center, 1f, nameof (center), 50);
          Predicate<IntVec3> validator = (c => IsGoodDropSpot(c, map, allowFogged, canRoofPunch) 
                                               && map.reachability.CanReach(center, (LocalTargetInfo) c, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly));
          var squareRadius = 5;
          while (!CellFinder.TryFindRandomCellNear(center, map, squareRadius, validator, out result))
          {
            squareRadius += 3;
            if (squareRadius > 16)
            {
              result = center;
              return false;
            }
          }
          return true;
        }

        [HarmonyPatch(typeof(DropCellFinder), "TradeDropSpot")]
        static class ShieldManager_DropCellFinder_TradeDropSpot
        {
            static bool Prefix(out IntVec3 __result, Map map)
            {
                IEnumerable<Building> collection = map.listerBuildings.allBuildingsColonist.Where(b => b.def.IsCommsConsole);
                IEnumerable<Building> buildings = map.listerBuildings.allBuildingsColonist.Where(b => b.def.IsOrbitalTradeBeacon);
                Building building = buildings.FirstOrDefault(b =>
                  !map.roofGrid.Roofed(b.Position)
                    ? AnyAdjacentGoodDropSpot(b.Position, map, false, false)
                    : false);
                if (building != null)
                {
                  var position = building.Position;
                  IntVec3 result;
                  if (TryFindDropSpotNear(position, map, out result, false, false))
                  {
                    Log.Error("Could find no good TradeDropSpot near dropCenter " + position + ". Using a random standable unfogged cell.");
                    __result = CellFinderLoose.RandomCellWith(c =>
                    {
                      if (c.Standable(map))
                        return !c.Fogged(map);
                      return false;
                    }, map);
                  }
                  __result = result;
                  return false;
                }
                var remainingBuildings = new List<Building>();
                remainingBuildings.AddRange(buildings);
                remainingBuildings.AddRange(collection);
                remainingBuildings.RemoveAll(b =>
                {
                  var comp = b.TryGetComp<CompPowerTrader>();
                  return comp != null ? !comp.PowerOn : false;
                });
                var validator = (Predicate<IntVec3>) (c => IsGoodDropSpot(c, map, false, false));
                if (!remainingBuildings.Any())
                {
                  remainingBuildings.AddRange(map.listerBuildings.allBuildingsColonist);
                  remainingBuildings.Shuffle();
                  if (!remainingBuildings.Any())
                  {
                    __result = CellFinderLoose.RandomCellWith(validator, map);
                    return false;
                  }
                }
                var squareRadius = 8;
                do
                {
                  for (var index = 0; index < remainingBuildings.Count; ++index)
                  {
                    IntVec3 result;
                    if (CellFinder.TryFindRandomCellNear(remainingBuildings[index].Position, map, squareRadius,
                          validator, out result)
                        && !Mod.ShieldManager.Shielded(map, Common.ToVector2(result)))
                    {
                      __result = result;
                      return false;
                    }
                  }
                  squareRadius = Mathf.RoundToInt(squareRadius * 1.1f);
                }
                while (squareRadius <= map.Size.x);
                Log.Error("Failed to generate trade drop center. Giving random.");
                __result = CellFinderLoose.RandomCellWith(validator, map);
                return false;
            }
        }
    }
}