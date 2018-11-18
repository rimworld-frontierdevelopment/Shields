using RimWorld;

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
    
    public class Comp_FixedEnergySource : Verse.ThingComp, IEnergySource
    {
        private CompProperties_FixedEnergySource Props => (CompProperties_FixedEnergySource) props;

        private CompFlickable _flickable;
        
        public float BaseConsumption { get => 0f; set {} }

        public bool WantActive => _flickable != null && _flickable.SwitchIsOn || _flickable == null;
        public bool IsActive => _heatSink != null && !_heatSink.OverTemperature || _heatSink == null;
        public float EnergyAvailable => float.PositiveInfinity;

        private IHeatsink _heatSink;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            _heatSink = HeatsinkUtility.Find(parent);
            _flickable = parent.GetComp<CompFlickable>();
        }

        public bool Draw(float amount)
        {
            if (IsActive && Props.amount <= amount)
            {
                _heatSink?.PushHeat(amount * Shields.Mod.Settings.HeatPerPower);
                return true;
            }

            return false;
        }

        public void Drain(float amount)
        {
        }
    }
}
