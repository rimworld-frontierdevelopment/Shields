using CombatExtended;
using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.CombatExtendedIntegration.Harmony
{
    public class Harmony_Verb_LaunchProjectileCE : Harmony_Verb
    {
        [HarmonyPatch(typeof(Verb_LaunchProjectileCE), nameof(Verb_LaunchProjectileCE.CanHitTargetFrom), new []{ typeof(IntVec3), typeof(LocalTargetInfo), typeof(string) }, new []{ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref})]
        static class Patch_CanHitTargetFrom
        {
            [HarmonyPostfix]
            static bool Postfix(
                bool __result, 
                Verb_LaunchProjectileCE __instance, 
                IntVec3 root, 
                LocalTargetInfo targ,
                ref string report)
            {
                if (__result)
                {
                    if (ShieldBlocks(__instance.caster, __instance, root, targ))
                    {
                        report = "Block by shield";
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }
    }
}