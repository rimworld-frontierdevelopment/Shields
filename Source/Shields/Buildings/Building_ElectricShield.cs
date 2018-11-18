using System.Text;
using FrontierDevelopments.General;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FrontierDevelopments.Shields.Buildings
{
    public class Building_ElectricShield : Building
    {
        public enum ShieldStatus
        {
            Unpowered,
            ThermalShutdown,
            Online
        }

        private IEnergySource _energySource;
        private IShield _shield;

        private IHeatsink _heatSink;

        private bool _activeLastTick;

        private bool IsActive => _energySource.IsActive;
        private float BasePowerConsumption => -_shield.ProtectedCellCount * Mod.Settings.PowerPerTile;

        public ShieldStatus Status
        {
            get
            {
                if (_heatSink != null && _heatSink.OverTemperature) return ShieldStatus.ThermalShutdown;
                if (!_energySource.IsActive) return ShieldStatus.Unpowered;
                return ShieldStatus.Online;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            LessonAutoActivator.TeachOpportunity(ConceptDef.Named("FD_Shields"), OpportunityType.Critical);
            _energySource = EnergySourceUtility.Find(this);
            _shield = ShieldUtility.FindComp(this);
            _heatSink = HeatsinkUtility.FindComp(this);
            _activeLastTick = IsActive;
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            var active = IsActive;
            _energySource.BaseConsumption = BasePowerConsumption;
            base.Tick();
            if(_activeLastTick && !active && _energySource.WantActive)
                Messages.Message("fd.shields.incident.offline.body".Translate(), new GlobalTargetInfo(Position, Map), MessageTypeDefOf.NegativeEvent);
            _activeLastTick = active;
        }

        public override string GetInspectString()
        {
            LessonAutoActivator.TeachOpportunity(ConceptDef.Named("FD_CoilTemperature"), OpportunityType.Important);
            var stringBuilder = new StringBuilder();
            switch (Status)
            {
                case ShieldStatus.Unpowered:
                    stringBuilder.AppendLine("shield.status.offline".Translate() + " - " + "shield.status.no_power".Translate());
                    break;
                case ShieldStatus.ThermalShutdown:
                    stringBuilder.AppendLine("shield.status.offline".Translate() + " - " + "shield.status.thermal_safety".Translate());
                    break;
                case ShieldStatus.Online:
                    stringBuilder.AppendLine("shield.status.online".Translate());
                    break;
            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString();
        }
    }
}
