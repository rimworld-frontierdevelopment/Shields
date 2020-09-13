using System.Reflection;
using FrontierDevelopments.Shields.Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            var harmony = new HarmonyLib.Harmony("FrontierDevelopments.Shields1.2");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Harmony_Verb.BlacklistType(typeof(Verb_Bombardment));
            
            // support for Cargo Pod transport
            Harmony_Skyfaller.WhitelistDef("HelicopterIncoming");
            Harmony_Skyfaller.WhitelistDef("HelicopterLeaving");
        }
    }
}