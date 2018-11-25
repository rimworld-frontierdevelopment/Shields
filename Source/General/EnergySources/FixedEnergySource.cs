using RimWorld;
using Verse;

namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_FixedEnergySource : CompProperties
    {
        public int amount;

        public CompProperties_FixedEnergySource()
        {
            compClass = typeof(Comp_FixedEnergySource);
        }
    }
    
    public class Comp_FixedEnergySource : Verse.ThingComp, IEnergySource
    {
        private CompProperties_FixedEnergySource Props => (CompProperties_FixedEnergySource) props;

        private CompFlickable _flickable;
        
        public float BaseConsumption { get => 0f; set {} }

        public bool WantActive => _flickable != null && _flickable.SwitchIsOn || _flickable == null;
        public float EnergyAvailable => float.PositiveInfinity;

        public bool IsActive()
        {
            return (_flickable != null && _flickable.SwitchIsOn || _flickable == null);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            _flickable = parent.GetComp<CompFlickable>();
        }

        public float Draw(float amount)
        {
            if (IsActive() && amount <= Props.amount)
            {
                return amount;
            }

            return Props.amount;
        }

        public void Drain(float amount)
        {
        }
    }
}
