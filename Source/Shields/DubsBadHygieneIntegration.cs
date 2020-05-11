using FrontierDevelopments.General;
using System;

namespace FrontierDevelopments.Shields
{
    public class DubsBadHygieneIntegration : ModIntegration
    {
        public override string ThisModName => Mod.ModName;
        public override string OtherModName => "Dubs Bad Hygiene";
        public override string OtherModAssemblyName => "BadHygiene";
        public override Version OtherModVersion => new Version(2, 7, 7273, 33335);
        public override string IntegrationAssemblyPath => "1.1/Integrations/FrontierDevelopments-Shields-BadHygiene.dll";
    }
}