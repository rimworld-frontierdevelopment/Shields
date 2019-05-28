using System.Collections.Generic;
using CentralizedClimateControl;
using FrontierDevelopments.General;
using Verse;

namespace FrontierDevelopments.ClimateControl
{
    public class Comp_AirFlowConsumer : CompAirFlowConsumer
    {
        private bool _connected = true;
    
        public override bool IsOperating()
        {
            return _connected && AirFlowNet != null && AirFlowNet.CurrentIntakeAir > 0f && !float.IsNaN(AirFlowNet.AverageConvertedTemperature);
        }

        public override string CompInspectStringExtra()
        {
            return IsOperating()
                ? ConnectedKey.Translate()
                : NotConnectedKey.Translate();
        }
    
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (OwnershipUtility.PlayerOwns(parent))
            {
                yield return new Command_Toggle
                {
                    icon = Resources.BuildingClimateControlAirThermal,
                    defaultDesc = "fd.heatsink.climate.net.connect.description".Translate(),
                    defaultLabel = "fd.heatsink.climate.net.connect.label".Translate(),
                    isActive = () => _connected,
                    toggleAction = () => _connected = !_connected
                };
            }
        }
    
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _connected, "connected", true);
        }
    }
}