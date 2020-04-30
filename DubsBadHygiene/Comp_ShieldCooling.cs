using DubsBadHygiene;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.BadHygiene
{
    internal class Comp_ShieldCooling : CompAirconIndoorUnit
    {
        // Todo: Not necessary for working compat, but a nice addition when I get time - Wiri
        //public override string CompInspectStringExtra()
        //{
        //    if (base.ParentHolder is MinifiedThing)
        //    {
        //        return base.CompInspectStringExtra();
        //    }
        //    StringBuilder stringBuilder = new StringBuilder();
        //    if (DebugSettings.godMode)
        //    {
        //        stringBuilder.AppendLine("Cooling per cell:" + CoolingUsed);
        //    }
        //    stringBuilder.AppendLine("ConnectedCoolingCapacity".Translate(PipeComp.pipeNet.OutdoorUnitCapacitySum, PipeComp.pipeNet.CoolingCap.ToStringPercent("0.00")));
        //    if (PipeComp.pipeNet.CoolingCap < 1f)
        //    {
        //        stringBuilder.Append("LowCoolingCap".Translate());
        //    }
        //    stringBuilder.Append("CoolingUnits".Translate(base.Props.Capacity));
        //    return stringBuilder.ToString().TrimEndNewlines();
        //}

        //public override IEnumerable<Gizmo> CompGetGizmosExtra()
        //{
        //    yield return new Command_Action
        //    {
        //        action = ReduceCooling,
        //        defaultLabel = "Decrease Cooling",
        //        defaultDesc = "Decrease the amount of cooling sent to a generator".Translate(),
        //        icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower")
        //    };
        //    yield return new Command_Action
        //    {
        //        action = AddCooling,
        //        defaultLabel = "Increase Cooling",
        //        defaultDesc = "Increase the amount of cooling sent to a generator",
        //        icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise")
        //    };
        //}

        //public void AddCooling()
        //{
        //    this.Props.Capacity += 100;
        //}

        //public void ReduceCooling()
        //{
        //    if (this.Props.Capacity > 100)
        //        this.Props.Capacity -= 100;
        //}
    }
}