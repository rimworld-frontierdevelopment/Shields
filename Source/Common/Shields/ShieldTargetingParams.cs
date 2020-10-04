using System.Linq;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields.Linear;
using RimWorld;
using UnityEngine;

namespace FrontierDevelopments.Shields
{
    public class ShieldTargetingParams
    {
        public TargetingParameters ForShip(Faction faction) => new TargetingParameters()
        {
            canTargetLocations = true,
            canTargetSelf = false,
            canTargetPawns = false,
            canTargetFires = false,
            canTargetBuildings = false,
            canTargetItems = false,
            validator = targetInfo =>
            {
                Mod.SetDropCellCheckEnabled(false);
                var goodDropSpot = DropCellFinder.IsGoodDropSpot(targetInfo.Cell, targetInfo.Map, false, true);
                Mod.SetDropCellCheckEnabled(true);

                var shielded = new FieldQuery(targetInfo.Map)
                    .FriendlyTo(faction, true)
                    .Intersects(PositionUtility.ToVector3(targetInfo.Cell))
                    .Get()
                    .Any();
                
                return !shielded && goodDropSpot;
            }
        };

        public static TargetingParameters LinearLink(ILinearShield source) => new TargetingParameters
        {
            canTargetLocations = false,
            canTargetSelf = false,
            canTargetPawns = false,
            canTargetFires = false,
            canTargetBuildings = true,
            canTargetItems = false,
            validator = targetInfo =>
            {
                var shield = LinearShieldUtility.Find(targetInfo.Thing);
                if (shield != null && source.CanLinkWith(shield))
                {
                    // TODO add efficiency
                    LinearShieldUtility.DrawFieldBetween(source.Position, shield.Position, shield.Map, Color.grey);
                    return true;
                }
                return false;
            }
        };
    }
}