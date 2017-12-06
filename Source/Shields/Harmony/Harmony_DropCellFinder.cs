using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Harmony_DropCellFinder
    {
        public static bool AnyAdjacentGoodDropSpot(IntVec3 c, Map map, bool allowFogged, bool canRoofPunch)
        {
          if (!DropCellFinder.IsGoodDropSpot(c + IntVec3.North, map, allowFogged, canRoofPunch) && !DropCellFinder.IsGoodDropSpot(c + IntVec3.East, map, allowFogged, canRoofPunch) && !DropCellFinder.IsGoodDropSpot(c + IntVec3.South, map, allowFogged, canRoofPunch))
            return DropCellFinder.IsGoodDropSpot(c + IntVec3.West, map, allowFogged, canRoofPunch);
          return true;
        }

        [HarmonyPatch(typeof(DropCellFinder), "TradeDropSpot")]
        static class ShieldManager_DropCellFinder_TradeDropSpot
        {
            static bool Prefix(out IntVec3 __result, Map map)
            {
                Log.Message("running patched TradeDropSpot");
              
                IEnumerable<Building> collection = map.listerBuildings.allBuildingsColonist.Where(b => b.def.IsCommsConsole);
                IEnumerable<Building> buildings = map.listerBuildings.allBuildingsColonist.Where(b => b.def.IsOrbitalTradeBeacon);
                Building building = buildings.FirstOrDefault(b =>
                  !map.roofGrid.Roofed(b.Position)
                    ? AnyAdjacentGoodDropSpot(b.Position, map, false, false)
                    : false);
                if (building != null)
                {
                  var position = building.Position;
                  if (!DropCellFinder.TryFindDropSpotNear(position, map, out __result, false, false))
                  {
                    Log.Error("Could find no good TradeDropSpot near dropCenter " + position + ". Using a random standable unfogged cell.");
                    __result = CellFinderLoose.RandomCellWith(c =>
                    {
                      if (c.Standable(map))
                        return !c.Fogged(map);
                      return false;
                    }, map);
                  }
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
                var validator = (Predicate<IntVec3>) (c => 
                  DropCellFinder.IsGoodDropSpot(c, map, false, false)
                  && !Mod.ShieldManager.ImpactShield(map, Common.ToVector2(c)));
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
                    if (CellFinder.TryFindRandomCellNear(remainingBuildings[index].Position, map, squareRadius, validator, out __result))
                      return false;
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