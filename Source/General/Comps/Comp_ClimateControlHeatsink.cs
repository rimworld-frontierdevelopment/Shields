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

        private bool HasAirflow => _connected && ((CentralizedClimateControl.CompAirFlowConsumer)_airFlowConsumer).IsActive();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _airFlowConsumer = parent.TryGetComp<CentralizedClimateControl.CompAirFlowConsumer>();
        }

        protected override float AmbientTemp()
        {
            return HasAirflow
                ? ((CentralizedClimateControl.CompAirFlowConsumer)_airFlowConsumer).AirFlowNet.AverageConvertedTemperature
                : base.AmbientTemp();
        }

        protected override void DissipateHeat(float kilojoules)
        {
            if(!HasAirflow) base.DissipateHeat(kilojoules);
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