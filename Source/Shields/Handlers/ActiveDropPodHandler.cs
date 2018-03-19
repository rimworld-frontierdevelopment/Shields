using System;
using System.Linq;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
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
                    if (pod?.Map == null) return true;
                    Mod.ShieldManager.ImpactShield(pod.Map, Common.ToVector3WithY(pod.Position, 0), (shield, point) =>
                    {
                        if (shield.IsActive())
                        {
                            if (shield.Damage(Mod.Settings.DropPodDamage, point))
                            {
                                foreach (var pawn in pod.Contents.innerContainer.Where(p => p is Pawn))
                                {
                                    pawn.Kill(new DamageInfo(new DamageDef(), 100));
                                }
                                pod.Destroy();
                                Messages.Message("fd.shields.incident.droppod.blocked.body".Translate(), new GlobalTargetInfo(pod.Position, pod.Map), MessageTypeDefOf.NeutralEvent);
                                return true;
                            }
                            Messages.Message("fd.shields.incident.droppod.not_blocked.body".Translate(), new GlobalTargetInfo(pod.Position, pod.Map), MessageTypeDefOf.NegativeEvent);
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
