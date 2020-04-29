using FrontierDevelopments.General;
using FrontierDevelopments.General.Windows;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Comps
{
    public class CompProperties_ShieldRadial : CompProperties
    {
        public int minRadius;
        public int maxRadius;

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

        public float DeploymentSize => Props.deploymentSize;

        public string Label => parent.Label;
        public IEnumerable<Gizmo> ShieldGizmos => CompGetGizmosExtra();

        public IShieldResists Resists => parent.TryGetComp<Comp_ShieldResistance>();

        private Vector3 ExactPosition => PositionUtility.GetRealPosition(parent.holdingOwner.Owner) ?? parent.TrueCenter();

        private static int NextId => Find.UniqueIDsManager.GetNextThingID();

        public void SetParent(IShield shieldParent)
        {
            _parent = shieldParent;
        }

        public override void Initialize(CompProperties compProperties)
        {
            base.Initialize(compProperties);
            SetRadius = Props.maxRadius;
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

        public CompProperties_ShieldRadial Props =>
            (CompProperties_ShieldRadial)props;

        public float SetRadius
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
                _fieldRadius = (int)value;
                _cellCount = GenRadial.NumCellsInRadius(_fieldRadius);
            }
        }

        public float Radius
        {
            get
            {
                if (_warmingUpTicks > 0)
                {
                    var result = Mathf.Lerp(Props.maxRadius, 0f, 1.0f * _warmingUpTicks / Props.warmupTicks);
                    if (result < _fieldRadius) return result;
                    return _fieldRadius;
                }
                else
                {
                    return _fieldRadius;
                }
            }
        }

        public override void CompTick()
        {
            _positionLast = parent.Position;
            _radiusLast = (int)Radius;

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
                        action = () => Find.WindowStack.Add(new Popup_IntSlider("radius.label".Translate(), Props.minRadius, Props.maxRadius, () => (int)SetRadius, size => SetRadius = size))
                    };
                }
            }
        }

        public bool Collision(Vector3 vector)
        {
            return Vector3.Distance(PositionUtility.ToVector3(ExactPosition), vector) < Radius + 0.5f;
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return Collision(ray.origin, ray.GetPoint(limit));
        }

        public Vector3? Collision(Vector3 origin, Vector3 destination)
        {
            var circleOrigin = PositionUtility.ToVector3(ExactPosition);

            var radius = Radius;

            var d = destination - origin;
            var f = origin - circleOrigin;

            var a = Vector3.Dot(d, d);
            var b = Vector3.Dot(2 * f, d);
            var c = Vector3.Dot(f, f) - radius * radius;

            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0) return null;

            // ray didn't totally miss sphere,
            // so there is a solution to
            // the equation.
            discriminant = (float)Math.Sqrt(discriminant);

            // either solution may be on or off the ray so need to test both
            // t1 is always the smaller value, because BOTH discriminant and
            // a are nonnegative.
            var t1 = (-b - discriminant) / (2 * a);
            var t2 = (-b + discriminant) / (2 * a);

            // 3x HIT cases:
            //          -o->             --|-->  |            |  --|->
            // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit),

            // 3x MISS cases:
            //       ->  o                     o ->              | -> |
            // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

            if (t1 >= 0 && t1 <= 1)
            {
                // t1 is the intersection, and it's closer than t2
                // (since t1 uses -b - discriminant)
                // Impale, Poke
                return new Vector3(origin.x + t1 * d.x, origin.y + t1 * d.y, origin.z + t1 * d.z);
            }

            // here t1 didn't intersect so we are either started
            // inside the sphere or completely past it
            if (t2 >= 0 && t2 <= 1)
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
            if (cameraRect == _cameraLast && parent.Position == _positionLast && (int)Radius == _radiusLast) return _renderLast;

            _cameraLast = cameraRect;

            var collides = Collides(cameraRect);
            _renderLast = collides;
            return collides;
        }

        public void Draw(CellRect cameraRect)
        {
            if (!IsActive() || !_renderField || !ShouldDraw(cameraRect)) return;

            var position = PositionUtility.ToVector3(ExactPosition);
            position.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            var scalingFactor = (float)(Radius * 2.2);
            var scaling = new Vector3(scalingFactor, 1f, scalingFactor);
            var matrix = new Matrix4x4();
            matrix.SetTRS(position, Quaternion.AngleAxis(0, Vector3.up), scaling);
            Graphics.DrawMesh(MeshPool.plane10, matrix, Resources.ShieldMat, 0);
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
    }
}