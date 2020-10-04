using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Linear
{
    public interface ILinearShield : IShield
    {
        Map Map { get; }
        IntVec3 Position { get; }
        Vector3 TrueCenter { get; }
        Faction Faction { get; }
        int EfficientRange { get; }
        float Block(float damage, Vector3 position);
        float Block(ShieldDamages damages, Vector3 position);
        void Add(LinearShieldLink link);
        void WantLinkWith(ILinearShield other);
        void RemoveWantLinkWith(ILinearShield other);
        void UnlinkFrom(ILinearShield other);
        bool CanLinkWith(ILinearShield other);
        void NotifyLinked(ILinearShield other);
    }
}