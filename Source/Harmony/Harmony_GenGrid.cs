using Harmony;
using Verse;
using UnityEngine;

namespace FrontierDevelopments.Harmony
{
    public class Harmony_GenGrid
    {
        [HarmonyPatch(typeof(GenGrid), nameof(GenGrid.InBounds), new []{ typeof(IntVec3), typeof(Map)})]
        static class Patch_InBounds_IntVec3
        {
            [HarmonyPrefix]
            static bool Prefix(out bool __result, IntVec3 c, Map map)
            {
                __result = false;
                return map != null;
            }
        }

        [HarmonyPatch(typeof(GenGrid), nameof(GenGrid.InBounds), new []{ typeof(Vector3), typeof(Map)})]
        static class Patch_InBounds_Vector3
        {
            [HarmonyPrefix]
            static bool Prefix(out bool __result, Vector3 v, Map map)
            {
                __result = false;
                return map != null;
            }
        }
    }
}