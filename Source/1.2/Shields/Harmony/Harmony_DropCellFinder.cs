using FrontierDevelopments.General;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_DropCellFinder
    {
        private static bool CanPhysicallyDropIntoShielded(Map map, IntVec3 c)
        {
            return !map.GetComponent<ShieldManager>().Shielded(PositionUtility.ToVector3(c), false);
        }
        
        [HarmonyPatch(typeof(DropCellFinder), "CanPhysicallyDropInto")]
        static class Patch_CanPhysicallyDropInto
        {
            [HarmonyPostfix]
            private static bool AddShieldCheck(bool __result, IntVec3 c, Map map, bool canRoofPunch)
            {
                if (__result)
                {
                    return CanPhysicallyDropIntoShielded(map, c);
                }
                return false;
            }
        }
    }
}