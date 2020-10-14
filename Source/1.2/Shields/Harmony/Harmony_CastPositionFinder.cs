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
            if (!Mod.Settings.EnableAICastPositionFinder) return score;
            
            var shieldManager = ShieldManager.For(map);

            var startAdjusted = PositionUtility.ToVector3(start);
            var endAdjusted = PositionUtility.ToVector3(end);

            var targetProtected = shieldManager.Query(map).IsActive().Intersects(startAdjusted, endAdjusted).Get().ToList();
            var shooterProtected = shieldManager.Query(map).IsActive().Intersects(endAdjusted, startAdjusted).Get().ToList();
            var shooterUnderShield = shieldManager.Query(map).IsActive().Intersects(startAdjusted).Get().ToList();

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

