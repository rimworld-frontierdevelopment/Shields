using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_Explosion
    {
        protected static bool TryBlock(Map map, Vector3 origin, DamageDef damType, int damAmount, Vector3 position)
        {
            if (damType?.defName == null) return false;
            var damages = new ShieldDamages(new ShieldDamage(damType, damAmount));
            var blocked = new FieldQuery(map)
                .IsActive()
                .Intersects(origin, PositionUtility.ToVector3(position).Yto0())
                .Block(damages);
            return blocked;
        }

        // Runs ahead of the standard check of cells to affect
        // Finds cells that are protected from the outermost edge first
        protected static void HandleProtected<T>(
            ICollection<IntVec3> cellsToAffect,
            T explosion,
            Vector3 origin,
            int startTick,
            Func<T, IntVec3, int> getDamage) where T : Explosion
        {
            var ticksGame = Find.TickManager.TicksGame;
            cellsToAffect
                .Where(cell => ticksGame >= GetCellAffectTickReversed(explosion, startTick, cell))
                .OrderByDescending(cell => cell.DistanceTo(explosion.Position))
                .Where(cell => TryBlock(explosion.Map, origin, explosion.damType, getDamage(explosion, cell), PositionUtility.ToVector3(cell).Yto0()))
                .Do(blocked => cellsToAffect.Remove(blocked));
        }

        private static int GetCellAffectTickReversed(Explosion explosion, int startTick, IntVec3 cell)
        {
            return startTick + (int)((explosion.radius - cell.DistanceTo(explosion.Position)) * 1.5);
        }

        private static int GetDamage(Explosion explosion, IntVec3 cell)
        {
            return explosion.GetDamageAmountAt(cell);
        }

        [HarmonyPatch(typeof(Explosion), nameof(Explosion.Tick))]
        static class Patch_Tick
        {
            [HarmonyPrefix]
            static void HandleOuterEdgesFirst(Explosion __instance, List<IntVec3> ___cellsToAffect, int ___startTick)
            {
                HandleProtected(___cellsToAffect, __instance, __instance.TrueCenter().Yto0(), ___startTick, GetDamage);
            } 
        }

        [HarmonyPatch(typeof(Explosion), "AffectCell")]
        static class Patch_AffectCell
        {
            [HarmonyPrefix]
            static bool CheckCellShielded(Explosion __instance, IntVec3 c)
            {
                return !TryBlock(__instance.Map, __instance.TrueCenter().Yto0(), __instance.damType, GetDamage(__instance, c), PositionUtility.ToVector3(c).Yto0());
            }
        }
    }
}
