using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Shields.Buildings;
using FrontierDevelopments.Shields.Comps;
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
            foreach (var shield in Find.Maps.SelectMany(map => map.GetComponent<ShieldManager>().Shields))
            {
                switch (shield)
                {
                    case Building_ElectricShield electricShield:
                        if(IsUnpowered(electricShield))
                            yield return electricShield;
                        break;
                    case Comp_ShieldRadial shieldRadial:
                        if(IsUnpowered(shieldRadial))
                            yield return shieldRadial.parent;
                        break;
                }
            }
        }

        private static bool IsUnpowered(Building_ElectricShield shield)
        {
            return shield.Status == Building_ElectricShield.ShieldStatus.Unpowered;
        }

        private static bool IsUnpowered(Comp_ShieldRadial shield)
        {
            switch (shield.parent)
            {
                case Building_ElectricShield electricShield:
                    return IsUnpowered(electricShield);
                default: 
                    return false;
            }
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