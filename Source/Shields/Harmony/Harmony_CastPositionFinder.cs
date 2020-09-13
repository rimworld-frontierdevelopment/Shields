using System.Linq;
using FrontierDevelopments.General;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_CastPositionFinder
    {
        private static float CalculateShieldPreference(float score, Map map, IntVec3 start, IntVec3 end)
        {
            var shieldManager = map.GetComponent<ShieldManager>();

            var startAdjusted = PositionUtility.ToVector3(start);
            var endAdjusted = PositionUtility.ToVector3(end);

            var targetProtected = shieldManager.WhichShielded(startAdjusted, endAdjusted).ToList();
            var shooterProtected = shieldManager.WhichShielded(endAdjusted, startAdjusted).ToList();
            var shooterUnderShield = shieldManager.WhichShielded(startAdjusted).ToList();

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
        static class Patch_CastPositionPreference
        {
            [HarmonyPostfix]
            static float AdjustScoreFromShielding(float __result, IntVec3 c, CastPositionRequest ___req)
            {
                return CalculateShieldPreference(
                    __result,
                    ___req.caster.Map,
                    c,
                    ___req.target.Position);
            }
        }
    }
}

