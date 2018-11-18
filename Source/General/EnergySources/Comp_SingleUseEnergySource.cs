using RimWorld;
using Verse;

namespace FrontierDevelopments.General.EnergySources
{
    public class Comp_SingleUseEnergySource : ThingComp, IEnergySource
    {
        protected float _charge;
        private IHeatsink _heatSink;
        private CompFlickable _flickable;

        private CompProperties_SingleUseEnergySource Props => (CompProperties_SingleUseEnergySource) props;

        public virtual float MinimumCharge => 0f;

        public bool IsActive => 
            _charge >= MinimumCharge 
            && WantActive 
            && (_heatSink != null && !_heatSink.OverTemperature || _heatSink == null);

        public bool WantActive => _flickable != null && _flickable.SwitchIsOn || _flickable == null;
        public float BaseConsumption { get => 0f; set {} }
        public float EnergyAvailable => _charge;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            _heatSink = HeatsinkUtility.Find(parent);
            _flickable = parent.GetComp<CompFlickable>();
            if (_charge < 0) _charge = Props.charge;
        }

        public override string CompInspectStringExtra()
        {
            return "Charge: " + _charge;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _charge, "charge", -1);
        }

        public bool Draw(float amount)
        {
            if (!IsActive) return false;
            var drawn = amount <= _charge ? amount : _charge;
            _charge -= drawn;
            _heatSink?.PushHeat(drawn * Shields.Mod.Settings.HeatPerPower);
            return !(drawn < amount);
        }

        public void Drain(float amount)
        {
            if (amount > _charge) _charge = 0f;
            else _charge -= amount;
        }
    }
}
