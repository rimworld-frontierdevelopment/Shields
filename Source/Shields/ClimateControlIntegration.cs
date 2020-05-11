using FrontierDevelopments.General;
using System;

namespace FrontierDevelopments.Shields
{
    public class ClimateControlIntegration : ModIntegration
    {
        public override string ThisModName => Mod.ModName;
        public override string OtherModName => "Centralized Climate Control";
        public override string OtherModAssemblyName => "CentralizedClimateControl";
        public override Version OtherModVersion => new Version(1, 5, 0, 0);
        public override string IntegrationAssemblyPath => "1.1/Integrations/FrontierDevelopments-Shields-ClimateControl.dll";
    }
}