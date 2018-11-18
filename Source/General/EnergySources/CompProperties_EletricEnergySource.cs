namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_ElectricEnergySource : Verse.CompProperties
    {
        public float minimumOnlinePower;
        
        public CompProperties_ElectricEnergySource()
        {
            compClass = typeof(Comp_ElectricEnergySource);
        }
    }
}