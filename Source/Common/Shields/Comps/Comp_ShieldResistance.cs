using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Comps
{
    public class ShieldResistance
    {
        public string damageDefName;
        public bool resist = true;
        public float multiplier = 1.0f;
    }

    public class CompProperties_ShieldResistance : CompProperties
    {
        public List<ShieldResistance> resists;

        public CompProperties_ShieldResistance()
        {
            compClass = typeof(Comp_ShieldResistance);
        }
    }

    public class Comp_ShieldResistance : ThingComp, IShieldResists
    {
        private Dictionary<string, ShieldResistance> resists = new Dictionary<string, ShieldResistance>();

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            ((CompProperties_ShieldResistance) props)?.resists?.ForEach(resist =>
            {
                resists.Add(resist.damageDefName, resist);
            });
        }

        public virtual float? Apply(ShieldDamages damages)
        {
            var primaryResist = CanResist(damages.Primary);
            if (primaryResist == null) return null;
            return primaryResist.Value + CannotResist(damages.Secondaries);
        }

        public virtual float? CanResist(ShieldDamage damage)
        {
            if (resists.ContainsKey(damage.def.defName))
            {
                var resist = resists[damage.def.defName];
                if (resist.resist)
                {
                    return damage.Damage * resist.multiplier;                    
                }
                return null;
            }
            return damage.Damage;
        }

        public virtual float CannotResist(ShieldDamage damage)
        {
            var multiplier = 1.0f;
            if (resists.ContainsKey(damage.def.defName))
            {
                multiplier = resists[damage.def.defName].multiplier;
            }
            return damage.Damage * multiplier;
        }

        public virtual float CannotResist(List<ShieldDamage> damages)
        {
            return damages.Aggregate(0f, (sum, damage) => sum + CannotResist(damage));
        }
    }
}