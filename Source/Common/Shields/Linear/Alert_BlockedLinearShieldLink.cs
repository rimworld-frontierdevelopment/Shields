using System.Linq;
using FrontierDevelopments.Shields.Linear;
using Verse;

namespace FrontierDevelopments.Shields.Alerts
{
    public class Alert_BlockedLinearShieldLink : Alert_ShieldBase
    {
        protected override bool IsOffender(IShield shield)
        {
            return shield.Status.OfType<Comp_ShieldLinear.ShieldStatusLinkBlocked>().Any();
        }

        public override string GetLabel()
        {
            return "fd.shields.alert.pylons.blocked.label".Translate();
        }

        public override TaggedString GetExplanation()
        {
            return "fd.shields.alert.pylons.blocked.description".Translate(Offenders.Count);
        }
    }
}