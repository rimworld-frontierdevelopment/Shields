using System;
using System.Collections.Generic;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Comps;
using FrontierDevelopments.General.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Comps
{
    public class CompProperties_ShieldRadial : CompProperties_ShieldBase
    {
        public int minRadius;
        public int maxRadius;
        
        public float powerPerTile = 0.1f;

        public int ticksPerExpansion;

        public CompProperties_ShieldRadial()
        {
            compClass = typeof(Comp_ShieldRadial);
        }
    }

    public class Comp_ShieldRadial : Comp_ShieldBase, IShieldField
    {
        private int _fieldRadius;
        private int _wantRadius;
        private int _cellCount;

        private int _warmingUpTicks;
        private bool _activeLastTick;

        private int _radiusLast;
        private CellRect? _cameraLast;
        private bool _renderLast = true;
        private IntVec3 _positionLast;

        private bool _resizingShield;

        protected override string ShieldLoadType => "ShieldRadial";

        public override bool HasWantSettings => _wantRadius != _fieldRadius;

        public int ProtectedCellCount => _cellCount;

        public float CellProtectionFactor => Props.powerPerTile;

        private CompProperties_ShieldRadial Props => (CompProperties_ShieldRadial) props;

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

                if (HasWantSettings)
                    Comp_FlickBoard.EmitWantFlick(this);
                else
                    Comp_FlickBoard.EmitWantReset(this);
            }
        }
        
        public float Radius
        {
            get
            {
                if (_warmingUpTicks != 0)
                {
                    var current = ((float)_fieldRadius * Props.ticksPerExpansion - _warmingUpTicks) / Props.ticksPerExpansion;
                    var difference = _fieldRadius - current;
                    var factor = difference / _warmingUpTicks / difference;
                    if (_warmingUpTicks > 0)
                    {
                        return Mathf.Lerp(
                            Math.Min(_fieldRadius, current),
                            Math.Max(_fieldRadius, current),
                            factor);
                    } 
                    return Mathf.Lerp(
                        Math.Max(_fieldRadius, current),
                        Math.Min(_fieldRadius, current),
                        factor);
                }
                return _fieldRadius;
            }

            set
            {
                var difference = value - _fieldRadius;
                if (difference != 0)
                {
                    _fieldRadius = (int)value;
                    _warmingUpTicks = (int)difference * Props.ticksPerExpansion;
                    _cellCount = GenRadial.NumCellsInRadius(value);
                }
            }
        }
        
        public override void Initialize(CompProperties compProperties)
        {
            base.Initialize(compProperties);
            WantRadius = Props.maxRadius;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _fieldRadius = Props.maxRadius;
            _cellCount = GenRadial.NumCellsInRadius(_fieldRadius);
            _positionLast = parent.Position;
            _radiusLast = (int)Radius;
            LessonAutoActivator.TeachOpportunity(ConceptDef.Named("FD_Shields"), OpportunityType.Critical);
        }

        public override void CompTick()
        {
            _positionLast = parent.Position;
            _radiusLast = (int) Radius;
            
            var active = IsActive();
            if (active != _activeLastTick)
            {
                if (active)
                {
                    _warmingUpTicks = _fieldRadius * Props.ticksPerExpansion;
                }
            }
            _activeLastTick = active;

            if (active)
            {
                if (_warmingUpTicks > 0)
                {
                    _warmingUpTicks--;
                } else if (_warmingUpTicks < 0)
                {
                    _warmingUpTicks++;
                }
            }
        }
        
        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            switch (signal)
            {
                case Comp_FlickBoard.SignalFlicked:
                    Radius = WantRadius;
                    break;
            }
        }

        public override IEnumerable<UiComponent> UiComponents
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
                        size => WantRadius = size,
                        hasMouse => _resizingShield = hasMouse);
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
            if (!IsActive() || !RenderField || !ShouldDraw(cameraRect)) return;
            var position = PositionUtility.ToVector3(ExactPosition);
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
            GenDraw.DrawRadiusRing(parent.Position, _resizingShield ? WantRadius : _fieldRadius);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _fieldRadius, "radius", Props.maxRadius);
            Scribe_Values.Look(ref _wantRadius, "shieldRadialWantRadius", Props.maxRadius);
            Scribe_Values.Look(ref _warmingUpTicks, "warmingUpTicks");
            Scribe_Values.Look(ref _activeLastTick, "activeLastTick");
        }

        public override IEnumerable<ShieldSetting> ShieldSettings
        {
            get
            {
                foreach (var setting in base.ShieldSettings)
                    yield return setting;
                
                yield return new RadiusSetting(WantRadius);
            }
        }

        protected override void Apply(ShieldSetting setting)
        {
            base.Apply(setting);
            switch (setting)
            {
                case RadiusSetting radiusSetting:
                    WantRadius = radiusSetting.Get();
                    break;
            }
        }

        public override void ClearWantSettings()
        {
            WantRadius = _fieldRadius;
        }

        public IEnumerable<IShield> Emitters
        {
            get
            {
                yield return this;
            }
        }

        public override IEnumerable<IShieldField> Fields
        {
            get
            {
                yield return this;
            }
        }

        public override IEnumerable<Thing> Things
        {
            get
            {
                yield return parent;
            }
        }
    }
    
}
