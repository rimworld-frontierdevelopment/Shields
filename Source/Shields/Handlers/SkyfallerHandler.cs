using System;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Handlers
{
    public class SkyfallerHandler
    {
        static SkyfallerHandler()
        {
            Log.Message("Frontier Developments Shields :: Skyfaller handler enabled");
        }

        [HarmonyPatch(typeof(Skyfaller), "Tick")]
        static class Patch_ProjectileCE_Tick
        {
            static bool Prefix(Skyfaller __instance)
            {
                try
                {
                    var skyfaller = __instance;
                    return !Mod.ShieldManager.ImpactShield(skyfaller.Map, Common.ToVector2(skyfaller.Position.ToVector3()), (shield, point) =>
                    {
                        if (skyfaller.ticksToImpact <= 1 && shield.Damage(Mod.Settings.SkyfallerDamage, point))
                        {
                            skyfaller.def.skyfaller.impactSound?.PlayOneShot(
                                SoundInfo.InMap(new TargetInfo(skyfaller.Position, skyfaller.Map)));
                            skyfaller.Destroy();
                            return true;
                        }
                        return false;
                    });
                }
                catch (InvalidOperationException) {}
                return true;
            }
        }
    }
}
