using System.Reflection;
using FrontierDevelopments.Shields.Module.RimworldModule;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Mod : Verse.Mod
    {
        public static Settings Settings;
        public static readonly ShieldManager ShieldManager = new ShieldManager();
        
        public Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
            
            var harmony = HarmonyInstance.Create("frontierdevelopment.shields");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Harmony_Verb_CanHitCellFromCellIgnoringRange.BlacklistType(typeof(Verb_Bombardment));
            
            Log.Message("Fronter Developments Shields :: Loaded");
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
        public static readonly Texture2D UiBlank = ContentFinder<Texture2D>.Get("UI/Buttons/blank");
    }  
}