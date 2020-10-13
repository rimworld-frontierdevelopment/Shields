using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using FrontierDevelopments.General.UI;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FrontierDevelopments.Shields.Buildings
{
    public class Building_ElectricShield : Building, IHeatsink, IEnergyNet, IShieldParent
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
                    return Fields.Aggregate(0f,
                        (total, field) => 
                            total + field.ProtectedCellCount * field.CellProtectionFactor);
                }
                else
                {
                    return 0;
                }
            }
        }

        public IEnumerable<IShieldStatus> Status
        {
            get
            {
                if (Heatsink != null && Heatsink.OverTemperature)
                    yield return new ShieldStatusThermalShutdown();

                if (_energyNet.RateAvailable <= 0)
                    yield return new ShieldStatusBatteryLow();
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

        public bool HasWantSettings => WantThermalShutoff != ThermalShutoff || _shield.HasWantSettings;
        
        private IEnumerable<IShieldField> Fields => _shield.Fields;
        
        public IEnumerable<Gizmo> ShieldGizmos => new List<Gizmo>();

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

        public string TextStats
        {
            get
            {
                var powerRequired = -(int)GetComp<CompPowerTrader>().PowerOutput;
                var canBlock = (int)(_energyNet.AmountAvailable * Mod.Settings.PowerPerDamage);
                var text = new StringBuilder();
                text.AppendLine(ShieldStatus.GetStringFromStatuses(Status.ToList()));
                text.AppendLine(("PowerNeeded".Translate() + ": " + (powerRequired).ToString("#####0") + " W"));
                text.AppendLine("fd.heatsink.temperature".Translate(Temp.ToStringTemperature()));
                text.AppendLine("FrontierDevelopments.Shields.ITab.CanBlock".Translate(canBlock));
                return text.ToString().TrimEndNewlines();
            }
        }

        public IEnumerable<UiComponent> UiComponents
        {
            get
            {
                yield return new TextComponent(TextStats);

                foreach (var component in _shield.UiComponents)
                {
                    yield return component;
                }

                yield return new CheckboxComponent(
                    "thermal_shutoff.label".Translate() + "\n" + "thermal_shutoff.description".Translate(),
                    () => Heatsink.WantThermalShutoff,
                    value => WantThermalShutoff = value);
            }
        }

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

        public void ClearWantSettings()
        {
            _shield.ClearWantSettings();
            WantThermalShutoff = Heatsink.ThermalShutoff;
        }
    }
}
