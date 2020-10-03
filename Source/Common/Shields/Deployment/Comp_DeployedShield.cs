using System.Collections.Generic;
using FrontierDevelopments.General.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Comps
{
    public class CompProperties_DeployedShield : CompProperties
    {
        public CompProperties_DeployedShield()
        {
            compClass = typeof(Comp_DeployedShield);
        }
    }
    public class Comp_DeployedShield : ThingComp, IShield
    {
        private IShield _deployed;
        private int _id;
        
        private Pawn pawn => parent as Pawn;
        public Map Map => pawn.Map;

        public Comp_DeployedShield()
        {
        }

        public Comp_DeployedShield(IShield deployed)
        {
            _deployed = deployed;
            _id = Find.UniqueIDsManager.GetNextThingID();
            parent.Map.GetComponent<ShieldManager>().Add(this);
        }

        public string Label => _deployed.Label;
        public int ProtectedCellCount => _deployed.ProtectedCellCount;
        public float CellProtectionFactor => _deployed.CellProtectionFactor;
        public Faction Faction => parent.Faction;
        public float DeploymentSize => _deployed.DeploymentSize;
        public IEnumerable<Gizmo> ShieldGizmos => _deployed.ShieldGizmos;
        public IShieldResists Resists => _deployed.Resists;
        public IShield Parent => _deployed.Parent;
        public Thing Thing => _deployed.Thing;

        public void SetParent(IShield shieldParent)
        {
            _deployed = shieldParent;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            parent.Map.GetComponent<ShieldManager>().Add(this);
        }

        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<ShieldManager>().Del(this);
        }

        public bool IsActive()
        {
            return _deployed.IsActive();
        }

        public bool Collision(Vector3 point)
        {
            return _deployed.Collision(point);
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return _deployed.Collision(ray, limit);
        }

        public Vector3? Collision(Vector3 start, Vector3 end)
        {
            return _deployed.Collision(start, end);
        }

        public float CalculateDamage(ShieldDamages damages)
        {
            return _deployed.CalculateDamage(damages);
        }

        public float SinkDamage(float damage)
        {
            return _deployed.SinkDamage(damage);
        }

        public float Block(float damage, Vector3 position)
        {
            return _deployed.Block(damage, position);
        }

        public float Block(ShieldDamages damages, Vector3 position)
        {
            return _deployed.Block(damages, position);
        }

        public void FieldPreDraw()
        {
            _deployed.FieldPreDraw();
        }

        public void FieldDraw(CellRect cameraRect)
        {
            _deployed.FieldDraw(cameraRect);
        }

        public void FieldPostDraw()
        {
            _deployed.FieldPostDraw();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            foreach (var gizmo in _deployed.ShieldGizmos)
            {
                yield return gizmo;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _id, "shieldDeployedId");
            Scribe_References.Look(ref _deployed, "shieldDeployedReference");
        }

        public string GetUniqueLoadID()
        {
            return "DeployedShield" + _id;
        }

        public IEnumerable<UiComponent> UiComponents => _deployed.UiComponents;

        public IEnumerable<ShieldSetting> ShieldSettings
        {
            get => _deployed.ShieldSettings;
            set => _deployed.ShieldSettings = value;
        }
    }
}