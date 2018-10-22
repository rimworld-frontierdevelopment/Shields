using FrontierDevelopments.General.Comps;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Module.CentralizedClimateControlModule
{
    public class HeatsinkPatch
    {
        public static class Patch_Heatsink_AmbientTemp
        {
            public static bool Prefix(Comp_HeatSink __instance, out double __result)
            {
                var compAirFlowConsumer = __instance.parent.GetComp<CentralizedClimateControl.CompAirFlowConsumer>();
                if (compAirFlowConsumer != null && compAirFlowConsumer.IsActive())
                {
                    __result = compAirFlowConsumer.AirFlowNet.AverageConvertedTemperature;
                    return false;
                }
                __result = 0;
                return true;
            }
        }
    }
}
