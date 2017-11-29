using System;
using System.Reflection;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Handlers
{
    public class ProjectileHandler
    {
        private static readonly bool Enabled = true;
        
        private static readonly FieldInfo OriginField = typeof(Projectile).GetField("origin", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo DestinationField = typeof(Projectile).GetField("destination", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo TicksToImpactField = typeof(Projectile).GetField("ticksToImpact", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo AssignedTargetField = typeof(Projectile).GetField("assignedTarget", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo IntendedTargetField = typeof(Projectile).GetField("intendedTarget", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo StartingTicksToImpactProperty = typeof(Projectile).GetProperty("StartingTicksToImpact", BindingFlags.Instance | BindingFlags.NonPublic);

        static ProjectileHandler()
        {
            if (OriginField == null)
            {
                Enabled = false;
                Log.Error("Frontier Developments Shields :: Projectile handler reflection error on field Projectile.origin");
            }
            if (DestinationField == null)
            {
                Enabled = false;
                Log.Error("Frontier Developments Shields :: Projectile handler reflection error on field Projectile.destination");
            }
            if (TicksToImpactField == null)
            {
                Enabled = false;
                Log.Error("Frontier Developments Shields :: Projectile handler reflection error on field Projectile.ticksToImpact");
            }
            if (AssignedTargetField == null)
            {
                Enabled = false;
                Log.Error("Frontier Developments Shields :: Projectile handler reflection error on field Projectile.assignedTarget");
            }
            if (IntendedTargetField == null)
            {
                Enabled = false;
                Log.Error("Frontier Developments Shields :: Projectile handler reflection error on field Projectile.intendedTarget");
            }
            if (StartingTicksToImpactProperty == null)
            {
                Enabled = false;
                Log.Error("Frontier Developments Shields :: Projectile handler reflection error on property Projectile.StartingTicksToImpact");
            }
            
            Log.Message("Frontier Developments Shields :: Projectile handler " + (Enabled ? "enabled" : "disabled due to errors"));
        }
        
        [HarmonyPatch(typeof(Projectile), "Tick")]
        static class Patch_Projectile_Tick
        {
            static bool Prefix(Projectile __instance)
            {
                if (!Enabled) return true;
                
                var projectile = __instance;
                    
                var ticksToImpact = (int)TicksToImpactField.GetValue(projectile);
                var startingTicksToImpact = (int)StartingTicksToImpactProperty.GetValue(projectile, null);

                var origin = Common.ToVector2((Vector3) OriginField.GetValue(projectile));
                var destination = Common.ToVector2((Vector3) DestinationField.GetValue(projectile));
                var position = Vector2.Lerp(origin, destination, 1.0f - ticksToImpact / (float)startingTicksToImpact);
                
                try
                {
                    if (projectile.def.projectile.flyOverhead)
                    {
                        // the shield has blocked the projectile - invert to get if harmony should allow the original block
                        return !Mod.ShieldManager.ImpactShield(projectile.Map, position, origin, destination, (shield, vector2) =>
                            {
                                if (shield.Damage(projectile.def.projectile.damageAmountBase, position))
                                {
                                    projectile.Destroy();
                                    return true;
                                }
                                return false;
                            });
                    }
                    
                    var ray = new Ray2D(position, Vector2.Lerp(origin, destination, 1.0f - (ticksToImpact - 1) / (float) startingTicksToImpact));
                    Mod.ShieldManager.ImpactShield(projectile.Map, origin, ray, 1, (shield, point) =>
                    {
                        if (shield.Damage(projectile.def.projectile.damageAmountBase, point))
                        {
                            DestinationField.SetValue(projectile, Common.ToVector3(point, projectile.def.Altitude));
                            TicksToImpactField.SetValue(projectile, 0);
                            AssignedTargetField.SetValue(projectile, null);
                            IntendedTargetField.SetValue(projectile, null);
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