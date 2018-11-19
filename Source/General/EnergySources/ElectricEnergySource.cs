using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_ElectricEnergySource : Verse.CompProperties
    {
        public float minimumOnlinePower;
        
        public CompProperties_ElectricEnergySource()
        {
            compClass = typeof(Comp_ElectricEnergySource);
        }
    }
    
    public class Comp_ElectricEnergySource : ThingComp, IEnergySource
    {
        public bool IsActive =>
            _powerTrader?.PowerOn == true
            && _powerTrader.PowerNet != null
            && _powerTrader.PowerNet.CurrentStoredEnergy() >= Props.minimumOnlinePower
            && (_heatSink != null && !_heatSink.OverTemperature || _heatSink == null);

        private IHeatsink _heatSink;

        private float _basePowerConsumption;
        private float _additionalPowerDraw;

        private CompPowerTrader _powerTrader;

        private CompProperties_ElectricEnergySource Props => (CompProperties_ElectricEnergySource) props;

        public bool WantActive => _powerTrader?.PowerOn == true;

        public float BaseConsumption
        {
            get => _basePowerConsumption;
            set
            {
                _powerTrader.PowerOutput = value;
                _basePowerConsumption = value;
            }
        }

        public float EnergyAvailable
        {
            get
            {
                if (_powerTrader?.PowerNet != null)
                {
                    return _powerTrader.PowerNet.CurrentStoredEnergy();
                }
                return 0f;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _powerTrader = parent.GetComp<CompPowerTrader>();
            _heatSink = HeatsinkUtility.Find(parent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _basePowerConsumption, "basePowerConsumption");
            Scribe_Values.Look(ref _additionalPowerDraw, "additionalPowerDraw");
        }

        // Do the actual draw
        public override void CompTick()
        {
            if (_powerTrader.PowerNet != null && _additionalPowerDraw > 0f)
            {
                var availThisTick = _powerTrader.PowerNet.CurrentEnergyGainRate() +
                                    _powerTrader.PowerNet.CurrentStoredEnergy() * 60000;
                var powerWanted = BaseConsumption - _additionalPowerDraw;
                if (availThisTick + powerWanted < 0)
                {
                    powerWanted = -availThisTick;
                }
                _powerTrader.PowerOutput = powerWanted;
            }
            _additionalPowerDraw = 0;
            base.CompTick();
        }

        public void Drain(float amount)
        {
            var perBattery = amount * GenDate.TicksPerDay / _powerTrader.PowerNet.batteryComps.Count;
            _powerTrader.PowerNet.batteryComps.ForEach(battery => battery.DrawPower(perBattery));
        }

        public bool Draw(float amount)
        {
            if (!IsActive) return false;
            amount *= GenDate.TicksPerDay;
            if (Shields.Mod.Settings.ScaleOnHeat && _heatSink != null) amount = amount * Mathf.Pow(1.01f, _heatSink.Temp);
            var drawn = DrawPowerOneTick(amount);
            _additionalPowerDraw += drawn;
            _heatSink?.PushHeat(drawn / 60000 * Shields.Mod.Settings.HeatPerPower);
            return drawn >= amount;
        }

        /// <summary>
        /// Checks and sets up the device to pull the instant draw during the next tick.
        /// </summary>
        /// <param name="amount">Amount to draw</param>
        /// <returns>Amount of power drawn</returns>
        private float DrawPowerOneTick(float amount)
        {
            if (_powerTrader.PowerNet == null) return 0f;
            
            // can this be feed by instantaneous draw? (who are we kidding, no way)
            var gainPowerCovers = _powerTrader.PowerNet.CurrentEnergyGainRate() + BaseConsumption + amount;
            if (gainPowerCovers >= 0) return amount;
            var gainAndBatteriesCover = gainPowerCovers + _powerTrader.PowerNet.CurrentStoredEnergy() * 60000;

            // will batteries cover the difference?
            if (gainAndBatteriesCover >= 0) return amount;

            // uh-oh, energy shortfall
            return amount - gainAndBatteriesCover;
        }
    }
}
