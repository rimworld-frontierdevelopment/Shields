using System.Collections.Generic;
using FrontierDevelopments.General.UI;
using HarmonyLib;
using Verse;

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

        public void SetParent(IShieldParent shieldParent)
        {
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            parent.Map.GetComponent<ShieldManager>().Add(Fields);
        }

        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<ShieldManager>().Del(Fields);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            previousMap.GetComponent<ShieldManager>().Del(Fields);
        }

        public bool IsActive()
        {
            return _deployed.IsActive();
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
