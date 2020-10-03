using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.BadHygiene
{
    [StaticConstructorOnStartup]
    public class Resources
    {
        public static readonly Texture2D BuildingAirConBaseUnit;

        static Resources()
        {
            BuildingAirConBaseUnit = GetModTexture("Dubs Bad Hygiene", "DBH/Things/Building/Heating/Aircon");
        }
        
        static Texture2D GetModTexture(string modName, string resourcePath)
        {
            return ModLister.HasActiveModWithName(modName)
                ? ContentFinder<Texture2D>.Get(resourcePath, ShaderDatabase.Transparent)
                : null;
        }
    }
}