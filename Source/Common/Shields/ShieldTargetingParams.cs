using System.Linq;
using FrontierDevelopments.General;
using RimWorld;

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

                var shielded = new ShieldQuery(targetInfo.Map)
                    .FriendlyTo(faction, true)
                    .Intersects(PositionUtility.ToVector3(targetInfo.Cell))
                    .Get()
                    .Any();
                
                return !shielded && goodDropSpot;
            }
        };
    }
}