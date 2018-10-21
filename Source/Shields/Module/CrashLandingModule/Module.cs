using System;
using System.Reflection;
using Harmony;
using Verse;

namespace FrontierDevelopments.Shields.Module.CrashLandingModule
{
    public class Module : Verse.Mod
    {
        public Module(ModContentPack content) : base(content)
        {
            if (ModIsRunning("Crash Landing"))
            {
                try
                {
                    ((Action) (() =>
                            {
                                var harmony = HarmonyInstance.Create("frontierdevelopment.shields");
                                var baseType = typeof(CrashLanding.CrashPod);
                                var types = baseType.AllSubclassesNonAbstract().Add(baseType);
                                var blockingTypes = "";
                                
                                foreach (Type current in types)
                                {
                                    blockingTypes += current.Name + " ";
                                    harmony.Patch(
                                        current.GetMethod("Impact", BindingFlags.NonPublic | BindingFlags.Instance),
                                        new HarmonyMethod(typeof(CrashPodHandler.Patch_CrashPod_Impact).GetMethod("Prefix")));
                                }
                                
                                Log.Message("Frontier Developments Shields :: Crash Landing support enabled. Blocking (" + blockingTypes.Trim() + ")");
                            }
                        ))();
                }
                catch (Exception) {}
            }
        }
        
        private bool ModIsRunning(string name)
        {
            try
            {
                return LoadedModManager.RunningModsListForReading.Find(content => content.Name == name) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}