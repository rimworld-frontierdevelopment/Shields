using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General.Comps;
using FrontierDevelopments.General.UI;
using FrontierDevelopments.Shields.Comps;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Linear
{
    public class CompProperties_ShieldLinear : CompProperties_ShieldBase
    {
        public int efficientRange = 0;
        public int maximumLinks = 2;

        public CompProperties_ShieldLinear()
        {
            compClass = typeof(Comp_ShieldLinear);
        }
    }

    public class Comp_ShieldLinear : Comp_ShieldBase, ILinearShield
    {
        public class ShieldStatusLinked : IShieldStatus
        {
            public bool Online => true;
            public string Description => "FrontierDevelopments.General.Linked".Translate();
        }

        public class ShieldStatusUnlinked : IShieldStatus
        {
            public bool Online => false;
            public string Description => "FrontierDevelopments.General.Unlinked".Translate();
        }

        public class ShieldStatusLinkBlocked : IShieldStatus
        {
            public bool Online => false;
            public string Description => "FrontierDevelopments.Shields.Linear.FieldBlocked".Translate();
        }

        private readonly HashSet<LinearShieldLink> _links = new HashSet<LinearShieldLink>();
        private HashSet<Thing> _wantLinks = new HashSet<Thing>();

        private readonly HashSet<Thing> _wantRenderFieldOverride = new HashSet<Thing>();

        public IntVec3 Position => parent.Position;

        public Vector3 TrueCenter => parent.DrawPos;

        public int EfficientRange => Props.efficientRange;

        private bool HasLinkCapacity => _links.Count + _wantLinks.Count < Props.maximumLinks;
        
        public override IEnumerable<IShieldField> Fields => _links;

        public void Add(LinearShieldLink link)
        {
            _links.Add(link);
        }

        protected override string ShieldLoadType => "ShieldLinear";

        public override bool HasWantSettings => _wantLinks.Any() || _links.Any(link => link.HasWantSettings);

        private CompProperties_ShieldLinear Props => (CompProperties_ShieldLinear) props;

        private bool LinkedWith(ILinearShield other) => _links.Any(link => link.Has(other));
        
        private bool IsLinked => _links.Count > 0;

        public override bool PresentOnMap(Map map) => map == Map;
        public override void ClearWantSettings()
        {
            _wantLinks.Select(LinearShieldUtility.Find).ToList().Do(RemoveWantLinkWith);
            _wantLinks.Clear();
            _links.Do(link => link.ClearWantSettings());
        }

        public override IEnumerable<IShieldStatus> Status
        {
            get
            {
                foreach (var status in Parent.Status)
                {
                    yield return status;
                }

                if (IsLinked)
                {
                    yield return new ShieldStatusLinked();

                    if (_links.Any(link => link.FieldBlocked))
                        yield return new ShieldStatusLinkBlocked();

                }
                else if(_wantLinks.Count < 1)
                {
                    yield return new ShieldStatusUnlinked();
                }
            }
        }

        protected override bool RenderField
        {
            get => base.RenderField;
            set => SetRenderField(value);
        }

        private void SetRenderField(bool value)
        {
            base.RenderField = value;
            _links.Do(link => link.RenderField = value);
        }

        public bool CanLinkWith(ILinearShield other)
        {
            return HasLinkCapacity && CanCreateLinkWith(other);
        }

        private bool CanCreateLinkWith(ILinearShield other)
        {
            return this != other && !LinkedWith(other);
        }

        private LinearShieldLink CreateLinkWith(ILinearShield other)
        {
            if (!CanCreateLinkWith(other)) return null;
            return new LinearShieldLink(this, other);
        }

        private void UnlinkAll()
        {
            _links.ToList().Do(link => link.Unlink());
        }

        public override void PostDeSpawn(Map map)
        {
            if (IsLinked)
            {
                IEnumerable<TargetInfo> Targets()
                {
                    yield return new TargetInfo(parent.Position, Map);

                    foreach (var thing in _links.Select(link => link.Other(this)).Select(link => link.Thing))
                    {
                        yield return new TargetInfo(thing);
                    }
                }
                Messages.Message(
                    "fd.shields.pylon.destroyed.body".Translate(),
                    Targets().ToList(),
                    MessageTypeDefOf.NegativeEvent);
            }
            UnlinkAll();
            base.PostDeSpawn(map);
        }

        public void NotifyLinked(ILinearShield other)
        {
            _wantLinks.Remove(other.Thing);
            FlickBoardUtility.FindBoard(parent)?.Notify_Want(HasWantSettings);
        }

        public override void Notify_Flicked()
        {
            _links.ToList().Do(link => link.Flicked());
            _wantLinks.ToList().Select(LinearShieldUtility.Find).Do(other => CreateLinkWith(other));
        }

        public override bool IsActive()
        {
            return IsLinked && _links.Any(link => !link.FieldBlocked) && (Parent?.ParentActive ?? true);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            var hasOverrides = _links.Any(link => link.FieldRenderOverride) || _wantRenderFieldOverride.Count > 0;
            if (hasOverrides)
            {
                foreach (var link in _links.Where(link => link.FieldRenderOverride))
                {
                    link.DrawFieldCells();
                }

                foreach (var wantLink in _wantRenderFieldOverride)
                {
                    LinearShieldUtility.DrawFieldBetween(Position, wantLink.Position, Map, Color.grey);
                }
            }
            else
            {
                foreach (var link in _links)
                {
                    link.DrawFieldCells();
                }
                
                foreach (var wantLink in _wantLinks.Select(LinearShieldUtility.Find))
                {
                    LinearShieldUtility.DrawFieldBetween(Position, wantLink.Position, Map, Color.grey);
                }
            }
        }

        private ShieldLinearWantLinkUiComponent WantLinkUiComponent(ILinearShield wantLink)
        {
            return new ShieldLinearWantLinkUiComponent(
                this,
                wantLink,
                (value) =>
                {
                    if (value)
                    {
                        _wantRenderFieldOverride.Add(wantLink.Thing);
                    }
                    else
                    {
                        _wantRenderFieldOverride.Remove(wantLink.Thing);
                    }
                },
                () =>
                {
                    // get rid of this since the UI component will be removed before the override can update
                    _wantRenderFieldOverride.Remove(wantLink.Thing); 
                    RemoveWantLinkWith(wantLink);
                });
        }

        public override IEnumerable<UiComponent> UiComponents
        {
            get
            {
                if (_wantLinks.Any())
                {
                    var wantLinkComponents = _wantLinks
                        .Select(LinearShieldUtility.Find)
                        .Where(thing => thing != null)
                        .Select(WantLinkUiComponent)
                        .OfType<UiComponent>() // for generic typing
                        .ToList();
                
                    yield return new TitledSectionComponent(
                        "FrontierDevelopments.Shields.ITab.PendingLinks".Translate(), 
                        wantLinkComponents);
                }

                if (_links.Any())
                {
                    yield return new TitledSectionComponent(
                        "FrontierDevelopments.Shields.ITab.ActiveLinks".Translate(), 
                        _links.Select(link => new ShieldLinearLinkUiComponent(link, this)).OfType<UiComponent>().ToList());
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var current in base.CompGetGizmosExtra())
                yield return current;

            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    // icon = (Texture2D)Resources.ShieldPylon.mainTexture,
                    defaultDesc = "fd.shields.linear.link.description".Translate(),
                    defaultLabel = "fd.shields.linear.link.label".Translate(),
                    disabled = !HasLinkCapacity,
                    disabledReason = "fd.shields.linear.link.disabled".Translate(Props.maximumLinks),
                    action = TargetOther
                };
            }
        }

        private void TargetOther()
        {
            Find.Targeter.BeginTargeting(ShieldTargetingParams.LinearLink(this), (other) =>
            {
                var shield = LinearShieldUtility.Find(other.Thing);
                if (shield != null)
                {
                    WantLink(shield);
                }
            });
        }

        public void WantLinkWith(ILinearShield other)
        {
            _wantLinks.Add(other.Thing);
            FlickBoardUtility.FindBoard(parent)?.Notify_Want(HasWantSettings);
        }

        private void WantLink(ILinearShield other)
        {
            WantLinkWith(other);
            other.WantLinkWith(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref _wantLinks, "wantLinks", LookMode.Reference);
        }

        public void UnlinkFrom(ILinearShield other)
        {
            var linksToRemove = _links.Where(link => link.Has(other)).ToList();
            linksToRemove.Do(link => _links.Remove(link));
            linksToRemove.Do(link => link.Unlink());
        }

        public void RemoveWantLinkWith(ILinearShield other)
        {
            // prevents infinite recursion
            if (_wantLinks.Remove(other.Thing))
            {
                other.RemoveWantLinkWith(this);
            }
            FlickBoardUtility.FindBoard(parent)?.Notify_Want(HasWantSettings);
        }
    }
}
