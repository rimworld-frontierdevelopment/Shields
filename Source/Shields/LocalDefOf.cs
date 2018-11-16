using RimWorld;

namespace FrontierDevelopments.Shields
{
    [DefOf]
    public class LocalDefOf
    {
        public static TaleDef KilledByImpactingOnShield;
        
        static LocalDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof (LocalDefOf));
        }
    }
}