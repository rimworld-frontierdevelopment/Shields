using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public interface IShield
    {
        bool IsActive();
        bool Collision(Vector3 point);
        Vector3? Collision(Ray ray, float limit);
        bool Damage(int damage, Vector3 position);
    }
}