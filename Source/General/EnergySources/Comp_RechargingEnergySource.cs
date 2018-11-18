namespace FrontierDevelopments.General.EnergySources
{
    public class Comp_RechargingEnergySource : Comp_SingleUseEnergySource
    {
        private CompProperties_RechargingEnergySource Props => (CompProperties_RechargingEnergySource) props;

        protected virtual float Rate => Props.rate;

        public override float MinimumCharge => Props.minimum;

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
        }
    }
}
