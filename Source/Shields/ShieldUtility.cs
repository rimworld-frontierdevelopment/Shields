using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FrontierDevelopments.Shields
{
    public static class ShieldUtility
    {
        public static IShield Find(ThingWithComps parent)
        {
            switch (parent)
            {
                case IShield parentSource:
                    return parentSource;

                default:
                    return FindComp(parent.AllComps);
            }
        }

        public static IShield FindComp(IEnumerable<ThingComp> comps)
        {
            try
            {
                return comps.OfType<IShield>().First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static IEnumerable<IShield> AllShields(Pawn pawn)
        {
            foreach (var shield in InventoryShields(pawn))
            {
                yield return shield;
            }

            foreach (var shield in EquipmentShields(pawn))
            {
                yield return shield;
            }

            foreach (var shield in HediffShields(pawn))
            {
                yield return shield;
            }
        }

        public static IEnumerable<IShield> InventoryShields(Pawn pawn)
        {
            if (pawn?.inventory?.innerContainer == null) yield break;
            foreach (var thing in pawn.inventory.innerContainer)
            {
                switch (thing)
                {
                    case MinifiedThing minified:
                        switch (minified.InnerThing)
                        {
                            case ThingWithComps thingWithComps:
                                foreach (var comp in thingWithComps.AllComps)
                                {
                                    switch (comp)
                                    {
                                        case IShield shield:
                                            yield return shield;
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                }
            }
        }

        public static IEnumerable<IShield> EquipmentShields(Pawn pawn)
        {
            if (pawn.equipment == null) yield break;
            foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
            {
                foreach (var comp in equipment.AllComps)
                {
                    switch (comp)
                    {
                        case IShield shield:
                            yield return shield;
                            break;
                    }
                }
            }
        }

        public static IEnumerable<IShield> HediffShields(Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null) yield break;
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                switch (hediff)
                {
                    case IShield shield:
                        yield return shield;
                        break;
                }
            }
        }
    }
}