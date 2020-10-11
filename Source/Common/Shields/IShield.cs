using System.Collections.Generic;
using FrontierDevelopments.General;
using FrontierDevelopments.General.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public interface IShield : ILabeled, ILoadReferenceable
    {
        IEnumerable<IShieldStatus> Status { get; }
        int ProtectedCellCount { get; }
        float CellProtectionFactor { get; }
        IShieldParent Parent { get; }
        void SetParent(IShieldParent shieldParent);
        bool IsActive();
        bool Collision(Vector3 point);
        Vector3? Collision(Ray ray, float limit);
        Vector3? Collision(Vector3 start, Vector3 end);
        float Block(float damage, Vector3 position);
        float Block(ShieldDamages damages, Vector3 position);
        void FieldPreDraw();
        void FieldDraw(CellRect cameraRect);
        void FieldPostDraw();
        Faction Faction { get; }
        IEnumerable<Gizmo> ShieldGizmos { get; }
        IEnumerable<UiComponent> UiComponents { get; }
        IEnumerable<ShieldSetting> ShieldSettings { get; set; }
        void ClearWantSettings();
        bool HasWantSettings { get; }
    }

    public interface IShieldParent : ILoadReferenceable
    {
        bool ParentActive { get; }
        float SinkDamage(float damage);
        IShieldResists Resists { get; }
    }

    public interface IShieldManageable : IShield
    {
        Thing Thing { get; }
        float DeploymentSize { get; }
        float SinkDamage(float damage);
        IShieldResists Resists { get; }
    }
}
