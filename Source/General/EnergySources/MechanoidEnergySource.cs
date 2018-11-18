using RimWorld;
using Verse;

namespace FrontierDevelopments.General.EnergySources
{
    public class CompProperties_MechanoidEnergySource : CompProperties_RechargingEnergySource
    {
        public CompProperties_MechanoidEnergySource()
        {
            compClass = typeof(Comp_MechanoidEnergySource);
        }
    }
    
    public class Comp_MechanoidEnergySource : Comp_RechargingEnergySource
    {
        private CompProperties_MechanoidEnergySource Props => (CompProperties_MechanoidEnergySource) props;
        private float PowerGeneration => ((Pawn) parent).health.capacities.GetLevel(PawnCapacityDefOf.BloodPumping);

        protected override float Rate => PowerGeneration * Props.rate;
    }
}