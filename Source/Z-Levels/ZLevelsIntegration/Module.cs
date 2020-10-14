using System.Reflection;
using Verse;

namespace FrontierDevelopments.Shields.ZLevelsIntegration
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            var harmony = new HarmonyLib.Harmony("FrontierDevelopments.Shields.ZLevels");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}