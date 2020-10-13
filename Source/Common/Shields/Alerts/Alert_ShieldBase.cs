using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Alerts
{
    public abstract class Alert_ShieldBase : Alert
    {
        public HashSet<Thing> Offenders { get; private set; }

        public override AlertReport GetReport()
        {
            Offenders = GetOffenders();
            if (!Offenders.Any()) return false;
            return AlertReport.CulpritsAre(Offenders.ToList());
        }

        protected virtual HashSet<Thing> GetOffenders()
        {
            return Find.Maps
                .SelectMany(map => map.GetComponent<ShieldManager>().Fields)
                .SelectMany(field => field.Emitters)
                .Where(IsOffender)
                .Select(shield => shield.Thing)
                .ToHashSet();
        }

        protected abstract bool IsOffender(IShield shield);
    }
}