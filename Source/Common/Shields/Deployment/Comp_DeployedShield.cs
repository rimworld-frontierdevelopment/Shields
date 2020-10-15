using System.Collections.Generic;
using FrontierDevelopments.General.UI;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Shields.Comps
{
    public class CompProperties_DeployedShield : CompProperties
    {
        public CompProperties_DeployedShield()
        {
            compClass = typeof(Comp_DeployedShield);
        }
    }

    public class Comp_DeployedShield : ThingComp, IShield
    {
        private IShield _deployed;
        private int _id;
        private bool _isActive = true;

        public IEnumerable<IShieldStatus> Status => _deployed.Status;

        public string TextStats => _deployed.TextStats;

        public Comp_DeployedShield()
        {
        }

        public Comp_DeployedShield(IShield deployed)
        {
            _deployed = deployed;
            _id = Find.UniqueIDsManager.GetNextThingID();
        }

        public string Label => _deployed.Label;
        public float DeploymentSize => _deployed.DeploymentSize;
        public IEnumerable<Gizmo> ShieldGizmos => _deployed.ShieldGizmos;
        public IShieldParent Parent => _deployed.Parent;

        public bool HasWantSettings => _deployed.HasWantSettings;

        public bool PresentOnMap(Map map) => _deployed.PresentOnMap(map);

        public void SetParent(IShieldParent shieldParent)
        {
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            ShieldManager.For(parent.Map).Add(Fields);
        }

        public override void PostDeSpawn(Map map)
        {
            ShieldManager.For(map).Del(Fields);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            ShieldManager.For(previousMap).Del(Fields);
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            return _deployed.ThreatDisabled(disabledFor);
        }

        public LocalTargetInfo TargetCurrentlyAimingAt => (parent as Pawn)?.TargetCurrentlyAimingAt ?? null;

        public float TargetPriorityFactor => _deployed.TargetPriorityFactor;

        public bool IsActive()
        {
            return _deployed.IsActive();
        }

        public void NotifyWantSettings()
        {
            _deployed.NotifyWantSettings();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            foreach (var gizmo in _deployed.ShieldGizmos)
            {
                yield return gizmo;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _id, "shieldDeployedId");
            Scribe_References.Look(ref _deployed, "shieldDeployedReference");
        }

        public string GetUniqueLoadID()
        {
            return "DeployedShield" + _id;
        }

        public IEnumerable<UiComponent> UiComponents => _deployed.UiComponents;

        public IEnumerable<ShieldSetting> ShieldSettings
        {
            get => _deployed.ShieldSettings;
            set => _deployed.ShieldSettings = value;
        }

        public void ClearWantSettings()
        {
            _deployed.ClearWantSettings();
        }

        public IEnumerable<IShieldField> Fields => _deployed.Fields;

        public Thing Thing => parent;
    }
}
