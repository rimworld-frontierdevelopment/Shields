using System;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields.Handlers
{
    public class ActiveDropPodHandler
    {
        static ActiveDropPodHandler()
        {
            Log.Message("Frontier Developments Shields :: ActiveDropPod handler enabled");
        }
        
        [HarmonyPatch(typeof(ActiveDropPod), "Tick")]
        static class Patch_ProjectileCE_Tick
        {
            static bool Prefix(ActiveDropPod __instance)
            {
                try
                {
                    var pod = __instance;
                    return !Mod.ShieldManager.ImpactShield(pod.Map, pod.Position.ToVector3(), (shield, point) =>
                    {
                        if (shield.Damage(Mod.Settings.DropPodDamage, point))
                        {
                            foreach (var pawn in pod.Contents.innerContainer.Where(p => p is Pawn))
                            {
                                pawn.Kill(new DamageInfo(new DamageDef(), 100));
                            }
                            pod.Destroy();
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
