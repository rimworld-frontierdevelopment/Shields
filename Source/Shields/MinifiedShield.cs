using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class MinifiedShield : MinifiedThing, IShield
    {
        private IShield _shield;
        private bool _enabled = true;
        private Pawn _deployer;
        private bool _needToMapLink;

        public bool Deployed => _deployer != null;

        public override void Tick()
        {
            if (_needToMapLink)
            {
                _needToMapLink = false;
                _deployer?.Map.GetComponent<ShieldManager>().Add(_deployer);
            }
            base.Tick();
            if(Deployed) GetDirectlyHeldThings()?.ThingOwnerTick();
        }

        public bool Deploy(Pawn pawn)
        {
            if (pawn.RaceProps.baseBodySize >= 2.0f)
            {
                _deployer = pawn;
            }
            return Deployed;
        }

        public void Undeploy()
        {
            _deployer = null;
        }

        private IShield Shield
        {
            get
            {
                if (_shield == null)
                {
                    switch (InnerThing)
                    {
                        case IShield shield:
                            _shield = shield;
                            break;
                        case ThingWithComps thing:
                            _shield = thing.AllComps.OfType<IShield>().First();
                            break;
                    }
                }
                return _shield;
            }
        }

        private bool ShieldOnline
        {
            get
            {
                if (!_enabled || !Deployed || Shield == null) return false;
                return Shield.IsActive();
            }
        }

        public int ProtectedCellCount => Shield.ProtectedCellCount;
        public bool IsActive()
        {
            return ShieldOnline && Shield.IsActive();
        }

        public bool Collision(Vector3 point)
        {
            return ShieldOnline && Shield.Collision(point);
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return ShieldOnline
                ? Shield.Collision(ray, limit)
                : null;
        }

        public Vector3? Collision(Vector3 start, Vector3 end)
        {
            return ShieldOnline
                ? Shield.Collision(start, end)
                : null;
        }

        public bool Block(long damage, Vector3 position)
        {
            return ShieldOnline && Shield.Block(damage, position);
        }

        public bool Block(ShieldDamages damages, Vector3 position)
        {
            return ShieldOnline && Shield.Block(damages, position);
        }

        public void Draw(CellRect cameraRect)
        {
            if(ShieldOnline) Shield.Draw(cameraRect);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _enabled, "enabled", true);
            Scribe_References.Look(ref _deployer, "deployer");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _needToMapLink = true;
            }
        }
    }
}