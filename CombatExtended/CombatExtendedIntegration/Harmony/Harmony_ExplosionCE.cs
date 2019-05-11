using CombatExtended;
using FrontierDevelopments.Shields.Harmony;
using Harmony;
using Verse;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Harmony_ExplosionCE : Harmony_Explosion
    {
        [HarmonyPatch(typeof(ExplosionCE), "AffectCell")]
        static class Patch_AffectCell
        {
            [HarmonyPrefix]
            static bool Prefix(ExplosionCE __instance, IntVec3 c)
            {
                return c.InBounds(__instance.Map) && !TryBlock(__instance, __instance.damType, __instance.damAmount, c);
            }
        }
    }
}