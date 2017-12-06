using Harmony;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ShieldRenderer
    {
        [HarmonyPatch(typeof(MapDrawer), "DrawMapMesh")]
        static class Path_MapDrawer_DrawMapMesh
        {
            static void Postfix()
            {
                foreach (var shield in Mod.ShieldManager.Shields(Find.VisibleMap))
                {
                    shield.DrawShield(Find.CameraDriver.CurrentViewRect);
                }
            }
        }
    }
}