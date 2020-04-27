using FrontierDevelopments.General.Energy;
using FrontierDevelopments.Shields.Comps;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FrontierDevelopments.Shields
{
    public static class ShieldDeploymentUtility
    {
        public static bool CanDeploy(Pawn pawn, IShield shield)
        {
            return pawn.RaceProps.baseBodySize >= shield.DeploymentSize;
        }

        public static void DeployShield(Pawn pawn, IShield shield)
        {
            var deployed = new Comp_DeployedShield(shield);
            deployed.props = new CompProperties_DeployedShield();

            switch (shield)
            {
                case IEnergyNode node:
                    node.ConnectTo(null); // TODO get pawn net
                    break;
            }

            pawn.Map.GetComponent<ShieldManager>().Add(shield);
            pawn.AllComps.Add(deployed);
        }

        public static void UndeployShield(Pawn pawn, IShield shield)
        {
            pawn.AllComps
                .Where(comp => comp == shield)
                .Do(comp =>
                {
                    pawn.AllComps.Remove(comp);
                });

            switch (shield)
            {
                case IEnergyNode node:
                    node.Disconnect();
                    break;
            }

            pawn.Map.GetComponent<ShieldManager>().Del(shield);
        }

        public static IEnumerable<IShield> DeployedShields(Pawn pawn)
        {
            var results = new List<IShield>();

            var deployed = pawn.TryGetComp<Comp_DeployedShield>();
            if (deployed != null) results.Add(deployed);

            return results;
        }

        public static bool ItemProvidesShield(Thing item, IShield shield)
        {
            switch (item)
            {
                case IShield itemShield:
                    return shield == itemShield;

                case MinifiedThing minified:
                    return shield == minified.InnerThing;
            }
            return false;
        }
    }
}