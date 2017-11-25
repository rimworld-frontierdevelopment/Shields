using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Settings : ModSettings
    {
        // Power
        // TODO include later maybe. rimworld's support for float settings is non-existant in A17 though
        public float PowerPerTile = 0.1f;
        public int MinimumOnlinePower = 50;
        
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

        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);

            // Power
            Heading(list, "fd.settings.shield.power.heading".Translate());
            
//            var powerPerTileBuffer = PowerPerTile.ToString();
//            Widgets.TextFieldNumericLabeled(
//                list.GetRect(Text.LineHeight),
//                "fd.settings.shield.power.per_tile.label".Translate(), 
//                ref PowerPerTile, 
//                ref powerPerTileBuffer);
            
            var minimumOnlinePowerBuffer = MinimumOnlinePower.ToString();
            list.TextFieldNumericLabeled<int>(
                "fd.settings.shield.power.minimum_online.label".Translate(),
                ref MinimumOnlinePower,
                ref minimumOnlinePowerBuffer);
            
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
        }

        public override void ExposeData()
        {
            // TODO see above
//            Scribe_Values.Look(ref PowerPerTile, "powerPerTile", 0.1f);
            Scribe_Values.Look(ref MinimumOnlinePower, "minimumOnlinePower", 50);
            
            Scribe_Values.Look(ref PowerPerDamage, "powerPerDamage", 1f);
            Scribe_Values.Look(ref DropPodDamage, "dropPodDamage", 100);
            
            Scribe_Values.Look(ref EnableThermal, "EnableThermal", true);
            Scribe_Values.Look(ref HeatPerPower, "HeatPerPower", 100);
            Scribe_Values.Look(ref EnableMinorThermalIncidents, "EnableMinorThermalIncidents", true);
            Scribe_Values.Look(ref EnableMajorThermalIncidents, "EnableMajorThermalIncidents", true);
            Scribe_Values.Look(ref EnableCriticalThermalIncidents, "EnableCriticalThermalIncidents", true); 
        }
    }
}