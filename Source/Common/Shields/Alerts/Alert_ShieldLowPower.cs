using System.Linq;
using FrontierDevelopments.Shields.Buildings;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Alerts
{
    public class Alert_ShieldLowPower : Alert_ShieldBase
    {
        protected override bool IsOffender(IShield shield)
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
                .Replace("{0}", "" + Offenders.Count)
                .Replace("{1}", "50");
        }
    }
}