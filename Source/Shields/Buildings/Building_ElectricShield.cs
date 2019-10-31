using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Buildings
{
    public class Building_ElectricShield : Building, IHeatsink, IEnergyNet, IShield
    {
        public enum ShieldStatus
        {
            Unpowered,
            ThermalShutdown,
            Online
        }

        private EnergyNet _energyNet = new EnergyNet();
        private IShield _shield;

        private IHeatsink _heatSink;
        private CompFlickable _flickable;

        private bool _activeLastTick;

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

        private bool WantActive => _flickable?.SwitchIsOn ?? true;

        private bool IsActive => _shield?.IsActive() ?? false;
        private float BasePowerConsumption => _shield?.ProtectedCellCount * Mod.Settings.PowerPerTile ?? 0f;

        public ShieldStatus Status
        {
            get
            {
                if (Heatsink != null && Heatsink.OverTemperature) return ShieldStatus.ThermalShutdown;
                if (_energyNet.RateAvailable <= 0) return ShieldStatus.Unpowered;
                return ShieldStatus.Online;
            }
        }

        public void Init()
        {
            AllComps.OfType<IEnergyNode>().Do(Connect);
            _flickable = GetComp<CompFlickable>();
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
            base.Tick();
            _energyNet.Update();
            var active = IsActive;
            if (active)
            {
                _energyNet.Consume(BasePowerConsumption / GenDate.TicksPerDay);
            }
            else if(_activeLastTick && WantActive)
            {
                Messages.Message("fd.shields.incident.offline.body".Translate(), new GlobalTargetInfo(Position, Map), MessageTypeDefOf.NegativeEvent);
            }
            _activeLastTick = active;
            
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref _energyNet, "energyNet");
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
        // Energy Net
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Connect(IEnergyNode node)
        {
            _energyNet.Connect(node);
        }

        public void Disconnect(IEnergyNode node)
        {
            _energyNet.Disconnect(node);
        }

        public void ConnectTo(IEnergyNet net)
        {
            _energyNet.ConnectTo(net);
        }

        public void Disconnect()
        {
            _energyNet.Disconnect();
        }

        public float Provide(float amount)
        {
            return _energyNet.Provide(amount);
        }

        public float Consume(float amount)
        {
            return _energyNet.Consume(amount);
        }

        public void Update()
        {
            _energyNet.Update();
        }

        public void HasPower(bool isPowered)
        {
            _energyNet.HasPower(isPowered);
        }

        public void Changed()
        {
            _energyNet.Changed();
        }

        public float AmountAvailable => _energyNet.AmountAvailable;
        public float RateAvailable => _energyNet.RateAvailable;
        public float TotalAvailable => _energyNet.TotalAvailable;
        public float MaxRate => _energyNet.MaxRate;
        public IEnergyNet Parent => _energyNet.Parent;
        public float Rate => _energyNet.Rate;
        public IEnumerable<IEnergyNode> Nodes => _energyNet.Nodes;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Shield
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public int ProtectedCellCount => Shield.ProtectedCellCount;

        public float DeploymentSize => _shield.DeploymentSize;
        
        public IEnumerable<Gizmo> ShieldGizmos => _shield.ShieldGizmos;

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

        public float Block(long damage, Vector3 position)
        {
            return Shield.Block(damage, position);
        }

        public float Block(ShieldDamages damages, Vector3 position)
        {
            return Shield.Block(damages, position);
        }

        public void Draw(CellRect cameraRect)
        {
            Shield.Draw(cameraRect);
        }
    }
}
