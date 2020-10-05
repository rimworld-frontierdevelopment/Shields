using System.Reflection;
using Verse;

namespace FrontierDevelopments.Shields.AvoidFriendlyFireIntegration
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            var harmony = new HarmonyLib.Harmony("FrontierDevelopments.Shields.AvoidFriendlyFire");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}