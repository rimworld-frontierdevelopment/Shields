using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    static class Harmony_RoyalTitlePermitWorker_DropResources
    {
        [HarmonyPatch]
        static class Patch_BeginCallResources
        {
            [HarmonyTargetMethod]
            static MethodInfo Target()
            {
                return typeof(RoyalTitlePermitWorker_DropResources).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic)
                    .SelectMany(AccessTools.GetDeclaredMethods)
                    .Where(method => method.Name.Contains("BeginCallResources"))
                    .Where(method => method.ReturnType == typeof(bool))
                    .First(method => method.GetParameters().Any(param => param.ParameterType == typeof(TargetInfo)));
            }

            [HarmonyPostfix]
            static bool AddShieldCheck(bool result, TargetInfo target)
            {
                if (result)
                {
                    var blocked = new ShieldQuery(target.Map)
                        .Intersects(target.Cell.ToVector3().Yto0())
                        .Get()
                        .Any();
                    if (blocked) return false;
                }

                return result;
            }
        }
    }
}