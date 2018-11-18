namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_FixedEnergySource : Verse.CompProperties
    {
        public int amount;

        public CompProperties_FixedEnergySource()
        {
            compClass = typeof(Comp_FixedEnergySource);
        }
    }
}