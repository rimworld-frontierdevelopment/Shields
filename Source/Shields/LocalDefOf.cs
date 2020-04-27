using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields
{
    [DefOf]
    public class LocalDefOf
    {
        public static TaleDef KilledByImpactingOnShield;
        public static ThingDef MinifiedShieldGeneratorPortable;

        static LocalDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LocalDefOf));
        }
    }
}