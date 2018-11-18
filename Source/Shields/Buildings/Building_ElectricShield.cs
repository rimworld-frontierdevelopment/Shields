using System.Text;
using FrontierDevelopments.General;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Buildings
{
    public class Building_ElectricShield : Building, IShield
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

        public int ProtectedCellCount => _shield.ProtectedCellCount;

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
            _activeLastTick = IsActive();
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            var active = IsActive();
            _energySource.BaseConsumption = BasePowerConsumption;
            base.Tick();
            if(_activeLastTick && !active && _energySource.WantActive)
                Messages.Message("fd.shields.incident.offline.body".Translate(), new GlobalTargetInfo(Position, Map), MessageTypeDefOf.NegativeEvent);
            _activeLastTick = active;
        }

        public bool IsActive()
        {
            return _energySource.IsActive && (_heatSink != null && !_heatSink.OverTemperature || _heatSink == null);
        }

        private void RenderImpactEffect(Vector2 position)
        {
            MoteMaker.ThrowLightningGlow(Common.ToVector3(position), Map, 0.5f);
        }

        private void PlayBulletImpactSound(Vector2 position)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Common.ToIntVec3(position), Map));
        }

        public bool Block(long damage, Vector3 position)
        {
            if (!IsActive()) return false;
            // convert watts per day to watts per tick
            var charge = (damage * 60000 * Mod.Settings.PowerPerDamage);
            if (!_energySource.Draw(charge)) return false;
            RenderImpactEffect(Common.ToVector2(position));
            PlayBulletImpactSound(Common.ToVector2(position));
            return true;    
        }

        public bool Collision(Vector3 vector)
        {
            return _shield.Collision(vector);
        }

        public Vector3? Collision(Vector3 start, Vector3 end)
        {
            return _shield.Collision(start, end);
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return _shield.Collision(ray, limit);
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

        public void Draw(CellRect cameraRect)
        {
            if(IsActive()) _shield.Draw(cameraRect);
        }
    }
}
