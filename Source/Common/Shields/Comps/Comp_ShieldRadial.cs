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
    public class CompProperties_ShieldRadial : CompProperties
    {
        public int minRadius;
        public int maxRadius;

        public float powerPerTile = 0.1f;

        public int warmupTicks;

        public int deploymentSize;

        public CompProperties_ShieldRadial()
        {
            compClass = typeof(Comp_ShieldRadial);
        }
    }

    public class Comp_ShieldRadial : ThingComp, IShield
    {
        private int? _id;
        private int _fieldRadius;
        private int _wantRadius;
        private int _cellCount;
        private bool _renderField = true;

        private int _warmingUpTicks;
        private bool _activeLastTick;

        private int _radiusLast;
        private CellRect? _cameraLast;
        private bool _renderLast = true;
        private IntVec3 _positionLast;

        private IShield _parent;

        public Faction Faction => parent.Faction;
        public Map Map => parent.Map;

        public float DeploymentSize => Props.deploymentSize;

        public string Label => parent.Label;

        public IShieldResists Resists => parent.TryGetComp<Comp_ShieldResistance>();

        private Vector3 ExactPosition => (PositionUtility.GetRealPosition(parent.holdingOwner.Owner) ?? parent.TrueCenter()).Yto0();

        private static int NextId => Find.UniqueIDsManager.GetNextThingID();

        private bool WantFlick => _wantRadius != _fieldRadius;

        public void SetParent(IShield shieldParent)
        {
            _parent = shieldParent;
        }
        
        public override void Initialize(CompProperties compProperties)
        {
            base.Initialize(compProperties);
            WantRadius = Props.maxRadius;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _cellCount = GenRadial.NumCellsInRadius(_fieldRadius);
            _positionLast = parent.Position;
            _radiusLast = (int)Radius;
            parent.Map.GetComponent<ShieldManager>().Add(this);
            LessonAutoActivator.TeachOpportunity(ConceptDef.Named("FD_Shields"), OpportunityType.Critical);
            if (_id == null) _id = NextId;
        }

        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<ShieldManager>().Del(this);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            previousMap.GetComponent<ShieldManager>().Del(this);
        }

        public int ProtectedCellCount => _cellCount;

        public float CellProtectionFactor => Props.powerPerTile;

        public CompProperties_ShieldRadial Props => 
            (CompProperties_ShieldRadial)props;

        public int WantRadius
        {
            get => _wantRadius;
            set
            {
                if (value < 0)
                {
                    _wantRadius = 0;
                }
                else if (value > Props.maxRadius)
                {
                    _wantRadius = Props.maxRadius;
                }
                else
                {
                    _wantRadius = value;
                }

                if (WantFlick)
                    Comp_FlickBoard.EmitWantFlick(this);
                else
                    Comp_FlickBoard.EmitWantReset(this);
            }
        }
        
        public float Radius
        {
            get
            {
                if (_warmingUpTicks > 0)
                {
                    var result = Mathf.Lerp(Props.maxRadius, 0f, 1.0f * _warmingUpTicks / Props.warmupTicks);
                    if(result < _fieldRadius) return result;
                    return _fieldRadius;
                }
                else
                {
                    return _fieldRadius;
                }
            }

            set
            {
                _fieldRadius = (int)value;
                _cellCount = GenRadial.NumCellsInRadius(value);
            }
        }

        public override void CompTick()
        {
            _positionLast = parent.Position;
            _radiusLast = (int) Radius;
            
            var active = IsActive();
            if (active != _activeLastTick)
            {
                if (active && _warmingUpTicks < Props.warmupTicks)
                {
                    _warmingUpTicks = Props.warmupTicks;
                }
            }
            _activeLastTick = active;

            if (active && _warmingUpTicks > 0)
            {
                _warmingUpTicks--;
            }
        }
        
        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case Comp_FlickBoard.SignalFlicked:
                    Radius = WantRadius;
                    break;
                case Comp_FlickBoard.SignalReset:
                    if(WantFlick)
                        parent.BroadcastCompSignal(Comp_FlickBoard.SignalWant);
                    break;
            }
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

        public IEnumerable<UiComponent> UiComponents
        {
            get
            {
                if (Props.minRadius != Props.maxRadius)
                {
                    yield return new IntSlider(
                        "radius.label".Translate(),
                        Props.minRadius,
                        Props.maxRadius,
                        () => WantRadius,
                        size => WantRadius = size);
                }
            }
        }

        public bool Collision(Vector3 vector)
        {
            return CollisionUtility.Circle.Point(ExactPosition.Yto0(), Radius, vector);
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return Collision(ray.origin, ray.GetPoint(limit));
        }

        public Vector3? Collision(Vector3 origin, Vector3 destination)
        {
            if (Mod.Settings.EnableShootingOut && Collision(origin))
                return null;
            return CollisionUtility.Circle.LineSegment(ExactPosition, Radius, origin, destination);
        }

        private bool ShouldDraw(CellRect cameraRect)
        {
            if (cameraRect == _cameraLast && parent.Position == _positionLast && (int) Radius == _radiusLast) return _renderLast;

            _cameraLast = cameraRect;

            var collides = CollisionUtility.Circle.CellRect(ExactPosition, Radius, cameraRect);
            _renderLast = collides;
            return collides;
        }

        public void FieldPreDraw()
        {
        }

        public void FieldDraw(CellRect cameraRect)
        {
            if (!IsActive() || !_renderField || !ShouldDraw(cameraRect)) return;
            var position = ExactPosition;
            position.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            var scalingFactor = (float)(Radius * 2.2);
            var scaling = new Vector3(scalingFactor, 1f, scalingFactor);
            var matrix = new Matrix4x4();
            matrix.SetTRS(position, Quaternion.AngleAxis(0, Vector3.up), scaling);
            Graphics.DrawMesh(MeshPool.plane10, matrix, Resources.ShieldMat, 0);
        }

        public void FieldPostDraw()
        {
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            GenDraw.DrawRadiusRing(parent.Position, _fieldRadius);
        }

        private IShield TryGetParent()
        {
            switch (parent)
            {
                case IShield shield:
                    return shield;
            }

            return null;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _id, "shieldRadialId");
            Scribe_Values.Look(ref _fieldRadius, "radius", Props.maxRadius);
            Scribe_Values.Look(ref _wantRadius, "shieldRadialWantRadius", Props.maxRadius);
            Scribe_Values.Look(ref _renderField, "renderField", true);
            Scribe_Values.Look(ref _warmingUpTicks, "warmingUpTicks");
            Scribe_Values.Look(ref _activeLastTick, "activeLastTick");
            Scribe_References.Look(ref _parent, "shieldRadialParent");
        }

        public bool IsActive()
        {
            return _parent?.IsActive() ?? true;
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

        public float SinkDamage(float damage)
        {
            return _parent?.SinkDamage(damage) ?? damage;
        }

        public float Block(float damage, Vector3 position)
        {
            if (!IsActive()) return 0f;

            var handled = SinkDamage(damage);

            if (handled >= damage)
            {
                RenderImpactEffect(PositionUtility.ToVector2(position));
                PlayBulletImpactSound(PositionUtility.ToVector2(position));
            }

            return handled;
        }

        public float Block(ShieldDamages damages, Vector3 position)
        {
            return Block(CalculateDamage(damages), position);
        }

        private void RenderImpactEffect(Vector2 position)
        {
            MoteMaker.ThrowLightningGlow(PositionUtility.ToVector3(position), parent.Map, 0.5f);
        }

        private void PlayBulletImpactSound(Vector2 position)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(PositionUtility.ToIntVec3(position), parent.Map));
        }

        public string GetUniqueLoadID()
        {
            return "ShieldRadial" + _id;
        }

        public IEnumerable<ShieldSetting> ShieldSettings
        {
            get
            {
                yield return new RadiusSetting(WantRadius);
                yield return new RenderFieldSetting(_renderField);
            }

            set => value.Do(Apply);
        }

        private void Apply(ShieldSetting setting)
        {
            switch (setting)
            {
                case RadiusSetting radiusSetting:
                    WantRadius = radiusSetting.Get();
                    break;
                case RenderFieldSetting renderFieldSetting:
                    _renderField = renderFieldSetting.Get();
                    break;
            }
        }
    }
    
}
