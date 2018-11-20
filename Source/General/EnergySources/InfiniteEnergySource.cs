using RimWorld;
using Verse;

namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_InfiniteEnergySource : Verse.CompProperties
    {
        public CompProperties_InfiniteEnergySource()
        {
            compClass = typeof(Comp_InfiniteEnergySource);
        }
    }
    
    public class Comp_InfiniteEnergySource : ThingComp, IEnergySource
    {
        private IHeatsink _heatSink;
        private CompFlickable _flickable;

        public float BaseConsumption { get => 0f; set {} }
        public bool WantActive => _flickable != null && _flickable.SwitchIsOn || _flickable == null;
        public float EnergyAvailable => float.PositiveInfinity;

        public bool IsActive()
        {
            return _flickable != null && _flickable.SwitchIsOn || _flickable == null;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            _heatSink = HeatsinkUtility.Find(parent);
            _flickable = parent.GetComp<CompFlickable>();
        }

        public bool Draw(float amount)
        {
            if (!IsActive()) return false;
            _heatSink?.PushHeat(amount * Shields.Mod.Settings.HeatPerPower);
            return true;
        }

        public void Drain(float amount)
        {
        }
    }
}