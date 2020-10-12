using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Shields.Buildings;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Alerts
{
    public class Alert_ShieldLowPower : Alert
    {
        private List<Thing> _offenders;
        
        public override AlertReport GetReport()
        {
            _offenders = GetOffenders().ToList();
            if (!_offenders.Any()) return false;
            return AlertReport.CulpritsAre(_offenders);
        }

        private static IEnumerable<Thing> GetOffenders()
        {
            return Find.Maps
                .SelectMany(map => map.GetComponent<ShieldManager>().Fields)
                .SelectMany(field => field.Emitters)
                .Where(IsOffender)
                .Select(shield => shield.Thing);
        }

        private static bool IsOffender(IShield shield)
        {
            return shield.Status.OfType<Building_ElectricShield.ShieldStatusBatteryLow>().Any();
        }

        protected override Color BGColor => Color.grey;

        public override string GetLabel()
        {
            return "fd.shields.alert.lowpower.label".Translate();
        }

        public override TaggedString GetExplanation()
        {
            return "fd.shields.alert.lowpower.description".Translate()
                .Replace("{0}", "" + _offenders.Count)
                .Replace("{1}", "50");
        }
    }
}