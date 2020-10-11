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

    public abstract class Comp_ShieldBase : ThingComp, IShield
    {
        private int? _id;
        private IShieldParent _parent;
        private bool _renderField = true;
        
        public virtual string Label => parent.Label;

        protected abstract string ShieldLoadType { get; }
        public Map Map => parent.Map;
        public virtual Faction Faction => parent.Faction;
        protected Vector3 ExactPosition => (PositionUtility.GetRealPosition(parent.holdingOwner.Owner) ?? parent.TrueCenter()).Yto0();
        protected virtual IShieldResists Resists => parent.TryGetComp<Comp_ShieldResistance>();
        public virtual float DeploymentSize => Props.deploymentSize;
        public abstract IEnumerable<UiComponent> UiComponents { get; }
        
        public abstract bool HasWantSettings { get; }
        
        public abstract IEnumerable<IShieldField> Fields { get; }

        public abstract IEnumerable<Thing> Things { get; }

        public IShieldParent Parent => _parent;

        protected bool RenderField => _renderField;

        public virtual IEnumerable<IShieldStatus> Status
        {
            get
            {
                foreach (var status in Parent.Status)
                {
                    yield return status;
                }
            }
        }

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
            Map.GetComponent<ShieldManager>().Add(Fields);
        }
        
        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<ShieldManager>().Del(Fields);
            base.PostDeSpawn(map);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            previousMap.GetComponent<ShieldManager>().Del(Fields);
            base.PostDestroy(mode, previousMap);
        }

        public virtual void SetParent(IShieldParent shieldParent)
        {
            _parent = shieldParent;
        }

        public virtual bool IsActive()
        {
            return Parent?.ParentActive ?? true;
        }

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

        public float Block(ShieldDamages damages, Vector3 position)
        {
            return Block(CalculateDamage(damages), position);
        }
        
        public float Block(float damage, Vector3 position)
        {
            if (!IsActive()) return 0f;
            var handled = Parent?.SinkDamage(damage) ?? 0f;
            if (Mathf.Abs(damage - handled) < 1)
            {
                RenderImpactEffect(PositionUtility.ToVector2(position), Map);
                PlayBulletImpactSound(PositionUtility.ToVector2(position), Map);
                return damage;
            }

            return handled;
        }
        
        protected virtual void RenderImpactEffect(Vector2 position, Map map)
        {
            MoteMaker.ThrowLightningGlow(PositionUtility.ToVector3(position), map, 0.5f);
        }

        protected virtual void PlayBulletImpactSound(Vector2 position, Map map)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(PositionUtility.ToIntVec3(position), map));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            return ShieldGizmos;
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

                foreach (var gizmo in Parent.ShieldGizmos)
                {
                    yield return gizmo;
                }
                
                if (Faction == Faction.OfPlayer)
                {
                    foreach (var gizmo in ShieldSettingsClipboard.Gizmos(this))
                    {
                        yield return gizmo;
                    }
                }
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

        public abstract void ClearWantSettings();
    }
}
