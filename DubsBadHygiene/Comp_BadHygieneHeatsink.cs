using FrontierDevelopments.General.Comps;
using System;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.BadHygiene
{
    public class CompProperties_BadHygieneHeatsink : CompProperties_HeatSink
    {
        public CompProperties_BadHygieneHeatsink()
        {
            compClass = typeof(Comp_BadHygieneHeatsink);
        }
    }

    internal class Comp_BadHygieneHeatsink : Comp_HeatSink
    {
        private ThingComp _airFlowConsumer;
        private bool HasAirflow => Cooling?.WorkingNow ?? false;

        private Comp_ShieldCooling Cooling => (Comp_ShieldCooling)_airFlowConsumer;

        private const float TargetTemp = 21f;
        public float TempDiff => TargetTemp - Temp;
        public float CoolingCapacity => Cooling.Capacity;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _airFlowConsumer = parent.TryGetComp<Comp_ShieldCooling>();
        }

        protected override float AmbientTemp()
        {
            return HasAirflow ? -1f : base.AmbientTemp();
        }

        protected override void DissipateHeat(float kilojoules)
        {
            if (Cooling.WorkingNow)
            {
                 base.DissipateHeat(kilojoules);
            }
            else
            {
                base.DissipateHeat(kilojoules);
            }
        }
    }
}