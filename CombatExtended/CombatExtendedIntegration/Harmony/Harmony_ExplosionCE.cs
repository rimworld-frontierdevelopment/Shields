using System.Collections.Generic;
using CombatExtended;
using FrontierDevelopments.Shields.Harmony;
using Harmony;
using Verse;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Harmony_ExplosionCE : Harmony_Explosion
    {
        [HarmonyPatch(typeof(ExplosionCE), nameof(ExplosionCE.Tick))]
        static class Patch_Tick
        {
            [HarmonyPrefix]
            static void HandleOuterEdgesFirst(ExplosionCE __instance, int ___startTick, List<IntVec3> ___cellsToAffect)
            {
                HandleProtected(___cellsToAffect, __instance, ___startTick);
            }
        }
    }
}