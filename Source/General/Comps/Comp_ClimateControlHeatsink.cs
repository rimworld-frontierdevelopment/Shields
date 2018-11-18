using System.Collections.Generic;
using FrontierDevelopments.Shields;
using RimWorld;
using Verse;

namespace FrontierDevelopments.General.Comps
{
    public class Comp_ClimateControlHeatsink : Comp_HeatSink
    {
        private ThingComp _airFlowConsumer;
        private bool _connected = true;
        private float _initialFlow;

        private bool HasAirflow => _connected && AirFlowConsumer.IsActive();

        private CentralizedClimateControl.CompAirFlowConsumer AirFlowConsumer =>
            (CentralizedClimateControl.CompAirFlowConsumer) _airFlowConsumer; 

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _airFlowConsumer = parent.TryGetComp<CentralizedClimateControl.CompAirFlowConsumer>();
            _initialFlow = AirFlowConsumer.Props.baseAirExhaust;
        }

        protected override float AmbientTemp()
        {
            return HasAirflow
                ? AirFlowConsumer.AirFlowNet.AverageConvertedTemperature
                : base.AmbientTemp();
        }

        protected override void DissipateHeat(float kilojoules)
        {
            if (HasAirflow)
            {
                var delta = Temp - AirFlowConsumer.AirFlowNet.AverageConvertedTemperature;
                var flowRate = delta * 1000;
                if (flowRate < _initialFlow) flowRate = _initialFlow;
                AirFlowConsumer.Props.baseAirExhaust = flowRate;
            }
            else
            {
                base.DissipateHeat(kilojoules);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _connected, "connected", true);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    icon = Resources.BuildingClimateControlAirThermal,
                    defaultDesc = "fd.heatsink.net.connect.description".Translate(),
                    defaultLabel = "fd.heatsink.net.connect.label".Translate(),
                    isActive = () => _connected,
                    toggleAction = () => _connected = !_connected
                };
            }
        }
    }
}