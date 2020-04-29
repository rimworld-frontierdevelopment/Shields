using FrontierDevelopments.Shields.Harmony;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    [StaticConstructorOnStartup]
    public class Mod : Verse.Mod
    {
        public static string ModName;
        public static Settings Settings;

        private const string CentralizedClimateControlName = "Centralized Climate Control (Continued)";
        private const string DubsBadHygiene = "Dubs Bad Hygiene";

        public Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
            ModName = content.Name;

            var harmony = new HarmonyLib.Harmony("frontierdevelopment.shields");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            var method = AccessTools.FirstMethod(typeof(Bombardment),
                inner => inner.Name.StartsWith("<StartRandomFire>")
                && inner.ReturnType == typeof(bool));

            harmony.Patch(method, new HarmonyMethod(typeof(Harmony_Bombardment), nameof(Harmony_Bombardment.Prefix)));

            Harmony_Verb.BlacklistType(typeof(Verb_Bombardment));

            // support for Cargo Pod transport
            Harmony_Skyfaller.WhitelistDef("HelicopterIncoming");
            Harmony_Skyfaller.WhitelistDef("HelicopterLeaving");

            LoadOneTemperatureMod(harmony);

            new CombatExtendedIntegration().TryEnable(harmony);
        }

        private void LoadOneTemperatureMod(HarmonyLib.Harmony harmony)
        {
            foreach (var mod in ModsConfig.ActiveModsInLoadOrder)
            {
                switch (mod.Name)
                {
                    case CentralizedClimateControlName:
                        if (!Settings.EnableCentralizedClimateControlSupport)
                        {
                            Log.Message("Frontier Developments Shields :: disabling Centralized Climate Control support");
                            continue;
                        }
                        new ClimateControlIntegration().TryEnable(harmony);
                        return;

                    case DubsBadHygiene:
                        if (!Settings.EnableDubsBadHygieneSupport)
                        {
                            Log.Message("Frontier Developments Shields :: disabling Dubs Bad Hygiene support");
                            continue;
                        }
                        new DubsBadHygieneIntegration().TryEnable(harmony);
                        return;
                }
            }
        }

        public override string SettingsCategory()
        {
            return "Frontier Developments Shields";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect);
        }
    }

    [StaticConstructorOnStartup]
    public static class Resources
    {
        public static readonly Material ShieldMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
        public static readonly Texture2D UiThermalShutoff = ContentFinder<Texture2D>.Get("UI/Buttons/ThermalShutoff");
        public static readonly Texture2D UiSetRadius = ContentFinder<Texture2D>.Get("UI/Buttons/Radius");
        public static readonly Texture2D UiChargeBattery = ContentFinder<Texture2D>.Get("UI/Buttons/PortableShieldDraw");
        public static readonly Texture2D UiToggleVisibility = ContentFinder<Texture2D>.Get("Other/ShieldBubble", ShaderDatabase.Transparent);
    }
}