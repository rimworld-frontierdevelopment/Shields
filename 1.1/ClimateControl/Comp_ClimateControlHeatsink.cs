using FrontierDevelopments.General.Comps;
using Verse;

namespace FrontierDevelopments.ClimateControl
{
    public class CompProperties_ClimateControlHeatsink : CompProperties_HeatSink
    {
        public CompProperties_ClimateControlHeatsink()
        {
            compClass = typeof(Comp_ClimateControlHeatsink);
        }
    }

    public class Comp_ClimateControlHeatsink : Comp_HeatSink
    {
        private ThingComp _airFlowConsumer;
        private float _initialFlow;

        private bool HasAirflow => AirFlowConsumer?.IsOperating() ?? false;

        private Comp_AirFlowConsumer AirFlowConsumer => (Comp_AirFlowConsumer)_airFlowConsumer;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _airFlowConsumer = parent.TryGetComp<Comp_AirFlowConsumer>();
            _initialFlow = AirFlowConsumer?.Props.baseAirExhaust ?? 0f;
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
    }
}