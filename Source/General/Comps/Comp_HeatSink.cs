using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General.CompProperties;
using FrontierDevelopments.Shields;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.General.Comps
{
    public class Comp_HeatSink : ThingComp, IHeatsink
    {
        public static readonly float KELVIN_ZERO_CELCIUS = 273.15f;

        private float _dissipationRate;
        private float _temperature = -500f;
        private bool _thermalShutoff = true;

        public bool CanBreakdown => !_thermalShutoff && OverMinorThreshold;
        
        public CompProperties_HeatSink Props => (CompProperties_HeatSink)props;

        public float Temp => _temperature; 

        private float Joules
        {
            get => (_temperature + KELVIN_ZERO_CELCIUS) * Props.specificHeat * Props.grams;
            set =>  _temperature = value / Props.specificHeat / Props.grams - KELVIN_ZERO_CELCIUS;
        }

        public bool OverTemperature => _thermalShutoff && OverMinorThreshold;

        public bool OverMinorThreshold => Temp >= Props.minorThreshold;
        
        public bool OverMajorThreshold => Temp >= Props.majorThreshold;
        
        public bool OverCriticalThreshold => Temp >= Props.criticalThreshold;

        public virtual float MaximumTemperature => Props.maximumTemperature;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (_temperature < -KELVIN_ZERO_CELCIUS) _temperature = parent.AmbientTemperature;
            _dissipationRate = GenDate.TicksPerDay / Props.conductivity;
        }

        public void PushHeat(float wattDays)
        {
            Joules += wattDays / 86.4f * 1000;
        }

        protected virtual float AmbientTemp()
        {
            return parent.AmbientTemperature;
        }

        protected virtual void DissipateHeat(float kilojoules)
        {
            GenTemperature.PushHeat(parent, kilojoules);
        }

        public override void CompTick()
        {
            if (Temp >= MaximumTemperature)
            {
                DoCriticalBreakdown();
            }
            else
            {
                var heatDissipated =  (Temp - AmbientTemp()) / _dissipationRate;
                Joules -= heatDissipated * 1000f;
                DissipateHeat(heatDissipated);
            }
        }

        public override string CompInspectStringExtra()
        {
            LessonAutoActivator.TeachOpportunity(ConceptDef.Named("FD_CoilTemperature"), OpportunityType.Important);
            return "fd.heatsink.temperature".Translate() + ": " + Temp.ToStringTemperature();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _temperature, "temperature", -500f);
            Scribe_Values.Look(ref _thermalShutoff, "thermalShutoff", true);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    icon = Resources.UiThermalShutoff,
                    defaultDesc = "thermal_shutoff.description".Translate(),
                    defaultLabel = "thermal_shutoff.label".Translate(),
                    isActive = () => _thermalShutoff,
                    toggleAction = () => _thermalShutoff = !_thermalShutoff
                };
            }
        }

        public void DoMinorBreakdown()
        {
            BreakdownMessage(
                "fd.shields.incident.minor.title".Translate(), 
                "fd.shields.incident.minor.body".Translate(), 
                MinorBreakdown());
        }

        public void DoMajorBreakdown()
        {
            BreakdownMessage(
                "fd.shields.incident.major.title".Translate(), 
                "fd.shields.incident.major.body".Translate(), 
                MajorBreakdown());
        }

        public void DoCriticalBreakdown()
        {
            BreakdownMessage(
                "fd.shields.incident.critical.title".Translate(), 
                "fd.shields.incident.critical.body".Translate(), 
                CriticalBreakdown());
            parent.Destroy(DestroyMode.KillFinalize);
        }
        
        private float MinorBreakdown()
        {
            var energySource = EnergySourceUtility.Find(parent);
            if (energySource == null) return 0;
            var amount = energySource.EnergyAvailable * (float) new Random().NextDouble();
            energySource.Drain(amount);
            return amount;
        }

        private float MajorBreakdown()
        {
            parent.GetComp<CompBreakdownable>().DoBreakdown();
            if (parent.Faction == Faction.OfPlayer)
            {
                // manually remove the default letter...
                try
                {
                    Find.LetterStack.RemoveLetter(
                        Find.LetterStack.LettersListForReading
                            .First(letter => letter.lookTargets.targets.Any(t => t.Thing == parent)));
                    
                }
                catch (Exception)
                {
                }
            }
            return MinorBreakdown();
        }

        private float CriticalBreakdown()
        {
            GenExplosion.DoExplosion(
                parent.Position,
                parent.Map,
                3.5f,
                DamageDefOf.Flame,
                parent);
            return MajorBreakdown();
        }
        
        private void BreakdownMessage(string title, string body, float drained)
        {
            if (parent.Faction != Faction.OfPlayer) return;
            Find.LetterStack.ReceiveLetter(
                title,
                body.Replace("{0}", ((int)drained).ToString()), 
                LetterDefOf.NegativeEvent, 
                new TargetInfo(parent.Position, parent.Map));
        }
    }
}
