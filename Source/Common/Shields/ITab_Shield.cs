using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ITab_Shield : ITab
    {
        private const float ViewMargin = 24f;
        private const float ComponentHeight = 72f;

        public ITab_Shield()
        {
            size = new Vector2(630f, 430f);
            labelKey = "FrontierDevelopments.Shields.ITab.Shield";
        }

        public override bool IsVisible => SelThing.Faction == Faction.OfPlayer && SelShield.UiComponents.Any();

        private IShield SelShield => SelThing as IShield;

        protected override void FillTab()
        {
            var outRect = new Rect(ViewMargin, ViewMargin, size.x - ViewMargin*2, size.y - ViewMargin*2);

            var list = new Listing_Standard();
            list.Begin(outRect);

            foreach (var component in SelShield.UiComponents)
            {
                var section = list.BeginSection(ComponentHeight);
                
                component.Draw(section.GetRect(ComponentHeight));
                
                list.EndSection(section);
                list.Gap();
            }

            list.End();
        }
    }
}