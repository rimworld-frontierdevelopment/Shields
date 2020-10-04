using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields
{
    public interface IShieldStatus
    {
        bool Online { get; }
        string Description { get; }
    }

    public static class ShieldStatus
    {
        public static bool IsOnline(IEnumerable<IShieldStatus> statuses)
        {
            return statuses.All(status => status.Online);
        }

        public static string GetStringFromStatuses(List<IShieldStatus> statuses)
        {
            var result = new StringBuilder();
            result.AppendLine(IsOnline(statuses)
                ? "shield.status.online".Translate()
                : "shield.status.offline".Translate());
            statuses.Do(status => result.AppendLine(status.Description));
            return result.ToString().TrimEndNewlines();
        }
    }
}