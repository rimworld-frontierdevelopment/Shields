using System;
using System.Reflection;
using Harmony;
using Verse;

namespace FrontierDevelopments.Core
{
    public abstract class ModIntegration
    {
        public virtual string ThisModName { get; }
        public virtual string OtherModName { get; }
        public virtual string OtherModAssemblyName { get; }
        public virtual Version OtherModVersion { get; }
        public virtual string IntegrationAssemblyPath { get; }

        public bool TryEnable(HarmonyInstance harmony)
        {
            try
            {
                var otherAssembly = Assembly.Load(OtherModAssemblyName);
                if (otherAssembly != null)
                {
                    var version = new AssemblyName(otherAssembly.FullName).Version;
                    if (version == OtherModVersion || OtherModVersion == null)
                    {
                        var assembly = AssemblyUtility.FindModAssembly(ThisModName, IntegrationAssemblyPath);
                        if (assembly != null)
                        {
                            harmony.PatchAll(assembly);
                            Log.Message("Frontier Developments Shields :: enabled " + OtherModName + " support");
                            return true;
                        }
                        else
                        {
                            Log.Warning("Frontier Developments Shields :: unable to load " + OtherModName + " support assembly");
                        }
                    }
                    else
                    {
                        Log.Warning("Frontier Developments Shields :: " + OtherModName + " " + version + 
                                    "is loaded and " + OtherModVersion + " is required, not enabling support");
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}