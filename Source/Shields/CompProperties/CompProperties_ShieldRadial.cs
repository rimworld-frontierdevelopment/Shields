using FrontierDevelopments.Shields.Comps;

namespace FrontierDevelopments.Shields.CompProperties
{
    public class CompProperties_ShieldRadial : Verse.CompProperties
    {
        public int minRadius;
        public int maxRadius;

        public int warmupTicks;
        
        public CompProperties_ShieldRadial()
        {
            compClass = typeof(Comp_ShieldRadial);
        }
    }
}