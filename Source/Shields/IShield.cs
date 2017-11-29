using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public interface IShield
    {
        bool IsActive();
        bool Collision(Vector2 point);
        Vector2? Collision(Ray2D ray, float limit);
        bool Damage(int damage, Vector2 position);
        IntVec3 EmitterPosition();
        void DrawShield();
    }
}