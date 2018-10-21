using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Buildings
{
    public abstract class Building_ShieldBase : Building, IShield
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Mod.ShieldManager.Add(Map, this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Mod.ShieldManager.Del(Map, this);
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Mod.ShieldManager.Del(Map, this);
            base.Destroy(mode);
        }

        public abstract bool IsActive();
        public abstract bool Collision(Vector3 point);
        public abstract Vector3? Collision(Ray ray, float limit);
        public abstract bool Block(int damage, Vector3 position);
    }
}