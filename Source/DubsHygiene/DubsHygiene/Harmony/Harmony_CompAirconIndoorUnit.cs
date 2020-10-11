using DubsBadHygiene;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields.BadHygiene.Harmony
{
    [StaticConstructorOnStartup]
    public static class Harmony_CompAirconIndoorUnit
    {
        // Needed due to materials in CompAirconIndoorUnit
        static Harmony_CompAirconIndoorUnit()
        {
            Patch(new HarmonyLib.Harmony("FrontierDevelopment.Shields.BadHygiene"));
        }
        
        public static void Patch(HarmonyLib.Harmony harmony)
        {
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(CompAirconIndoorUnit), nameof(CompAirconIndoorUnit.Capacity)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patch_Capacity), nameof(Patch_Capacity.AddDynamicCapacity))));
        }
        
        // [HarmonyPatch(typeof(CompAirconIndoorUnit), nameof(CompAirconIndoorUnit.Capacity), MethodType.Getter)]
        static class Patch_Capacity
        {
            [HarmonyPostfix]
            public static float AddDynamicCapacity(float result, CompAirconIndoorUnit __instance)
            {
                switch (__instance)
                {
                    case Comp_DubsAirVent airVent:
                        return airVent.VentCapacity;
                    default:
                        return result; 
                }
            }
        }
    }
}