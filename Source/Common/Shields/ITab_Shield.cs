using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ITab_Shield : ITab
    {
        private static readonly Color GreyOutColor = new Color(0.5f, 0.5f, 0.5f);

        private const float ViewMargin = 24f;
        
        private Vector2 _scrollPosition = Vector2.zero;

        public ITab_Shield()
        {
            size = new Vector2(640f, 480f);
            labelKey = "FrontierDevelopments.Shields.ITab.Shield";
        }

        public override bool IsVisible => SelThing.Faction == Faction.OfPlayer && SelShield.UiComponents.Any();

        private IShieldUserInterface SelShield => SelThing as IShieldUserInterface;

        private void ResetButton(Listing_Standard list)
        {
            var resetActive = SelShield.HasWantSettings;

            var prevColor = GUI.color;
            if (!resetActive)
            {
                GUI.color = GreyOutColor;
            }

            // TODO doesnt cancel other linear shield
            if (list.ButtonText("FrontierDevelopments.General.CancelFlick".Translate()) && resetActive)
                SelShield.ClearWantSettings();

            GUI.color = prevColor;
        }

        protected override void FillTab()
        {
            var components = SelShield.UiComponents.ToList();

            const float buttonHeight = 30f;
            const float sectionBorder = 4f;
            const float gapHeight = 12f;

            var viewHeight = components.Aggregate(buttonHeight, (total, component) => total + component.Height + sectionBorder * 2 + gapHeight);
            
            var outRect = new Rect(ViewMargin, ViewMargin, size.x - ViewMargin, size.y - ViewMargin);
            var viewRect = new Rect(0, 0, size.x - ViewMargin*2, viewHeight);
            
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect, true);

            var list = new Listing_Standard();
            list.Begin(viewRect);

            ResetButton(list);

            foreach (var component in components)
            {
                var section = list.BeginSection(component.Height, sectionBorder, sectionBorder);
                component.Draw(section.GetRect(component.Height));
                list.EndSection(section);
                list.Gap(gapHeight);
            }

            list.End();
            Widgets.EndScrollView();
        }
    }
}