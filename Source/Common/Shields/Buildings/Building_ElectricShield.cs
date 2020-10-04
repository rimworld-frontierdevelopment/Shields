using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using FrontierDevelopments.General.UI;
using FrontierDevelopments.Shields.Comps;
using FrontierDevelopments.Shields.Deployment;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Buildings
{
    public class Building_ElectricShield : Building, IHeatsink, IEnergyNet, IShieldManageable, IShieldParent
    {
        public class ShieldStatusThermalShutdown : IShieldStatus
        {
            public bool Online => false;
            public string Description => "shield.status.thermal_safety".Translate();
        }
    
        public class ShieldStatusBatteryLow : IShieldStatus
        {
            public bool Online => false;
            public string Description => "shield.status.battery_too_low".Translate();
        }
        
        private EnergyNet _energyNet = new EnergyNet();
        private IShield _shield;

        private IHeatsink _heatSink;
        private CompFlickable _flickable;

        private bool _activeLastTick;
        private float _lifetimeDamageBlocked;

        IShieldParent IShield.Parent => null; // TODO FIXME needed for proper deployment

        public Thing Thing => this;

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

        public bool ParentActive => WantActive && RateAvailable > 0 && !Heatsink.OverTemperature;

        private float BasePowerConsumption
        {
            get
            {
                if (_shield != null)
                {
                    return _shield.ProtectedCellCount * _shield.CellProtectionFactor;
                }

                return 0;
            }
        }

        public IShieldResists Resists => this.TryGetComp<Comp_ShieldResistance>();

        public IEnumerable<IShieldStatus> Status
        {
            get
            {
                if (Heatsink != null && Heatsink.OverTemperature)
                    yield return new ShieldStatusThermalShutdown();

                if (_energyNet.RateAvailable <= 0)
                    yield return new ShieldStatusBatteryLow();

                foreach (var status in _shield.Status)
                    yield return status;
            }
        }

        public void Init()
        {
            AllComps.OfType<IEnergyNode>().Do(Connect);
            _flickable = GetComp<CompFlickable>();
            _shield = ShieldUtility.FindComp(AllComps);
            _shield.SetParent(this);
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
            Map.GetComponent<ShieldManager>().Add(this);
        }

        private void RemoveShield()
        {
            _activeLastTick = false;
            Map.GetComponent<ShieldManager>().Del(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            RemoveShield();
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            RemoveShield();
            base.Destroy(mode);
        }

        public override void Tick()
        {
            base.Tick();
            _energyNet.Update();
            var active = ParentActive;
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
            stringBuilder.AppendLine(ShieldStatus.GetStringFromStatuses(Status.ToList()));
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (var gizmo in ShieldGizmos)
            {
                yield return gizmo;
            }
        }
        
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (var stat in base.SpecialDisplayStats())
            {
                yield return stat;
            }

            yield return new StatDrawEntry(
                StatCategoryDefOf.Building,
                "FrontierDevelopments.Shields.Stats.Blocked.Label".Translate(),
                "" + _lifetimeDamageBlocked,
                "FrontierDevelopments.Shields.Stats.Blocked.Desc".Translate(),
                100);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref _energyNet, "energyNet");
            Scribe_Values.Look(ref _lifetimeDamageBlocked, "lifetimeDamageBlocked");
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

        public bool WantThermalShutoff
        {
            get =>  Heatsink.WantThermalShutoff;
            set => Heatsink.WantThermalShutoff = value;
        }

        public bool ThermalShutoff => Heatsink.ThermalShutoff;

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

        public float Request(float amount)
        {
            return _energyNet.Request(amount);
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

        public float CellProtectionFactor => Shield.CellProtectionFactor;

        public float DeploymentSize => def.GetModExtension<ShieldDeploymentSizeExtension>()?.deploymentSize ?? -1;
        
        public IEnumerable<Gizmo> ShieldGizmos
        {
            get
            {
                foreach (var gizmo in _shield.ShieldGizmos)
                {
                    yield return gizmo;
                }
                if (Faction == Faction.OfPlayer)
                {
                    foreach (var gizmo in ShieldSettingsClipboard.Gizmos(this))
                    {
                        yield return gizmo;
                    }
                }
            }
        }

        public void SetParent(IShieldParent shieldParent)
        {
        }

        bool IShield.IsActive()
        {
            return ParentActive;
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

        private void HandleBlockingHeat(float handled)
        {
            Heatsink?.PushHeat(handled * Mod.Settings.HeatPerPower);
        }

        public float SinkDamage(float damage)
        {
            var temp = Heatsink.Temp;
            var drawn = _energyNet.Consume(damage * Mod.Settings.PowerPerDamage);
            HandleBlockingHeat(drawn);
            
            // fix cases where floating point rounding causes the unapplied to be 0.00000001 shy of the damage
            var blocked = drawn / Mod.Settings.PowerPerDamage;
            if (damage - blocked > 1f)
            {
                _lifetimeDamageBlocked += blocked;
                return blocked;
            }

            _lifetimeDamageBlocked += damage;
            return damage;
        }

        public float Block(float damage, Vector3 position)
        {
            return _shield.Block(damage, position);
        }

        public float Block(ShieldDamages damages, Vector3 position)
        {
            return _shield.Block(damages, position);
        }

        public void FieldPreDraw()
        {
            Shield.FieldPreDraw();
        }

        public void FieldDraw(CellRect cameraRect)
        {
            Shield.FieldDraw(cameraRect);
        }

        public void FieldPostDraw()
        {
            Shield.FieldPostDraw();
        }

        public IEnumerable<UiComponent> UiComponents => _shield.UiComponents;

        public IEnumerable<ShieldSetting> ShieldSettings
        {
            get
            {
                foreach (var setting in _shield.ShieldSettings)
                {
                    yield return setting;
                }
                yield return new ThermalShutoffSetting(Heatsink.WantThermalShutoff);
            }
            set
            {
                _shield.ShieldSettings = value;
                value.Do(Apply);
            }
        }

        private void Apply(ShieldSetting setting)
        {
            switch (setting)
            {
                case ThermalShutoffSetting thermalShutoff:
                    WantThermalShutoff = thermalShutoff.Get();
                    break;
            }
        }
    }
}
