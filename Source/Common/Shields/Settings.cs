using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Settings : ModSettings
    {
        private Vector2 _scrollPosition = Vector2.zero;
        private const float ViewMargin = 20f;
        private const int WindowHeight = 720;
        
        // Integrations
        public bool EnableCentralizedClimateControlSupport = true;
        public bool EnableDubsBadHygieneSupport = false;

        // General
        public bool EnableShootingOut = true;
        public bool ScaleOnHeat = true;

        // Performance
        public bool EnableAIAttackTargetFinder = true;
        public bool EnableAICastPositionFinder = true;
        public bool EnableAIVerbFindShotLine = true;
        
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
        
        

        private static void Heading(Listing_Standard list, string text)
        {
            list.GapLine();
            Text.Font = GameFont.Medium;
            list.Label(text);
            Text.Font = GameFont.Small;
            list.Gap();
        }
        
        private static void TextContent(Listing_Standard list, string text)
        {
            list.Label(text);
            list.Gap();
        }

        public void DoWindowContents(Rect inRect)
        {
            var contents = new Rect(ViewMargin, ViewMargin, inRect.size.x - ViewMargin, WindowHeight);
            
            Widgets.BeginScrollView(inRect, ref _scrollPosition, contents, true);
            
            var list = new Listing_Standard();
            list.Begin(contents);

            // Integrations
            Heading(list, "fd.settings.shield.integrations.heading".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.climatecontrol.label".Translate(),
                ref EnableCentralizedClimateControlSupport,
                "fd.settings.shield.climatecontrol.description".Translate());

            list.CheckboxLabeled(
                "fd.settings.shield.badhygiene.label".Translate(),
                ref EnableDubsBadHygieneSupport,
                "fd.settings.shield.badhygiene.description".Translate());

            // General
            Heading(list, "fd.settings.shield.general.heading".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.shootout.label".Translate(), 
                ref EnableShootingOut, 
                "fd.settings.shield.shootout.description".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.scale-heat.label".Translate(),
                ref ScaleOnHeat,
                "fd.settings.shield.scale-heat.description".Translate());
            
            // Performance
            Heading(list, "fd.settings.shield.performance.heading".Translate());
            TextContent(list, "fd.settings.shield.performance.description".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.performance.target.label".Translate(), 
                ref EnableAIAttackTargetFinder, 
                "fd.settings.shield.performance.target.description".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.performance.cast.label".Translate(), 
                ref EnableAICastPositionFinder, 
                "fd.settings.shield.performance.cast.description".Translate());
            list.CheckboxLabeled(
                "fd.settings.shield.performance.verb.label".Translate(), 
                ref EnableAIVerbFindShotLine, 
                "fd.settings.shield.performance.verb.description".Translate());

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
            Widgets.EndScrollView();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref EnableCentralizedClimateControlSupport, "enableCentralizedClimateControlSupport", true);
            Scribe_Values.Look(ref EnableDubsBadHygieneSupport, "enableDubsBadHygieneSupport", false);

            Scribe_Values.Look(ref EnableShootingOut, "enableShootingOut", true);
            Scribe_Values.Look(ref ScaleOnHeat, "scaleOnHeat", true);

            // TODO see above
//            Scribe_Values.Look(ref PowerPerTile, "powerPerTile", 0.1f);
            
            Scribe_Values.Look(ref PowerPerDamage, "powerPerDamage", 1f);
            Scribe_Values.Look(ref DropPodDamage, "dropPodDamage", 100);
            
            Scribe_Values.Look(ref EnableThermal, "EnableThermal", true);
            Scribe_Values.Look(ref HeatPerPower, "HeatPerPower", 1f);
            Scribe_Values.Look(ref EnableMinorThermalIncidents, "EnableMinorThermalIncidents", true);
            Scribe_Values.Look(ref EnableMajorThermalIncidents, "EnableMajorThermalIncidents", true);
            Scribe_Values.Look(ref EnableCriticalThermalIncidents, "EnableCriticalThermalIncidents", true); 
            
            Scribe_Values.Look(ref EnableAIAttackTargetFinder, "EnableAIAttackTargetFinder", true);
            Scribe_Values.Look(ref EnableAICastPositionFinder, "EnableAICastPositionFinder", true);
            Scribe_Values.Look(ref EnableAIVerbFindShotLine, "EnableAIVerbFindShotLine", true); 
        }
    }
}