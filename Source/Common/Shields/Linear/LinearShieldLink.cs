using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Linear
{
    public class LinearShieldLink : IShieldField, IExposable
    {
        private Thing _oneParent;
        private Thing _twoParent;

        private bool _wantUnlink;
        private int _cellCount;

        public bool FieldRenderOverride { get; set; }

        private ILinearShield One { get; set; }

        private ILinearShield Two { get; set; }

        public bool FieldBlocked => FieldBlockedAt.Any();

        private List<IntVec3> FieldBlockedAt { get; set; } = new List<IntVec3>();

        public float Distance { get; private set; }

        public bool RenderField { get; set; }

        public int ProtectedCellCount => IsActive() ? _cellCount : 0;

        public float Efficiency { get; private set; }

        public float CellProtectionFactor { get; private set; }

        public Map Map => One.Map;

        public bool PresentOnMap(Map map) => Map == map;

        public Faction Faction => One.Faction;

        public bool HasWantSettings => WantUnlink;

        public IEnumerable<ILinearShield> Shields
        {
            get
            {
                yield return One;
                yield return Two;
            }
        }

        public bool WantUnlink
        {
            get => _wantUnlink;
            set => _wantUnlink = value;
        }

        public bool IsActive() => !FieldBlocked && One.IsActive() && Two.IsActive();

        public bool Has(ILinearShield shield) => One == shield || Two == shield;

        public IEnumerable<IShield> Emitters
        {
            get
            {
                yield return One;
                yield return Two;
            }
        }

        private IEnumerable<IntVec3> CellsCovered => LinearShieldUtility.CellsBetween(One.Position, Two.Position);

        public LinearShieldLink()
        {
            RenderField = true;
        }

        public LinearShieldLink(ILinearShield one, ILinearShield two)
        {
            Log.Message("new link " + one + " " + two);

            One = one;
            Two = two;

            _oneParent = one.Thing;
            _twoParent = two.Thing;

            RenderField = true;

            Init();
            
            ShieldManager.For(Map).Add(this);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref _oneParent, "one");
            Scribe_References.Look(ref _twoParent, "two");
            Scribe_Values.Look(ref _wantUnlink, "wantUnlink");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                One = LinearShieldUtility.Find(_oneParent);
                Two = LinearShieldUtility.Find(_twoParent);
                Init();
            }
        }

        private void Init()
        {
            Distance = One.Position.DistanceTo(Two.Position);
            _cellCount = Mathf.CeilToInt(Distance / 2f);
            CellProtectionFactor = LinearShieldUtility.CellProtectionFactorCost(Distance);
            Efficiency = LinearShieldUtility.CalculateFieldEfficiency(Distance, One.EfficientRange, Two.EfficientRange);
            
            One.Add(this);
            Two.Add(this);
            
            NotifyLinked();
        }

        public ILinearShield Other(ILinearShield shield)
        {
            if (One == shield)
                return Two;
            if (Two == shield)
                return One;
            return null;
        }

        public void Unlink()
        {
            One.UnlinkFrom(Two);
            Two.UnlinkFrom(One);

            ShieldManager.For(Map).Del(this);

            Emitters.Do(emitter => emitter.NotifyWantSettings());
        }
        
        public bool Collision(Vector3 point)
        {
            return false;
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return Collision(ray.origin, ray.GetPoint(limit));
        }

        public Vector3? Collision(Vector3 start, Vector3 end)
        {
            if (!IsActive()) return null;
            // TODO add a gap so that the emitter is exposed
            return CollisionUtility.LineSegment.Other(start, end, One.TrueCenter, Two.TrueCenter);
        }

        public float Block(float damage, Vector3 position)
        {
            var adjusted = damage * Efficiency;
            
            var blocked = One.Block(adjusted / 2, position) 
                          + Two.Block(adjusted / 2, position);

            var unAdjusted = blocked / Efficiency;

            if (Mathf.Abs(damage - unAdjusted) < 1)
            {
                return damage;
            }

            return unAdjusted;
        }

        public float Block(ShieldDamages damages, Vector3 position)
        {
            damages.Factor *= Efficiency;
            damages.Factor *= 0.5f;

            var blocked = One.Block(damages, position) 
                          + Two.Block(damages, position);

            var unAdjusted = blocked / Efficiency;
            var damageSum = damages.Damage;
            if (Mathf.Abs(damages.Damage - unAdjusted) < 1)
            {
                return damageSum;
            }

            return unAdjusted;
        }

        public void Tick()
        {
            var lastBlocked = FieldBlocked;
            FieldBlockedAt = LinearShieldUtility.BlockingBetween(One, Two).ToList();

            if (lastBlocked != FieldBlocked)
            {
                if (FieldBlocked)
                {
                    Messages.Message(
                        "fd.shields.pylon.blocked.body".Translate(),
                        FieldBlockedAt.Select(cell => new TargetInfo(cell, Map)).ToList(),
                        MessageTypeDefOf.NegativeEvent);
                }
                else
                {
                    Messages.Message(
                        "fd.shields.pylon.unblocked.body".Translate(),
                        Emitters.Select(emitter => emitter.Thing).Select(thing => new TargetInfo(thing)).ToList(),
                        MessageTypeDefOf.NeutralEvent);
                }
            }
        }

        public void FieldPreDraw()
        {
        }

        public void FieldDraw(CellRect cameraRect)
        {
            if (IsActive() && RenderField)
            {
                GraphicsUtility.DrawLine(
                    One.TrueCenter, 
                    Two.TrueCenter,
                    Resources.SolidLine,
                    AltitudeLayer.MoteOverhead.AltitudeFor(), 
                    0.4f);
            }
        }

        public void FieldPostDraw()
        {
        }

        public void DrawFieldCells()
        {
            LinearShieldUtility.DrawFieldBetween(CellsCovered.ToList(), Map, Color.white);
        }

        public void ClearWantSettings()
        {
            WantUnlink = false;
            Emitters.Do(emitter => emitter.NotifyWantSettings());
        }

        public void Flicked()
        {
            if(WantUnlink) Unlink();
        }

        private void NotifyLinked()
        {
            One.NotifyLinked(Two);
            Two.NotifyLinked(One);
        }
    }
}