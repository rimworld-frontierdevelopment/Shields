using FrontierDevelopments.General;
using System;

namespace FrontierDevelopments.Shields
{
    public class CombatExtendedIntegration : ModIntegration
    {
        public override string ThisModName => Mod.ModName;
        public override string OtherModName => "Combat Extended";
        public override string OtherModAssemblyName => "CombatExtended";
        public override Version OtherModVersion => new Version(1, 0, 1, 6);
        public override string IntegrationAssemblyPath => "Integrations/FrontierDevelopments-Shields-CombatExtended.dll";
    }
}