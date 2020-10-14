using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Comps;
using FrontierDevelopments.General.UI;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Comps
{
    public abstract class CompProperties_ShieldBase : CompProperties {
        public int deploymentSize;
    }

    public abstract class Comp_ShieldBase : ThingComp, IShield, IFlickBoardSwitch
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

        public virtual void NotifyWantSettings()
        {
            FlickBoardUtility.FindBoard(parent)?.Notify_Want(HasWantSettings);
        }

        public abstract IEnumerable<IShieldField> Fields { get; }

        public virtual Thing Thing => parent;

        public IShieldParent Parent => _parent;

        protected virtual bool RenderField
        {
            get => _renderField;
            set => _renderField = value;
        } 

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

        public virtual string TextStats => Parent.TextStats;

        public virtual IEnumerable<ShieldSetting> ShieldSettings
        {
            get
            {
                yield return new RenderFieldSetting(RenderField);
            } 
            
            set => value.Do(Apply);
        }

        protected virtual void Apply(ShieldSetting setting)
        {
            switch (setting)
            {
                case RenderFieldSetting renderFieldSetting:
                    RenderField = renderFieldSetting.Get();
                    break;
            }
        }

        public abstract bool PresentOnMap(Map map);

        private CompProperties_ShieldBase Props => (CompProperties_ShieldBase) props;
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (_id == null) _id = Find.UniqueIDsManager.GetNextThingID();
            var manager = ShieldManager.For(Map);
            manager.Add(Fields);
            manager.Add(this);
        }

        public override void PostDeSpawn(Map map)
        {
            var manager = ShieldManager.For(map);
            manager.Del(Fields);
            manager.Del(this);
            base.PostDeSpawn(map);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            var manager = ShieldManager.For(previousMap);
            manager.Del(Fields);
            manager.Del(this);
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
                if(PresentOnMap(Find.CurrentMap))
                {
                    RenderImpactEffect(PositionUtility.ToVector2(position), Find.CurrentMap);
                    PlayBulletImpactSound(PositionUtility.ToVector2(position), Find.CurrentMap);
                }

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
                    isActive = () => RenderField,
                    toggleAction = () => RenderField = !RenderField
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
            
                if (parent.Faction == Faction.OfPlayer)
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

        public virtual bool WantFlick => HasWantSettings;
        public abstract void Notify_Flicked();

        public abstract void ClearWantSettings();
    }
}
