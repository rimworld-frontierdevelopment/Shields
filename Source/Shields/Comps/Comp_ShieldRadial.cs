using System;
using System.Collections.Generic;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields.CompProperties;
using FrontierDevelopments.Shields.Windows;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;


namespace FrontierDevelopments.Shields.Comps
{
    public class Comp_ShieldRadial : ThingComp, IShield
    {
        private int _fieldRadius;
        private int _cellCount;
        private bool _renderField = true;

        private CellRect? _cameraLast;
        private bool _renderLast = true;
        private IntVec3 _positionLast;

        private IEnergySource _energySource;
        private IHeatsink _heatSink;

        public Faction Faction => parent.Faction;

        private Vector3 ExactPosition => parent.TrueCenter();

        public override void Initialize(Verse.CompProperties compProperties)
        {
            base.Initialize(compProperties);
            Radius = Props.maxRadius;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _cellCount = GenRadial.NumCellsInRadius(_fieldRadius);
            _positionLast = parent.Position;
            _energySource = EnergySourceUtility.Find(parent);
            _heatSink = HeatsinkUtility.Find(parent);
            parent.Map.GetComponent<ShieldManager>().Add(this);
            LessonAutoActivator.TeachOpportunity(ConceptDef.Named("FD_Shields"), OpportunityType.Critical);
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

        public CompProperties_ShieldRadial Props => 
            (CompProperties_ShieldRadial)props;

        public int Radius
        {
            get => _fieldRadius;
            set
            {
                if (value < 0)
                {
                    _fieldRadius = 0;
                    return;
                }
                if (value > Props.maxRadius)
                {
                    _fieldRadius = Props.maxRadius;
                    return;
                }
                _fieldRadius = value;
                _cellCount = GenRadial.NumCellsInRadius(_fieldRadius);
            }
        }

        public override void CompTick()
        {
            _positionLast = parent.Position;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var current in base.CompGetGizmosExtra())
                yield return current;
            
            yield return new Command_Toggle
            {
                icon = Resources.UiToggleVisibility,
                defaultDesc = "fd.shield.render_field.description".Translate(),
                defaultLabel = "fd.shield.render_field.label".Translate(),
                isActive = () => _renderField,
                toggleAction = () => _renderField = !_renderField
            };

            if (parent.Faction == Faction.OfPlayer)
            {
                if (Props.minRadius != Props.maxRadius)        
                {
                    yield return new Command_Action
                    {
                        icon = Resources.UiSetRadius,
                        defaultDesc = "radius.description".Translate(),
                        defaultLabel = "radius.label".Translate(),
                        activateSound = SoundDef.Named("Click"),
                        action = () => Find.WindowStack.Add(new Popup_IntSlider("radius.label".Translate(), Props.minRadius, Props.maxRadius, () => Radius, size =>  Radius = size))
                    };
                }
            }
        }

        public bool Collision(Vector3 vector)
        {
            return Vector3.Distance(Common.ToVector3(ExactPosition), vector) < _fieldRadius + 0.5f;
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return Collision(ray.origin, ray.GetPoint(limit));
        }

        public Vector3? Collision(Vector3 origin, Vector3 destination)
        {
            var circleOrigin = Common.ToVector3(ExactPosition);

            var d = destination - origin;
            var f = origin - circleOrigin;
            
            var a = Vector3.Dot(d, d);
            var b = Vector3.Dot(2*f, d) ;
            var c = Vector3.Dot(f, f) - _fieldRadius * _fieldRadius;
            
            var discriminant = b*b-4*a*c;

            if (discriminant < 0) return null;

            // ray didn't totally miss sphere,
            // so there is a solution to
            // the equation.
            discriminant = (float)Math.Sqrt(discriminant);

            // either solution may be on or off the ray so need to test both
            // t1 is always the smaller value, because BOTH discriminant and
            // a are nonnegative.
            var t1 = (-b - discriminant)/(2*a);
            var t2 = (-b + discriminant)/(2*a);

            // 3x HIT cases:
            //          -o->             --|-->  |            |  --|->
            // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

            // 3x MISS cases:
            //       ->  o                     o ->              | -> |
            // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

            if( t1 >= 0 && t1 <= 1 )
            {
                // t1 is the intersection, and it's closer than t2
                // (since t1 uses -b - discriminant)
                // Impale, Poke
                return new Vector3(origin.x + t1 * d.x, origin.y + t1 * d.y, origin.z + t1 * d.z);
            }

            // here t1 didn't intersect so we are either started
            // inside the sphere or completely past it
            if( t2 >= 0 && t2 <= 1 )
            {
                // ExitWound
                return new Vector3(origin.x + t1 * d.x, origin.y + t1 * d.y, origin.z + t1 * d.z);
            }

            // no intersection: FallShort, Past, CompletelyInside
            return null;
        }

        private bool Collides(CellRect rect)
        {
            var position = parent.Position;
            if (rect.minX <= position.x
                && position.x <= rect.maxX
                && rect.minZ <= position.z
                && position.z <= rect.maxZ) return true;

            var a = new Vector3(rect.minX + 0.5f, 0, rect.minZ + 0.5f);
            var b = new Vector3(rect.minX + 0.5f, 0, rect.maxZ + 0.5f);
            var c = new Vector3(rect.maxX + 0.5f, 0, rect.maxZ + 0.5f);
            var d = new Vector3(rect.maxX + 0.5f, 0, rect.minZ + 0.5f);

            return Collision(a, b) != null
                         || Collision(b, c) != null
                         || Collision(c, d) != null
                         || Collision(d, a) != null;
        }

        private bool ShouldDraw(CellRect cameraRect)
        {
            if (cameraRect == _cameraLast && parent.Position == _positionLast) return _renderLast;

            _cameraLast = cameraRect;

            var collides = Collides(cameraRect);
            _renderLast = collides;
            return collides;
        }

        public void Draw(CellRect cameraRect)
        {
            if (!IsActive() || !_renderField || !ShouldDraw(cameraRect)) return;
            var position = Common.ToVector3(ExactPosition);
            position.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            var scalingFactor = (float)(_fieldRadius * 2.2);
            var scaling = new Vector3(scalingFactor, 1f, scalingFactor);
            var matrix = new Matrix4x4();
            matrix.SetTRS(position, Quaternion.AngleAxis(0, Vector3.up), scaling);
            Graphics.DrawMesh(MeshPool.plane10, matrix, Resources.ShieldMat, 0);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            GenDraw.DrawRadiusRing(parent.Position, _fieldRadius);
        }
        
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _fieldRadius, "radius", Props.maxRadius);
            Scribe_Values.Look(ref _renderField, "renderField", true);
        }

        public bool IsActive()
        {
            return _energySource.IsActive() && (_heatSink != null && !_heatSink.OverTemperature || _heatSink == null);
        }

        public bool Block(long damage, Vector3 position)
        {
            if (!IsActive()) return false;
            var charge = damage * Mod.Settings.PowerPerDamage;
            if (Mod.Settings.ScaleOnHeat && _heatSink != null) charge = charge * Mathf.Pow(1.01f, _heatSink.Temp);
            var drawn = _energySource.Draw(charge);
            _heatSink?.PushHeat(drawn / 60000 * Mod.Settings.HeatPerPower);
            RenderImpactEffect(Common.ToVector2(position));
            PlayBulletImpactSound(Common.ToVector2(position));
            return drawn >= charge;
        }

        private void RenderImpactEffect(Vector2 position)
        {
            MoteMaker.ThrowLightningGlow(Common.ToVector3(position), parent.Map, 0.5f);
        }

        private void PlayBulletImpactSound(Vector2 position)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Common.ToIntVec3(position), parent.Map));
        }
    }
}