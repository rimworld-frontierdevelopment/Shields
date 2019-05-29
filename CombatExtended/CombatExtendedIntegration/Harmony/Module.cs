using FrontierDevelopments.Shields.Harmony;
using Harmony;
using RimWorld;
using CombatExtended;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Module
    {
        [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        static class Load
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                Harmony_Verb.BlacklistType(typeof(Verb_ShootMortarCE));
                Harmony_Verb.BlacklistType(typeof(Verb_MarkForArtillery));
            }
        }
    }
}
