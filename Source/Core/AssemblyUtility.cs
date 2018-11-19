using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace FrontierDevelopments.Core
{
    public class AssemblyUtility
    {
        public static Assembly FindModAssembly(string modName, string pathInModFolder)
        {
            try
            {
                var modMetaData = ModLister.AllInstalledMods
                    .First(mod => mod.Active && mod.Name == modName);
                return Assembly.LoadFrom(modMetaData.RootDir.FullName + "/" + pathInModFolder);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
