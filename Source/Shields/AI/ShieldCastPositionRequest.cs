using Verse;

namespace FrontierDevelopments.Shields.AI
{
    public struct ShieldCastPositionRequest
    {
        public Pawn caster;
        public Thing target;
        public Verb verb;
        public float maxRangeFromCaster;
        public float maxRangeFromTarget;
        public IntVec3 locus;
        public float maxRangeFromLocus;
        public bool wantCoverFromTarget;
        public int maxRegions;
        public bool avoidShields;
    }
}