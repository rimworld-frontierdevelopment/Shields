using RimWorld;
using Verse;

namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_ElectricEnergySource : CompProperties
    {
        public float minimumOnlinePower;
        
        public CompProperties_ElectricEnergySource()
        {
            compClass = typeof(Comp_ElectricEnergySource);
        }
    }
    
    public class Comp_ElectricEnergySource : ThingComp, IEnergySource
    {
        private float _basePowerConsumption;
        private float _additionalPowerDraw;

        private CompPowerTrader _powerTrader;

        private CompProperties_ElectricEnergySource Props => (CompProperties_ElectricEnergySource) props;

        public bool WantActive => _powerTrader?.PowerOn == true;

        public bool IsActive()
        {
            return _powerTrader?.PowerOn == true
                   && EnergyAvailable >= Props.minimumOnlinePower;
        }

        public float BaseConsumption
        {
            get => _basePowerConsumption;
            set
            {
                _powerTrader.PowerOutput = value;
                _basePowerConsumption = value;
            }
        }

        public float GainEnergyAvailable
        {
            get
            {
                if (_powerTrader?.PowerNet != null)
                {
                    return _powerTrader.PowerNet.CurrentEnergyGainRate() / GenDate.TicksPerDay;
                }
                return 0f;
            }
        }

        public float StoredEnergyAvailable
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

        public float EnergyAvailable => GainEnergyAvailable + StoredEnergyAvailable;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _powerTrader = parent.GetComp<CompPowerTrader>();
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
            Log.Message("power: " + GainEnergyAvailable);
            if (_powerTrader.PowerNet != null && _additionalPowerDraw > 0f)
            {
                var availThisTick = _powerTrader.PowerNet.CurrentEnergyGainRate() + StoredEnergyAvailable * 60000;
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
            var perBattery = amount / _powerTrader.PowerNet.batteryComps.Count;
            _powerTrader.PowerNet.batteryComps.ForEach(battery => battery.DrawPower(perBattery));
        }

        public float Draw(float amount)
        {
            if (!IsActive()) return 0f;
            amount *= GenDate.TicksPerDay;
            var drawn = DrawPowerOneTick(amount);
            _additionalPowerDraw += drawn;
            return drawn;
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
            var gainAndBatteriesCover = gainPowerCovers + StoredEnergyAvailable * 60000;

            // will batteries cover the difference?
            if (gainAndBatteriesCover >= 0) return amount;

            // uh-oh, energy shortfall
            return amount - gainAndBatteriesCover;
        }
    }
}
