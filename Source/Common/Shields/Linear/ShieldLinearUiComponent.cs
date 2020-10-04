using System;
using System.Linq;
using System.Text;
using FrontierDevelopments.General.UI;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Linear
{
    public abstract class ShieldLinearUiComponent : UiComponent
    {
        protected abstract ILinearShield Other { get; }
        
        protected abstract string ButtonKey { get; }

        protected abstract float PowerNeeded { get; }

        protected abstract float Efficiency { get; }

        protected abstract Action<bool> FieldRenderOverride { get; }

        protected abstract Action ButtonAction { get; }

        protected abstract bool Blocked { get; }

        public int Height => Other.TextStats.Split('\n').Length * 20;

        public void Draw(Rect rect)
        {
            var fifth = rect.width / 5;
            var height = 0f;

            var linkRect = new Rect(0, height, fifth * 2, Height);
            var otherShieldRect = new Rect(fifth * 2, height, fifth * 2, Height);

            var buttonRect = new Rect(fifth * 4, height, fifth, Height);

            var linkListing = new Listing_Standard();
            linkListing.Begin(linkRect);

            var twoLabel = Other.Label;
            var twoPosition = Other.Position;

            // link
            var text = new StringBuilder();
            text.AppendLine("FrontierDevelopments.Shields.ITab.LinkWith".Translate(twoLabel, twoPosition.x, twoPosition.z));
            text.AppendLine("PowerNeeded".Translate() + ": " + PowerNeeded.ToString("#####0") + " W");
            text.AppendLine("FrontierDevelopments.Shields.ITab.Efficiency".Translate((int)(Efficiency * 100)));
            if(Blocked) text.AppendLine("FrontierDevelopments.Shields.Linear.FieldBlocked".Translate());
            linkListing.Label(text.ToString().TrimEndNewlines());
            linkListing.End();

            var shieldListing = new Listing_Standard();
            shieldListing.Begin(otherShieldRect);

            shieldListing.Label(Other.TextStats);
            shieldListing.End();

            FieldRenderOverride.Invoke(rect.Contains(Event.current.mousePosition));
            if (Widgets.ButtonText(buttonRect, ButtonKey.Translate()))
            {
                ButtonAction.Invoke();
            }
        }
    }
    
    public class ShieldLinearLinkUiComponent : ShieldLinearUiComponent
    {
        private LinearShieldLink Link { get; }

        public ShieldLinearLinkUiComponent(LinearShieldLink link, ILinearShield self)
        {
            Link = link;
            Self = self;
        }

        private ILinearShield Self { get; }
        protected override ILinearShield Other => Link.Other(Self);
        protected override string ButtonKey => "fd.shields.linear.unlink.label";
        protected override float PowerNeeded => Link.CellProtectionFactor * Link.Distance;
        protected override float Efficiency => Link.Efficiency;

        protected override bool Blocked => Link.FieldBlocked;

        protected override Action<bool> FieldRenderOverride => value =>
        {
            Link.FieldRenderOverride = value;
        };

        protected override Action ButtonAction => () =>
        {
            Link.WantUnlink = true;
            Link.Emitters.Do(emitter => emitter.NotifyWantSettings());
        };
    }

    public class ShieldLinearWantLinkUiComponent : ShieldLinearUiComponent
    {
        private ILinearShield Self { get; }
        protected override ILinearShield Other { get; }

        protected override bool Blocked => LinearShieldUtility.BlockingBetween(Self, Other).Any();

        public ShieldLinearWantLinkUiComponent(ILinearShield self, ILinearShield other, Action<bool> fieldRenderOverride, Action buttonAction)
        {
            Self = self;
            Other = other;
            FieldRenderOverride = fieldRenderOverride;
            ButtonAction = buttonAction;

            var distance = self.Position.DistanceTo(other.Position);
            PowerNeeded = LinearShieldUtility.CellProtectionFactorCost(distance) * distance;
            Efficiency = LinearShieldUtility.CalculateFieldEfficiency(distance, self.EfficientRange, other.EfficientRange);
        }

        protected override string ButtonKey => "FrontierDevelopments.General.Cancel";
        protected override float PowerNeeded { get; }
        protected override float Efficiency { get; }

        protected override Action<bool> FieldRenderOverride { get; }

        protected override Action ButtonAction { get; }
    }
}