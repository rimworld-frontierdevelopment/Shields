using CombatExtended;
using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using RimWorld;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Module
    {
        [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        private static class Load
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                Harmony_Verb.BlacklistType(typeof(Verb_ShootMortarCE));
                Harmony_Verb.BlacklistType(typeof(Verb_MarkForArtillery));
            }
        }
    }
}