using Verse;

namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_RechargingEnergySource : CompProperties_SingleUseEnergySource
    {
        public float rate;
        public float minimum;
        public float onlineRechargeAmount;

        public CompProperties_RechargingEnergySource()
        {
            compClass = typeof(Comp_RechargingEnergySource);
        }
    }

    public class Comp_RechargingEnergySource : Comp_SingleUseEnergySource
    {
        private bool _offlineRecharging;
        
        private CompProperties_RechargingEnergySource Props => (CompProperties_RechargingEnergySource) props;

        protected virtual float Rate => Props.rate;

        public override float MinimumCharge => Props.minimum;

        public override bool IsActive()
        {
            return !_offlineRecharging && base.IsActive();
        }

        public override void CompTick()
        {
            if (_charge + Rate <= Props.charge)
            {
                _charge += Rate;
            }
            else
            {
                _charge = Props.charge;
            }

            if (_offlineRecharging && _charge >= Props.onlineRechargeAmount)
            {
                _offlineRecharging = false;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _offlineRecharging, "offlineRecharging", false);
        }

        protected override void OnEmpty()
        {
            _offlineRecharging = true;
        }
    }
}
