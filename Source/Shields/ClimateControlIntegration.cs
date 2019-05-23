using System;
using FrontierDevelopments.General;

namespace FrontierDevelopments.Shields
{
    public class ClimateControlIntegration : ModIntegration
    {
        public override string ThisModName => Mod.ModName;
        public override string OtherModName => "Centralized Climate Control";
        public override string OtherModAssemblyName => "CentralizedClimateControl";
        public override Version OtherModVersion => new Version(1, 5, 0, 0);
        public override string IntegrationAssemblyPath => "Integrations/FrontierDevelopments-Shields-ClimateControl.dll";
    }
}