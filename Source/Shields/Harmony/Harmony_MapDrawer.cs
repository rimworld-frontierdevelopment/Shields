using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_MapDrawer
    {
        [HarmonyPatch(typeof(MapDrawer), nameof(MapDrawer.DrawMapMesh))]
        private static class Patch_DrawMapMesh
        {
            private static Map last;
            private static ShieldManager manager;

            [HarmonyPostfix]
            private static void Postfix(Map ___map)
            {
                if (___map != last)
                {
                    last = ___map;
                    manager = ___map.GetComponent<ShieldManager>();
                }
                manager?.DrawShields(Find.CameraDriver.CurrentViewRect);
            }
        }
    }
}