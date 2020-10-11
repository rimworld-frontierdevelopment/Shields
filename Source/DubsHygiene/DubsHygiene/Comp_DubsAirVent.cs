using System.Collections.Generic;
using DubsBadHygiene;
using RimWorld;
using System.Text;
using FrontierDevelopments.General;
using Verse;

namespace FrontierDevelopments.Shields.BadHygiene
{
    public class Comp_DubsAirVent : CompAirconIndoorUnit
    {
        private bool _connected = true;
        private float _capacity = 0f;

        public bool Active => _connected && PipeComp.pipeNet.CoolingCap > 0;

        public float VentCapacity
        {
            get => WorkingNow? _capacity : 0f;
            set => _capacity = value;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _capacity = Props.Capacity;
        }

        public override string CompInspectStringExtra()
        {
            if (!Active || ParentHolder is MinifiedThing)
            {
                return base.CompInspectStringExtra();
            }
            var stringBuilder = new StringBuilder();
            if (PipeComp.pipeNet.CoolingCap < 1f)
            {
                stringBuilder.Append("LowCoolingCap".Translate());
                stringBuilder.Append("\n");
            }
            stringBuilder.Append("CoolingUnits".Translate((int)VentCapacity));
            return stringBuilder.ToString().TrimEndNewlines();
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
                    icon = Resources.BuildingAirConBaseUnit,
                    defaultDesc = "fd.heatsink.dubs.net.connect.description".Translate(),
                    defaultLabel = "fd.heatsink.dubs.net.connect.label".Translate(),
                    isActive = () => _connected,
                    toggleAction = () => _connected = !_connected
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _connected, "connectedToDubsCoolingNetwork", true);
            Scribe_Values.Look(ref _capacity, "ventCapacity");
        }
    }
}