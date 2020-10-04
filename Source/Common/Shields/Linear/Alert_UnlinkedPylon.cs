using System.Linq;
using FrontierDevelopments.Shields.Linear;
using Verse;

namespace FrontierDevelopments.Shields.Alerts
{
    public class Alert_UnlinkedPylon : Alert_ShieldBase
    {
        protected override bool IsOffender(IShield shield)
        {
            return shield.Status.OfType<Comp_ShieldLinear.ShieldStatusUnlinked>().Any();
        }

        public override string GetLabel()
        {
            return "fd.shields.alert.pylons.unlinked.label".Translate();
        }

        public override TaggedString GetExplanation()
        {
            return "fd.shields.alert.pylons.unlinked.description".Translate()
                .Replace("{0}", "" + Offenders.Count);
        }
    }
}