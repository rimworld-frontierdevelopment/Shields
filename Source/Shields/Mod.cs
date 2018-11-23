using System.Reflection;
using FrontierDevelopments.Shields.Harmony;
using Harmony;
using RimWorld;
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
        private const string RedistHeatName = "RedistHeat";
        
        public Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
            ModName = content.Name;
            
            var harmony = HarmonyInstance.Create("frontierdevelopment.shields");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Harmony_Verb.BlacklistType(typeof(Verb_Bombardment));
            
            Log.Message("Frontier Developments Shields :: Loaded");
            LoadOneTemperatureMod(harmony);
        }

        private void LoadOneTemperatureMod(HarmonyInstance harmony)
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
                        if(Settings.EnableRedistHeatSupport && ModLister.HasActiveModWithName(RedistHeatName))
                            Log.Warning("Frontier Developments Shields :: detected both " + CentralizedClimateControlName + " and " + RedistHeatName +" active. Using " + CentralizedClimateControlName + " since it is loaded first");
                        new ClimateControlIntegration().TryEnable(harmony);
                        return;
                    case "RedistHeat":
                        if (!Settings.EnableRedistHeatSupport)
                        {
                            Log.Message("Frontier Developments Shields :: disabling RedistHeat support");
                            continue;
                        }
                        if(Settings.EnableCentralizedClimateControlSupport && ModLister.HasActiveModWithName(CentralizedClimateControlName))
                            Log.Warning("Frontier Developments Shields :: detected both " + CentralizedClimateControlName + " and " + RedistHeatName +" active. Using " + RedistHeatName + " since it is loaded first");
                        new RedistHeatIntegration().TryEnable(harmony);
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

        [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        class Patch_GenerateImpliedDefs_PostResolve
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                Harmony_Explosion.BlockType(DamageDefOf.Bomb.defName);
            }
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