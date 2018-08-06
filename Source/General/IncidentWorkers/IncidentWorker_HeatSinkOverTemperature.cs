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
            return map.listerBuildings.allBuildingsColonist
                .Where(b => b.GetComp<Comp_HeatSink>() != null)
                .Select(b => b.GetComp<Comp_HeatSink>())
                .Where(c => c.CanBreakdown());
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

            if (Mod.Settings.EnableCriticalThermalIncidents && target.OverCriticalhreshold)
            {
                target.CricticalBreakdown();
            } 
            else if (Mod.Settings.EnableMajorThermalIncidents && target.OverMajorThreshold)
            {
                target.MajorBreakdown();
            }
            else if (Mod.Settings.EnableMinorThermalIncidents && target.OverMinorThreshold)
            {
                target.MinorBreakdown();
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
