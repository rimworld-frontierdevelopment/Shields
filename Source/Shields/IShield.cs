using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public interface IShield
    {
        int ProtectedCellCount { get; }
        bool IsActive();
        bool Collision(Vector3 point);
        Vector3? Collision(Ray ray, float limit);
        Vector3? Collision(Vector3 start, Vector3 end);
        bool Block(long damage, Vector3 position);
        void Draw(CellRect cameraRect);
    }

    public class ShieldUtility
    {
        public static IShield Find(ThingWithComps parent)
        {
            switch (parent)
            {
                case IShield parentSource:
                    return parentSource;
                default:
                    return FindComp(parent);
            }
        }

        public static IShield FindComp(ThingWithComps parent)
        {
            foreach (var comp in parent.AllComps)
            {
                switch (comp)
                {
                    case IShield compSource:
                        return compSource;
                }
            }
            return null;
        }
    }
}