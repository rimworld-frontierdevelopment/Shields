using FrontierDevelopments.General.Comps;

namespace FrontierDevelopments.General.CompProperties
{
    public class CompProperties_HeatSink : Verse.CompProperties
    {
        public float grams;
        public float specificHeat;
        public float conductivity;

        public float minorThreshold;
        public float majorThreshold;
        public float criticalThreshold;
        public float maximumTemperature;

        public CompProperties_HeatSink()
        {
            compClass = typeof(Comp_HeatSink);
        }
    }
}