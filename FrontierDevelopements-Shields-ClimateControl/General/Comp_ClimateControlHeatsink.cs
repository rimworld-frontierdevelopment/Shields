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

        private bool HasAirflow => AirFlowConsumer.IsOperating();

        private Comp_AirFlowConsumer AirFlowConsumer => (Comp_AirFlowConsumer) _airFlowConsumer; 
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _airFlowConsumer = parent.TryGetComp<Comp_AirFlowConsumer>();
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
    }
}
