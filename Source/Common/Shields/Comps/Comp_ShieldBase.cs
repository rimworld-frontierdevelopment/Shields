using System.Collections.Generic;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Comps;
using FrontierDevelopments.General.UI;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Comps
{
    public abstract class CompProperties_ShieldBase : CompProperties {
        public int deploymentSize;
    }
    
    public abstract class Comp_ShieldBase : ThingComp, IShield, ILoadReferenceable
    {
        private int? _id;
        private IShieldParent _parent;
        private bool _renderField = true;
        
        public virtual string Label => parent.Label;

        protected abstract string ShieldLoadType { get; }
        public abstract IEnumerable<IShieldStatus> Status { get; }
        public Map Map => parent.Map;
        public abstract int ProtectedCellCount { get; }
        public abstract float CellProtectionFactor { get; }
        public virtual Faction Faction => parent.Faction;
        protected Vector3 ExactPosition => (PositionUtility.GetRealPosition(parent.holdingOwner.Owner) ?? parent.TrueCenter()).Yto0();
        public virtual IShieldResists Resists => parent.TryGetComp<Comp_ShieldResistance>();
        public virtual float DeploymentSize => Props.deploymentSize;
        public abstract IEnumerable<UiComponent> UiComponents { get; }
        
        public abstract bool HasWantSettings { get; }

        public IShieldParent Parent => _parent;

        protected bool RenderField => _renderField;

        public virtual IEnumerable<ShieldSetting> ShieldSettings
        {
            get
            {
                yield return new RenderFieldSetting(_renderField);
            } 
            
            set => value.Do(Apply);
        }

        protected virtual void Apply(ShieldSetting setting)
        {
            switch (setting)
            {
                case RenderFieldSetting renderFieldSetting:
                    _renderField = renderFieldSetting.Get();
                    break;
            }
        }

        private CompProperties_ShieldBase Props => (CompProperties_ShieldBase) props;
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (_id == null) _id = Find.UniqueIDsManager.GetNextThingID();
        }

        public virtual void SetParent(IShieldParent shieldParent)
        {
            _parent = shieldParent;
        }

        public virtual bool IsActive()
        {
            return Parent?.ParentActive ?? true;
        }

        public abstract bool Collision(Vector3 point);

        public abstract Vector3? Collision(Ray ray, float limit);

        public abstract Vector3? Collision(Vector3 start, Vector3 end);

        public float CalculateDamage(ShieldDamages damages)
        {
            float total;
            if (Resists != null)
            {
                var oTotal = Resists.Apply(damages);
                if (oTotal == null) return 0f;
                total = oTotal.Value;
            }
            else
            {
                total = damages.Damage;
            }
            return total;
        }

        public virtual float SinkDamage(float damage)
        {
            if (!IsActive()) return 0f;
            return Parent?.SinkDamage(damage) ?? damage;
        }

        public float Block(ShieldDamages damages, Vector3 position)
        {
            return Block(CalculateDamage(damages), position);
        }
        
        public float Block(float damage, Vector3 position)
        {
            if (!IsActive()) return 0f;

            var handled = SinkDamage(damage);

            if (handled >= damage)
            {
                RenderImpactEffect(PositionUtility.ToVector2(position), Map);
                PlayBulletImpactSound(PositionUtility.ToVector2(position), Map);
            }

            return handled;
        }
        
        protected static void RenderImpactEffect(Vector2 position, Map map)
        {
            MoteMaker.ThrowLightningGlow(PositionUtility.ToVector3(position), map, 0.5f);
        }

        protected static void PlayBulletImpactSound(Vector2 position, Map map)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(PositionUtility.ToIntVec3(position), map));
        }

        public virtual void FieldPreDraw()
        {
        }

        public abstract void FieldDraw(CellRect cameraRect);

        public virtual void FieldPostDraw()
        {
        }

        public IEnumerable<Gizmo> ShieldGizmos
        {
            get
            {
                yield return new Command_Toggle
                {
                    icon = Resources.UiToggleVisibility,
                    defaultDesc = "fd.shield.render_field.description".Translate(),
                    defaultLabel = "fd.shield.render_field.label".Translate(),
                    isActive = () => _renderField,
                    toggleAction = () => _renderField = !_renderField
                };
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _id, "shieldRadialId");
            Scribe_References.Look(ref _parent, "shieldRadialParent");
            Scribe_Values.Look(ref _renderField, "renderField", true);
        }
        
        public string GetUniqueLoadID()
        {
            return ShieldLoadType + _id;
        }

        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case Comp_FlickBoard.SignalReset:
                    if(HasWantSettings)
                        Comp_FlickBoard.EmitWantFlick(this);
                    break;
            }
        }

        protected IShield TryGetParent()
        {
            switch (parent)
            {
                case IShield shield:
                    return shield;
            }

            return null;
        }

        public abstract void ClearWantSettings();
    }
}
