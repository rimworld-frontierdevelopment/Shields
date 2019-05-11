using System;
using FrontierDevelopments.Core;

namespace FrontierDevelopments.Shields
{
    public class CombatExtendedIntegration : ModIntegration
    {
        public override string ThisModName => Mod.ModName;
        public override string OtherModName => "Combat Extended";
        public override string OtherModAssemblyName => "CombatExtended";
        public override Version OtherModVersion => null;//new Version(1, 5, 0, 0);
        public override string IntegrationAssemblyPath => "Integrations/FrontierDevelopments-Shields-CombatExtended.dll";
    }
}