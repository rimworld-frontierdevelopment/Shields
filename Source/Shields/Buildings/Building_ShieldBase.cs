using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Buildings
{
    public abstract class Building_ShieldBase : Building, IShield
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Map.GetComponent<ShieldManager>().Add(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Map.GetComponent<ShieldManager>().Del(this);
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Map.GetComponent<ShieldManager>().Del(this);
            base.Destroy(mode);
        }

        public abstract bool IsActive();
        public abstract bool Collision(Vector3 point);
        public abstract Vector3? Collision(Ray ray, float limit);
        public abstract Vector3? Collision(Vector3 start, Vector3 end);
        public abstract bool Block(long damage, Vector3 position);
        public abstract void Draw(CellRect cameraRect);
    }
}