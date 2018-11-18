namespace FrontierDevelopments.General.EnergySources
{
    public class Comp_RechargingEnergySource : Comp_SingleUseEnergySource
    {
        private CompProperties_RechargingEnergySource Props => (CompProperties_RechargingEnergySource) props; 

        public override void CompTick()
        {
            if (_charge + Props.rate <= Props.charge)
            {
                _charge += Props.rate;
            }
            else
            {
                _charge = Props.charge;
            }
        }
    }
}
