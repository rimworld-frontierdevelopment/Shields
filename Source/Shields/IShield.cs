using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public interface IShield
    {
        bool IsActive();
        bool Collision(Vector3 point);
        Vector3? Collision(Ray ray, float limit);
        Vector3? Collision(Vector3 start, Vector3 end);
        bool Block(long damage, Vector3 position);
    }
}