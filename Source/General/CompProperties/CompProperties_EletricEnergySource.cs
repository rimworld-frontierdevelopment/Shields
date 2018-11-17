using FrontierDevelopments.General.Comps;
using RimWorld;

namespace FrontierDevelopments.General.CompProperties
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