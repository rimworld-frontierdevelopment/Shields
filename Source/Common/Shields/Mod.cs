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

        private const string CentralizedClimateControlName = "Centralized Climate Control";
        
        public Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
            ModName = content.Name;
            
            var harmony = new HarmonyLib.Harmony("FrontierDevelopments.Shields");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

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