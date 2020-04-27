using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Tale
{
    public class Tale_KilledByImpactingOnShield : Tale_SinglePawn
    {
        public Tale_KilledByImpactingOnShield()
        {
        }

        public Tale_KilledByImpactingOnShield(Pawn victim) : base(victim)
        {
        }

        public Tale_KilledByImpactingOnShield(Pawn victim, IntVec3 position, Map map) : base(victim)
        {
            surroundings = TaleData_Surroundings.GenerateFrom(position, map);
        }
    }
}