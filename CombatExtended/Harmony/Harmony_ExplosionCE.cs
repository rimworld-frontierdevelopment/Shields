using CombatExtended;
using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Harmony_ExplosionCE : Harmony_Explosion
    {
        private static int GetDamage(ExplosionCE explosion, IntVec3 cell)
        {
            return explosion.GetDamageAmountAtCE(cell);
        }

        [HarmonyPatch(typeof(ExplosionCE), nameof(ExplosionCE.Tick))]
        private static class Patch_Tick
        {
            [HarmonyPrefix]
            private static void HandleOuterEdgesFirst(ExplosionCE __instance, int ___startTick, List<IntVec3> ___cellsToAffect)
            {
                HandleProtected(___cellsToAffect, __instance, ___startTick, GetDamage);
            }
        }
    }
}