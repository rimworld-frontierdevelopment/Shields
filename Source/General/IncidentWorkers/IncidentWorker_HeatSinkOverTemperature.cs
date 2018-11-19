using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General.Comps;
using RimWorld;
using Verse;
using Mod = FrontierDevelopments.Shields.Mod;

namespace FrontierDevelopments.General.IncidentWorkers
{
    public class IncidentWorker_HeatSinkOverTemperature : IncidentWorker
    {
        private static IEnumerable<Comp_HeatSink> GetTargets(Map map)
        {
            foreach (var thing in map.listerThings.AllThings)
            {
                var heatSink = thing.TryGetComp<Comp_HeatSink>();
                if (heatSink != null && heatSink.CanBreakdown)
                {
                    yield return heatSink;
                }
            }
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return Mod.Settings.EnableThermal 
                   && GetTargets((Map)parms.target).Any();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;
            var target = GetTargets(map).RandomElement();

            if (Mod.Settings.EnableCriticalThermalIncidents && target.OverCriticalThreshold)
            {
                target.DoCriticalBreakdown();
            } 
            else if (Mod.Settings.EnableMajorThermalIncidents && target.OverMajorThreshold)
            {
                target.DoMajorBreakdown();
            }
            else if (Mod.Settings.EnableMinorThermalIncidents && target.OverMinorThreshold)
            {
                target.DoMinorBreakdown();
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
