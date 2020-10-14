using HarmonyLib;
using Verse;
using ZLevels;

namespace FrontierDevelopments.Shields.ZLevelsIntegration.Harmony
{
    public class Harmony_ZLevelsManager
    {
        [HarmonyPatch(typeof(ZLevelsManager), nameof(ZLevelsManager.CreateUpperLevel))]
        static class Patch_CreateUpperLevel
        {
            [HarmonyPostfix]
            public static void AssociateShieldManager(Map __result, Map origin, IntVec3 playerStartSpot)
            {
                var manager = ShieldManager.For(origin, false);
                if (manager != null)
                {
                    manager.AssociateWithMap(__result); 
                    ShieldManager.Register(__result, manager);
                }
            }
        }
    }
}