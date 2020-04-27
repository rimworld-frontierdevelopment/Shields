using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Settings : ModSettings
    {
        // Integrations
        public bool EnableCentralizedClimateControlSupport = true;

        public bool EnableDubsBadHygieneSupport = true;

        // General
        public bool EnableShootingOut = true;

        public bool EnableShootingIn = false;
        public bool OverlapPassThrough = false;
        public bool ScaleOnHeat = true;

        // Power
        // TODO include later maybe. rimworld's support for float settings is non-existant in A17 though
        public float PowerPerTile = 0.1f;

        // Integrity
        public float PowerPerDamage = 1.0f;

        public int DropPodDamage = 100;
        public int SkyfallerDamage = 1000;

        // Thermal
        public bool EnableThermal = true;

        public float HeatPerPower = 1.0f;
        public bool EnableMinorThermalIncidents = true;
        public bool EnableMajorThermalIncidents = true;
        public bool EnableCriticalThermalIncidents = true;

        // Colour
        public bool SecondaryColour = true;

        public Color ShieldColour = new Color(1, 0, 0, .8f);
        public Color ShieldSecondaryColour = new Color(0, 0, 1, .8f);
        public Color[] colours = null;

        private static void Heading(Listing_Standard list, string text)
        {
            list.GapLine();
            Text.Font = GameFont.Medium;
            list.Label(text);
            Text.Font = GameFont.Small;
            list.Gap();
        }

        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);

            // Integrations
            Heading(list, "fd.settings.shield.integrations.heading".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.climatecontrol.label".Translate(),
                ref EnableCentralizedClimateControlSupport,
                "fd.settings.shield.climatecontrol.description".Translate());

            list.CheckboxLabeled(
                "Dubs Bad Hygiene Integration",
                ref EnableDubsBadHygieneSupport,
                "placeholder");

            // General
            Heading(list, "fd.settings.shield.general.heading".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.shootout.label".Translate(),
                ref EnableShootingOut,
                "fd.settings.shield.shootout.description".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.shootin.label".Translate(),
                ref EnableShootingIn,
                "fd.settings.shield.shootin.description".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.overlap-passthrough.label".Translate(),
                ref OverlapPassThrough,
                "fd.settings.shield.overlap-passthrough.description".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.scale-heat.label".Translate(),
                ref ScaleOnHeat,
                "fd.settings.shield.scale-heat.description".Translate());

            // Power
            Heading(list, "fd.settings.shield.power.heading".Translate());

            //            var powerPerTileBuffer = PowerPerTile.ToString();
            //            Widgets.TextFieldNumericLabeled(
            //                list.GetRect(Text.LineHeight),
            //                "fd.settings.shield.power.per_tile.label".Translate(),
            //                ref PowerPerTile,
            //                ref powerPerTileBuffer);

            // Integrity
            Heading(list, "fd.settings.shield.integrity.heading".Translate());

            var powerPerDamageBuffer = PowerPerDamage.ToString();
            Widgets.TextFieldNumericLabeled(
                list.GetRect(Text.LineHeight),
                "fd.settings.shield.integrity.power_per_damage.label".Translate(),
                ref PowerPerDamage,
                ref powerPerDamageBuffer);

            var dropPodDamageBuffer = DropPodDamage.ToString();
            Widgets.TextFieldNumericLabeled(
                list.GetRect(Text.LineHeight),
                "fd.settings.shield.integrity.drop_pod_damage.label".Translate(),
                ref DropPodDamage,
                ref dropPodDamageBuffer);

            var skyfallerDamageBuffer = SkyfallerDamage.ToString();
            Widgets.TextFieldNumericLabeled(
                list.GetRect(Text.LineHeight),
                "fd.settings.shield.integrity.skyfaller_damage.label".Translate(),
                ref SkyfallerDamage,
                ref skyfallerDamageBuffer);

            Heading(list, "Colour");
            Rect first = list.GetRect(Text.LineHeight);
            ShieldColour.a = Widgets.HorizontalSlider(new Rect(first.position.x, first.position.y, first.width / 5, first.height), ShieldColour.a, 0f, .8f);
            ShieldColour.r = Widgets.HorizontalSlider(new Rect(first.position.x + first.width / 5, first.position.y, first.width / 5, first.height), ShieldColour.r, 0f, 1f);
            ShieldColour.g = Widgets.HorizontalSlider(new Rect(first.position.x + 2 * (first.width / 5), first.position.y, first.width / 5, first.height), ShieldColour.g, 0f, 1f);
            ShieldColour.b = Widgets.HorizontalSlider(new Rect(first.position.x + 3 * (first.width / 5), first.position.y, first.width / 5, first.height), ShieldColour.b, 0f, 1f);
            Widgets.DrawBoxSolid(new Rect(first.position.x + 4 * (first.width / 5), first.position.y, first.width / 5, first.height), new Color(ShieldColour.r, ShieldColour.g, ShieldColour.b));

            list.CheckboxLabeled("Enable secondary shield colouring", ref SecondaryColour, "fd.settings.shield.thermal.description".Translate());

            if (SecondaryColour)
            {
                Rect second = list.GetRect(Text.LineHeight);
                ShieldSecondaryColour.a = Widgets.HorizontalSlider(new Rect(second.position.x, second.position.y, second.width / 5, second.height), ShieldSecondaryColour.a, 0f, .8f);
                ShieldSecondaryColour.r = Widgets.HorizontalSlider(new Rect(second.position.x + second.width / 5, second.position.y, second.width / 5, second.height), ShieldSecondaryColour.r, 0f, 1f);
                ShieldSecondaryColour.g = Widgets.HorizontalSlider(new Rect(second.position.x + 2 * (second.width / 5), second.position.y, second.width / 5, second.height), ShieldSecondaryColour.g, 0f, 1f);
                ShieldSecondaryColour.b = Widgets.HorizontalSlider(new Rect(second.position.x + 3 * (second.width / 5), second.position.y, second.width / 5, second.height), ShieldSecondaryColour.b, 0f, 1f);
                Widgets.DrawBoxSolid(new Rect(second.position.x + 4 * (second.width / 5), second.position.y, second.width / 5, second.height), new Color(ShieldSecondaryColour.r, ShieldSecondaryColour.g, ShieldSecondaryColour.b));
            }
            colours = null;

            // Thermal
            Heading(list, "fd.settings.shield.thermal.heading".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.thermal.label".Translate(),
                ref EnableThermal,
                "fd.settings.shield.thermal.description".Translate());
            if (EnableThermal)
            {
                var heatPerPowerBuffer = HeatPerPower.ToString();
                Widgets.TextFieldNumericLabeled(
                    list.GetRect(Text.LineHeight),
                    "fd.settings.shield.thermal.per_power.label".Translate(),
                    ref HeatPerPower,
                    ref heatPerPowerBuffer);
                list.CheckboxLabeled(
                    "fd.settings.shield.minor_thermal_incidents.label".Translate(),
                    ref EnableMinorThermalIncidents,
                    "fd.settings.shield.minor_thermal_incidents.description".Translate());
                list.CheckboxLabeled(
                    "fd.settings.shield.major_thermal_incidents.label".Translate(),
                    ref EnableMajorThermalIncidents,
                    "fd.settings.shield.major_thermal_incidents.description".Translate());
                list.CheckboxLabeled(
                    "fd.settings.shield.critical_thermal_incidents.label".Translate(),
                    ref EnableCriticalThermalIncidents,
                    "fd.settings.shield.critical_thermal_incidents.description".Translate());
            }
            list.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref EnableCentralizedClimateControlSupport, "enableCentralizedClimateControlSupport", true);

            Scribe_Values.Look(ref EnableShootingOut, "enableShootingOut", true);
            Scribe_Values.Look(ref EnableShootingIn, "enableShootingIn", false);
            Scribe_Values.Look(ref OverlapPassThrough, "overlapPassThrough", false);
            Scribe_Values.Look(ref ScaleOnHeat, "scaleOnHeat", true);

            // TODO see above
            //            Scribe_Values.Look(ref PowerPerTile, "powerPerTile", 0.1f);
            Scribe_Values.Look<bool>(ref SecondaryColour, "secondaryColour", false);
            Scribe_Values.Look<Color>(ref ShieldColour, "shieldColour", new Color(0.1f, 0.1f, 0.1f, 0.8f));
            Scribe_Values.Look<Color>(ref ShieldSecondaryColour, "shieldSecondaryColour", new Color(0.9f, 0.9f, 0.9f, 0.8f));

            Scribe_Values.Look(ref PowerPerDamage, "powerPerDamage", 1f);
            Scribe_Values.Look(ref DropPodDamage, "dropPodDamage", 100);

            Scribe_Values.Look(ref EnableThermal, "EnableThermal", true);
            Scribe_Values.Look(ref HeatPerPower, "HeatPerPower", 1f);
            Scribe_Values.Look(ref EnableMinorThermalIncidents, "EnableMinorThermalIncidents", true);
            Scribe_Values.Look(ref EnableMajorThermalIncidents, "EnableMajorThermalIncidents", true);
            Scribe_Values.Look(ref EnableCriticalThermalIncidents, "EnableCriticalThermalIncidents", true);
        }
    }
}