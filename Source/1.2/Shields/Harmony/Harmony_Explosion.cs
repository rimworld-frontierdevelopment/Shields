using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_Explosion
    {
        private static bool TryBlock(Thing explosion, DamageDef damType, int damAmount, IntVec3 position)
        {
            if (damType?.defName == null) return false;
            var damages = new ShieldDamages(new ShieldDamage(damType, damAmount));
            
            var blocked = new ShieldQuery(explosion.Map)
                .IsActive()
                .Intersects(explosion.TrueCenter(), PositionUtility.ToVector3(position))
                .Block(damages) != null;
            return blocked;
        }

        // Runs ahead of the standard check of cells to affect
        // Finds cells that are protected from the outermost edge first
        protected static void HandleProtected<T>(
            ICollection<IntVec3> cellsToAffect,
            T explosion,
            int startTick,
            Func<T, IntVec3, int> getDamage) where T : Explosion
        {
            var ticksGame = Find.TickManager.TicksGame;
            cellsToAffect
                .Where(cell => ticksGame >= GetCellAffectTickReversed(explosion, startTick, cell))
                .OrderByDescending(cell => cell.DistanceTo(explosion.Position))
                .Where(cell => TryBlock(explosion, explosion.damType, getDamage(explosion, cell), cell))
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
                HandleProtected(___cellsToAffect, __instance, ___startTick, GetDamage);
            } 
        }
    }
}
