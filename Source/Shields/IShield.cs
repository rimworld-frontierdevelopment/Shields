using FrontierDevelopments.General;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        public float Damage => Secondaries.Aggregate(Primary.Damage, (sum, damage) => sum + damage.Damage);
    }

    public interface IShield : ILabeled, ILoadReferenceable
    {
        int ProtectedCellCount { get; }

        void SetParent(IShield shieldParent);

        bool IsActive();

        bool Collision(Vector3 point);

        Vector3? Collision(Ray ray, float limit);

        Vector3? Collision(Vector3 start, Vector3 end);

        float CalculateDamage(ShieldDamages damages);

        float SinkDamage(float damage);

        float Block(float damage, Vector3 position);

        float Block(ShieldDamages damages, Vector3 position);

        void Draw(CellRect cameraRect);

        Faction Faction { get; }
        IShieldResists Resists { get; }
        float DeploymentSize { get; }
        IEnumerable<Gizmo> ShieldGizmos { get; }
    }
}