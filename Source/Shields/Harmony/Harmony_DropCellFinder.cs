using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Harmony_DropCellFinder
    {
        [HarmonyPatch(typeof(DropCellFinder), "CanPhysicallyDropInto")]
        static class Patch_DropCellFinder_CanPhysicallyDropInto
        {
            static bool Prefix(out bool __result, IntVec3 c, Map map, bool canRoofPunch)
            {
                if (Mod.ShieldManager.Shielded(map, Common.ToVector3(c)))
                {
                    __result = false;
                    return false;
                }

                __result = true;
                return true;
            }
        }
    }
}