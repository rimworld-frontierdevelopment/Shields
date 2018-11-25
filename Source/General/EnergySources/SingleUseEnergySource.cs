using RimWorld;
using Verse;

namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_SingleUseEnergySource : CompProperties
    {
        public float charge;
        
        public CompProperties_SingleUseEnergySource()
        {
            compClass = typeof(Comp_SingleUseEnergySource);
        }
    }
    
    public class Comp_SingleUseEnergySource : ThingComp, IEnergySource
    {
        protected float _charge = -1f;
        private CompFlickable _flickable;

        private CompProperties_SingleUseEnergySource Props => (CompProperties_SingleUseEnergySource) props;

        public virtual float MinimumCharge => 0f;

        public bool WantActive => _flickable != null && _flickable.SwitchIsOn || _flickable == null;
        public float BaseConsumption { get => 0f; set {} }
        public float EnergyAvailable => _charge;

        public virtual bool IsActive()
        {
            return _charge >= MinimumCharge
                   && WantActive;
        }
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
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

        public float Draw(float amount)
        {
            if (!IsActive()) return 0f;
            var drawn = amount <= _charge ? amount : _charge;
            _charge -= drawn;
            if(_charge < 1) OnEmpty();
            return drawn;
        }

        public void Drain(float amount)
        {
            if (amount > _charge) _charge = 0f;
            else _charge -= amount;
        }

        protected virtual void OnEmpty()
        {
        }
    }
}
