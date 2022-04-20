using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.ClimateControl
{
    [StaticConstructorOnStartup]
    public class Resources
    {
        public static readonly Texture2D BuildingClimateControlAirThermal;

        static Resources()
        {
            BuildingClimateControlAirThermal = GetModTexture("Centralized Climate Control (Continued)", "Things/Building/AirThermal_north");
        }

        static Texture2D GetModTexture(string modName, string resourcePath)
        {
            return ModLister.HasActiveModWithName(modName)
                ? ContentFinder<Texture2D>.Get(resourcePath, ShaderDatabase.Transparent)
                : null;
        }
    }
}