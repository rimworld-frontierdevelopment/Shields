using FrontierDevelopments.General;
using Harmony;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields
{
    public class Harmony_AttackTargetFinder
    {
        [HarmonyPatch(typeof(AttackTargetFinder), "GetShootingTargetScore")]
        public static class Patch_Attack_TargetFinder_GetShootingTargetScore
        {
            static float Postfix(float __result, IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
            {
                if (Mod.ShieldManager.Shielded(searcher.Thing.Map, Common.ToVector3(searcher.Thing.Position),
                    Common.ToVector3(target.Thing.Position)))
                {
                    return __result / 2;
                }
                return __result;
            }
        }
    }
}