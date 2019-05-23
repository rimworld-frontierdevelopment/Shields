using System;
using FrontierDevelopments.General;

namespace FrontierDevelopments.Shields
{
    public class RedistHeatIntegration : ModIntegration
    {
        public override string ThisModName => Mod.ModName;
        public override string OtherModName => "RedistHeat";
        public override string OtherModAssemblyName => "redistHeat";
        public override Version OtherModVersion => null; // TODO Morgloz released 50 tagged as version 47. Can't check :(
        public override string IntegrationAssemblyPath => "Integrations/FrontierDevelopments-Shields-RedistHeat.dll";
    }
}