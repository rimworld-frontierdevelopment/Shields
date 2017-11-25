using RimWorld;

namespace FrontierDevelopments.General.Comps
{
    public class Comp_PowerPlantTrader : CompPowerPlant
    {
        protected override float DesiredPowerOutput => PowerOutput;

        public override void CompTick()
        {
            
        }
    }
}