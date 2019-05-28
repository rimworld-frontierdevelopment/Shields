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
        
        private IEnergySource EnergySource {
            get
            {
                if (_energySource == null) 
                    _energySource = EnergySourceUtility.FindComp(AllComps);
                return _energySource;
            }
        }

        public IShield Shield {
            get
            {
                if (_shield == null) 
                    _shield = ShieldUtility.FindComp(AllComps);
                return _shield;
            }
        }

        private IHeatsink Heatsink
        {
            get
            {
                if (_heatSink == null) 
                    _heatSink = HeatsinkUtility.FindComp(AllComps);
                return _heatSink;
            }
        }

        private bool IsActive => _shield?.IsActive() ?? false;
        private float BasePowerConsumption => -_shield?.ProtectedCellCount * Mod.Settings.PowerPerTile ?? 0f;

        public ShieldStatus Status
        {
            get
            {
                if (Heatsink != null && Heatsink.OverTemperature) return ShieldStatus.ThermalShutdown;
                if (EnergySource != null && !EnergySource.IsActive()) return ShieldStatus.Unpowered;
                return ShieldStatus.Online;
            }
        }

        public void Init()
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
                EnergySource.BaseConsumption = BasePowerConsumption;
            }
            else if(_activeLastTick && (EnergySource?.WantActive ?? false))
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
            Heatsink.PushHeat(wattDays);
        }

        public bool OverTemperature => Heatsink.OverTemperature;

        public float Temp => Heatsink.Temp;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Energy Source
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        bool IEnergySource.IsActive()
        {
            return EnergySource.IsActive();
        }

        public float Draw(float amount)
        {
            return EnergySource.Draw(amount);
        }

        public void Drain(float amount)
        {
            EnergySource.Drain(amount);
        }

        public bool WantActive => EnergySource.WantActive;

        public float BaseConsumption
        {
            get => EnergySource.BaseConsumption;
            set => EnergySource.BaseConsumption = value;
        }
        
        public float EnergyAvailable => EnergySource.EnergyAvailable;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Shield
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public int ProtectedCellCount => Shield.ProtectedCellCount;
        
        bool IShield.IsActive()
        {
            return Shield.IsActive();
        }

        public bool Collision(Vector3 point)
        {
            return Shield.Collision(point);
        }

        public Vector3? Collision(Ray ray, float limit)
        {
            return Shield.Collision(ray, limit);
        }

        public Vector3? Collision(Vector3 start, Vector3 end)
        {
            return Shield.Collision(start, end);
        }

        public bool Block(long damage, Vector3 position)
        {
            return Shield.Block(damage, position);
        }

        public bool Block(ShieldDamages damages, Vector3 position)
        {
            return Shield.Block(damages, position);
        }

        public void Draw(CellRect cameraRect)
        {
            Shield.Draw(cameraRect);
        }
    }
}
