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
}