using System.Reflection;
using Verse;

namespace FrontierDevelopments.Shields.SrtsExpandedModule
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            var harmony = new HarmonyLib.Harmony("FrontierDevelopments.Shields.SrtsExpanded");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}