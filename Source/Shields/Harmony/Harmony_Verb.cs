using System;
using System.Collections.Generic;
using FrontierDevelopments.General;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public class Harmony_Verb
    {
        private static readonly List<Type> UncheckedTypes = new List<Type>();

        public static void BlacklistType(Type type)
        {
            UncheckedTypes.Add(type);
        }

        protected static bool ShieldBlocks(Thing caster, Verb verb, IntVec3 source, LocalTargetInfo target)
        {
            if (Mod.Settings.EnableShootingIn) return false;
            if (!verb.verbProps.requireLineOfSight) return false;
            if (UncheckedTypes.Exists(a => a.IsInstanceOfType(verb))) return false;
            return caster.Map.GetComponent<ShieldManager>().Shielded(PositionUtility.ToVector3(source), PositionUtility.ToVector3(target.Cell), caster.Faction);
        }

        [HarmonyPatch(typeof(Verb), nameof(Verb.TryFindShootLineFromTo))]
        static class Patch_Verb_TryFindShootLineFromTo
        {
            [HarmonyPrefix]
            static bool AddShieldCheck(ref bool __result, Verb __instance, IntVec3 root, LocalTargetInfo targ, ref ShootLine resultingLine)
            {
                if (ShieldBlocks(__instance.caster, __instance, root, targ))
                {
                    __result = false;
                    resultingLine = new ShootLine();
                    return false;
                }

                return true;
            }
        }
    }
}
