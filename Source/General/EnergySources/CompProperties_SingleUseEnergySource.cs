namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_SingleUseEnergySource : Verse.CompProperties
    {
        public float charge;
        
        public CompProperties_SingleUseEnergySource()
        {
            compClass = typeof(Comp_SingleUseEnergySource);
        }
    }
}