using FrontierDevelopments.General;
using HarmonyLib;
using System.Linq;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_CastPositionFinder
    {
        private static float CalculateShieldPreference(float score, ShieldManager manager, IntVec3 start, IntVec3 end)
        {
            var startAdjusted = PositionUtility.ToVector3(start);
            var endAdjusted = PositionUtility.ToVector3(end);

            var targetProtected = manager.WhichShielded(startAdjusted, endAdjusted).ToList();
            var shooterProtected = manager.WhichShielded(endAdjusted, startAdjusted).ToList();
            var shooterUnderShield = manager.WhichShielded(startAdjusted).ToList();

            if (shooterProtected.Any())
            {
                score *= 1 + shooterProtected.Count;
            }

            if (shooterUnderShield.Any())
            {
                score *= 1 + shooterUnderShield.Count;
            }

            if (targetProtected.Any())
            {
                score /= 1 + targetProtected.Count;
            }
            return score;
        }

        [HarmonyPatch(typeof(CastPositionFinder), "CastPositionPreference")]
        private static class Patch_CastPositionPreference
        {
            [HarmonyPostfix]
            private static float AdjustScoreFromShielding(float __result, IntVec3 c, CastPositionRequest ___req)
            {
                var shieldManager = ___req.caster.Map.GetComponent<ShieldManager>();
                if (shieldManager.ShieldCount() == 0) return __result;

                return CalculateShieldPreference(
                    __result,
                    shieldManager,
                    c,
                    ___req.target.Position);
            }
        }
    }
}