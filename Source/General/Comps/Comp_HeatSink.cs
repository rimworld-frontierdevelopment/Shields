using System;
using FrontierDevelopments.General.CompProperties;
using Verse;

namespace FrontierDevelopments.General.Comps
{
    public class Comp_HeatSink : ThingComp
    {
        public static readonly double KELVIN_ZERO_CELCIUS = 273.15;

        public static readonly int TICKS_PER_DAY = 60000;

        public static double DisipationRate;

        private double _temperature;

        public Func<bool> CanBreakdown = () => false;
        public Action MinorBreakdown = () => { };
        public Action MajorBreakdown = () => { };
        public Action CricticalBreakdown = () => { };
        
        public CompProperties_HeatSink Props => (CompProperties_HeatSink)props;

        public double Temp => _temperature; 

        private double Joules
        {
            get => (_temperature + KELVIN_ZERO_CELCIUS) * Props.specificHeat * Props.grams;
            set =>  _temperature = value / Props.specificHeat / Props.grams - KELVIN_ZERO_CELCIUS;
        }

        public bool OverTemperature => OverMinorThreshold;

        public bool OverMinorThreshold => Temp >= Props.minorThreshold;
        
        public bool OverMajorThreshold => Temp >= Props.majorThreshold;
        
        public bool OverCriticalhreshold => Temp >= Props.criticalThreshold;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            DisipationRate = TICKS_PER_DAY / Props.conductivity;
        }

        public void PushHeat(double wattDays)
        {
            Joules += wattDays / 86.4 * 1000;
        }

        protected virtual double AmbientTemp() 
        {
           return (parent.GetRoom()?.Group?.Temperature)
               .GetValueOrDefault(parent.Map.mapTemperature.OutdoorTemp);
        }

        protected virtual void DisipateHeat(double kilojoules)
        {
            GenTemperature.PushHeat(parent, (float)kilojoules);
        }

        public override void CompTick()
        {
            var ambient = AmbientTemp();
            var tempDelta = Temp - ambient;
            var heatDisipated =  tempDelta / DisipationRate;
            Joules -= heatDisipated * 1000;
            DisipateHeat(heatDisipated);
        }

        public override string CompInspectStringExtra()
        {
            return "fd.heatsink.temperature".Translate() + ": " + (int) Temp + "C";
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _temperature, "temperature", 20f);
        }
    }
}
