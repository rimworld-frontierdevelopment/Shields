using System.Linq;
using FrontierDevelopments.General;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_DropCellFinder
    {
        private static bool _enabled = true;
        
        private static bool CanPhysicallyDropIntoShielded(Map map, IntVec3 c)
        {
            var shieldProtected = new FieldQuery(map)
                .Intersects(PositionUtility.ToVector3(c))
                .Get()
                .Any();

            return !shieldProtected;
        }

        [HarmonyPatch(typeof(DropCellFinder), "CanPhysicallyDropInto")]
        static class Patch_CanPhysicallyDropInto
        {
            [HarmonyPostfix]
            private static bool AddShieldCheck(bool __result, IntVec3 c, Map map, bool canRoofPunch)
            {
                if (!_enabled) return __result;
                if (__result)
                {
                    return CanPhysicallyDropIntoShielded(map, c);
                }
                return false;
            }
        }

        // Patching the base mod to listen for when to change if the patch is enabled
        [HarmonyPatch(typeof(Mod), nameof(Mod.SetDropCellCheckEnabled))]
        static class Patch_SetDropCellCheckEnabled
        {
            [HarmonyPostfix]
            static void ListenForSetEnable(bool enabled)
            {
                _enabled = enabled;
            }
        }
    }
}