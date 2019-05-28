using System.Text;
using FrontierDevelopments.General;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Buildings
{
    public class Building_ElectricShield : Building, IHeatsink, IEnergySource, IShield
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

        private bool IsActive => _shield?.IsActive() ?? false;
        private float BasePowerConsumption => -_shield?.ProtectedCellCount * Mod.Settings.PowerPerTile ?? 0f;

        public ShieldStatus Status
        {
            get
            {
                if (_heatSink != null && _heatSink.OverTemperature) return ShieldStatus.ThermalShutdown;
                if (_energySource != null && _energySource.IsActive()) return ShieldStatus.Unpowered;
                return ShieldStatus.Online;
            }
        }

        private void Init()
        {
            _energySource = EnergySourceUtility.FindComp(AllComps);
            _shield = ShieldUtility.FindComp(AllComps);
            _heatSink = HeatsinkUtility.FindComp(AllComps);
        }

        public override void PostMake()
        {
            base.PostMake();
            Init();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            _activeLastTick = false;
            Init();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            _activeLastTick = false;
        }

        public override void Tick()
        {
            var active = IsActive;
            if (active)
            {
                _energySource.BaseConsumption = BasePowerConsumption;
            }
            else if(_activeLastTick && (_energySource?.WantActive ?? false))
            {
                Messages.Message("fd.shields.incident.offline.body".Translate(), new GlobalTargetInfo(Position, Map), MessageTypeDefOf.NegativeEvent);
            }
            _activeLastTick = active;
            base.Tick();
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            switch (Status)
            {
                case ShieldStatus.Unpowered:
                    stringBuilder.AppendLine("shield.status.offline".Translate() + " - " + "shield.status.battery_too_low".Translate());
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Heatsink
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public void PushHeat(float wattDays)
        {
            _heatSink.PushHeat(wattDays);
        }

        public bool OverTemperature => _heatSink.OverTemperature;

        public float Temp => _heatSink.Temp;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Energy Source
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        bool IEnergySource.IsActive()
        {
            return _energySource.IsActive();
        }

        public float Draw(float amount)
        {
            return _energySource.Draw(amount);
        }

        public void Drain(float amount)
        {
            _energySource.Drain(amount);
        }

        public bool WantActive => _energySource.WantActive;

        public float BaseConsumption
        {
            get => _energySource.BaseConsumption;
            set => _energySource.BaseConsumption = value;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Shield
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public float EnergyAvailable => _energySource.EnergyAvailable;

        public int ProtectedCellCount => _shield.ProtectedCellCount;
        
        bool IShield.IsActive()
        {
            return _shield.IsActive();
        }

        public bool Collision(Vector3 point)
        {
            return _shield.Collision(point);
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return _shield.Collision(ray, limit);
        }

        public Vector3? Collision(Vector3 start, Vector3 end)
        {
            return _shield.Collision(start, end);
        }

        public bool Block(long damage, Vector3 position)
        {
            return _shield.Block(damage, position);
        }

        public bool Block(ShieldDamages damages, Vector3 position)
        {
            return _shield.Block(damages, position);
        }

        public void Draw(CellRect cameraRect)
        {
            _shield.Draw(cameraRect);
        }
    }
}
