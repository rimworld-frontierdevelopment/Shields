using System;
using FrontierDevelopments.General.CompProperties;
using RimWorld;
using Verse;

namespace FrontierDevelopments.General.Comps
{
    public class Comp_HeatSink : ThingComp
    {
        public static readonly float KELVIN_ZERO_CELCIUS = 273.15f;

        private float _dissipationRate;
        private float _temperature;

        public Func<bool> CanBreakdown = () => false;
        public Action MinorBreakdown = () => { };
        public Action MajorBreakdown = () => { };
        public Action CriticalBreakdown = () => { };
        
        public CompProperties_HeatSink Props => (CompProperties_HeatSink)props;

        public float Temp => _temperature; 

        private float Joules
        {
            get => (_temperature + KELVIN_ZERO_CELCIUS) * Props.specificHeat * Props.grams;
            set =>  _temperature = value / Props.specificHeat / Props.grams - KELVIN_ZERO_CELCIUS;
        }

        public bool OverTemperature => OverMinorThreshold;

        public bool OverMinorThreshold => Temp >= Props.minorThreshold;
        
        public bool OverMajorThreshold => Temp >= Props.majorThreshold;
        
        public bool OverCriticalThreshold => Temp >= Props.criticalThreshold;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _dissipationRate = GenDate.TicksPerDay / Props.conductivity;
        }

        public void PushHeat(float wattDays)
        {
            Joules += wattDays / 86.4f * 1000;
        }

        protected virtual float AmbientTemp()
        {
            return parent.AmbientTemperature;
        }

        protected virtual void DissipateHeat(float kilojoules)
        {
            GenTemperature.PushHeat(parent, kilojoules);
        }

        public override void CompTick()
        {
            var heatDissipated =  (Temp - AmbientTemp()) / _dissipationRate;
            Joules -= heatDissipated * 1000f;
            DissipateHeat(heatDissipated);
        }

        public override string CompInspectStringExtra()
        {
            return "fd.heatsink.temperature".Translate() + ": " + Temp.ToStringTemperature();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _temperature, "temperature", 20f);
        }
    }
}
