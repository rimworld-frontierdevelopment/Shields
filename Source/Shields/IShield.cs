using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
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

    public interface IShield
    {
        int ProtectedCellCount { get; }
        bool IsActive();
        bool Collision(Vector3 point);
        Vector3? Collision(Ray ray, float limit);
        Vector3? Collision(Vector3 start, Vector3 end);
        bool Block(long damage, Vector3 position);
        bool Block(ShieldDamages damages, Vector3 position);
        void Draw(CellRect cameraRect);
        Faction Faction { get; }
    }
}