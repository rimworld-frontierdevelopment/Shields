using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields
{
    public static class ShieldSettingsClipboard
    {
        private static List<ShieldSetting> _copied = new List<ShieldSetting>();

        public static IEnumerable<Gizmo> Gizmos(IShield shield)
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings"),
                defaultLabel = "CommandCopyZoneSettingsLabel".Translate(),
                defaultDesc = "CommandCopyZoneSettingsDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc4,
                action = () =>
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    _copied = shield.ShieldSettings.ToList();
                }
            };
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings"),
                defaultLabel = "CommandPasteZoneSettingsLabel".Translate(),
                defaultDesc = "CommandPasteZoneSettingsDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc5,
                action = () =>
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    shield.ShieldSettings = _copied;
                }
                // disabled = _copied.Count > 0
            };
        }
    }
}