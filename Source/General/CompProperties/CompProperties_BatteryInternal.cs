using FrontierDevelopments.General.Comps;
using RimWorld;

namespace FrontierDevelopments.General.CompProperties
{
    public class CompProperties_BatteryInternal : CompProperties_Battery
    {
        public float chargeRate;
        
        public CompProperties_BatteryInternal()
        {
            compClass = typeof(Comp_BatteryInternal);
        }
    }
}