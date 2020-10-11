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

        public ITab_Shield()
        {
            size = new Vector2(630f, 430f);
            labelKey = "FrontierDevelopments.Shields.ITab.Shield";
        }

        public override bool IsVisible => SelThing.Faction == Faction.OfPlayer && SelShield.UiComponents.Any();

        private IShield SelShield => SelThing as IShield;

        private void ResetButton(Listing_Standard list)
        {
            var resetActive = SelShield.HasWantSettings;

            var prevColor = GUI.color;
            if (!resetActive)
            {
                GUI.color = GreyOutColor;
            }

            if (list.ButtonText("FrontierDevelopments.General.CancelFlick".Translate()) && resetActive)
                SelShield.ClearWantSettings();

            GUI.color = prevColor;
        }

        protected override void FillTab()
        {
            var outRect = new Rect(ViewMargin, ViewMargin, size.x - ViewMargin*2, size.y - ViewMargin*2);

            var list = new Listing_Standard();
            list.Begin(outRect);

            ResetButton(list);

            foreach (var component in SelShield.UiComponents)
            {
                var section = list.BeginSection(component.Height);
                component.Draw(section.GetRect(component.Height));
                list.EndSection(section);
                list.Gap();
            }

            list.End();
        }
    }
}