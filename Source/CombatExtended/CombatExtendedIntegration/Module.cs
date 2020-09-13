using System.Reflection;
using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using RimWorld;
using CombatExtended;
using Verse;

namespace FrontierDevelopments.CombatExtendedIntegration
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            Log.Message("Frontier Developments Shields :: Loading Combat Extended (Continued) support");
            
            var harmony = new HarmonyLib.Harmony("FrontierDevelopment.Shields.CombatExtended");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

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
