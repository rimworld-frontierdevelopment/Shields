using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ShieldDamage
    {
        public readonly DamageDef def;
        private readonly float _amount;

        public ShieldDamage(DamageDef def, float amount)
        {
            this.def = def;
            _amount = amount;
        }

        public float Damage => _amount;
    }

    public class ShieldDamages
    {
        private readonly ShieldDamage _primary;
        private readonly List<ShieldDamage> secondaries = new List<ShieldDamage>();

        private int _overrideDamage;
        private int _damageLimit = 500;

        public float Factor { get; set; }

        public ShieldDamage Primary => _primary;
        public List<ShieldDamage> Secondaries => secondaries;

        public ShieldDamages(ShieldDamage primary)
        {
            _primary = primary;
        }
        
        public int OverrideDamage
        {
            get => _overrideDamage;
            set => _overrideDamage = value;
        }

        public int DamageLimit
        {
            get => _damageLimit;
            set => _damageLimit = value;
        }

        public void Add(ShieldDamage damage)
        {
            secondaries.Add(damage);
        }

        public float Damage => Secondaries.Aggregate(Primary.Damage, (sum, damage) => sum + damage.Damage) * Factor;
    }
}