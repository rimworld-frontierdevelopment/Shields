namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_RechargingEnergySource : CompProperties_SingleUseEnergySource
    {
        public float rate;
        public float minimum;

        public CompProperties_RechargingEnergySource()
        {
            compClass = typeof(Comp_RechargingEnergySource);
        }
    }
    
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
