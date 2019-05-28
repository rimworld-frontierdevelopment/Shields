using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ShieldManager : MapComponent
    {
        public ShieldManager(Map map) : base(map)
        {
        }

        private readonly List<IShield> _shields = new List<IShield>();
        private readonly List<Pawn> _pawns = new List<Pawn>();

        public void Add(IShield shield)
        {
            _shields.Add(shield);
        }

        public void Del(IShield shield)
        {
            _shields.Remove(shield);
        }

        public void Add(Pawn pawn)
        { 
            _pawns.Add(pawn);
        }

        public void Del(Pawn pawn)
        {
            _pawns.Remove(pawn);
        }

        private IEnumerable<IShield> Shields => _shields.Concat(InventoryShields).Concat(EquipmentShields);

        private IEnumerable<IShield> InventoryShields => _pawns.SelectMany(ShieldUtils.InventoryShields); 

        private IEnumerable<IShield> EquipmentShields => _pawns.SelectMany(ShieldUtils.EquipmentShields);
        
        public Vector3? Block(
            Vector3 origin, 
            Ray ray, 
            float limit,
            long damage)
        {
            try
            {
                foreach(var shield in Shields)
                {
                    if(shield == null || !shield.IsActive() || Mod.Settings.EnableShootingOut && shield.Collision(origin)) continue;
                    var point = shield.Collision(ray, limit);
                    if (point != null && !ShouldPassThrough(shield, point.Value) && shield.Block(damage, point.Value))
                    {
                        return point.Value;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return null;
        }

        private bool ShouldPassThrough(IShield shield, Vector3 position)
        {
            if (Mod.Settings.OverlapPassThrough)
            {
                foreach (var otherShield in Shields)
                {
                    if (shield == otherShield) continue;
                    if (otherShield.Collision(position))
                        return true;
                }
            }
            return false;
        }
        
        public Vector3? Block(
            Vector3 origin,
            Vector3 position,
            Vector3 end,
            long damage)
        {
            try
            {
                foreach(var shield in Shields)
                {
                    if(shield == null || !shield.IsActive() || ShouldPassThrough(shield, position) || Mod.Settings.EnableShootingOut && shield.Collision(origin)) continue;
                    var point = shield.Collision(position, end);
                    if (point != null && shield.Block(damage, point.Value))
                    {
                        return point.Value;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return null;
        }
        
        public Vector3? Block(
            Vector3 origin,
            Vector3 position,
            Vector3 end,
            ShieldDamages damages)
        {
            try
            {
                foreach(var shield in Shields)
                {
                    if(shield == null || !shield.IsActive() || ShouldPassThrough(shield, position) || Mod.Settings.EnableShootingOut && shield.Collision(origin)) continue;
                    var point = shield.Collision(position, end);
                    if (point != null && shield.Block(damages, point.Value))
                    {
                        return point.Value;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return null;
        }

        public bool Shielded(Vector3 start, Vector3 end, Faction friendly = null)
        {
            try
            {
                foreach(var shield in Shields)
                {
                    if(shield == null || !shield.IsActive() || Mod.Settings.EnableShootingOut && shield.Collision(start)) continue;
                    if (shield.Faction != friendly) continue;
                    var position = shield.Collision(start, end);
                    if (position != null && !ShouldPassThrough(shield, position.Value))
                    {
                        return true;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }

        public bool Shielded(Vector3 position, bool active = true)
        {
            try
            {
                foreach (var shield in Shields)
                {
                    if (active)
                    {
                        if (shield?.IsActive() == true && shield.Collision(position)) return true;
                    }
                    else
                    {
                        if (shield.Collision(position)) return true;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }
        
        public bool Block(Vector3 position, long damage)
        {
            try
            {
                foreach (var shield in Shields)
                {
                    if (shield?.IsActive() == true
                        && !ShouldPassThrough(shield, position)
                        && shield.Collision(position)
                        && shield.Block(damage, position))
                    {
                        return true;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }
        
        public bool Block(Vector3 origin, Vector3 position, long damage)
        {
            try
            {
                foreach (var shield in Shields)
                {
                    if (shield?.IsActive() == true
                        && !ShouldPassThrough(shield, position)
                        && !shield.Collision(origin)
                        && shield.Collision(position)
                        && shield.Block(damage, position))
                    {
                        return true;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }
        
        public bool Block(Vector3 origin, Vector3 position, ShieldDamages damages)
        {
            try
            {
                foreach (var shield in Shields)
                {
                    if (shield?.IsActive() == true
                        && !ShouldPassThrough(shield, position)
                        && !shield.Collision(origin)
                        && shield.Collision(position)
                        && shield.Block(damages, position))
                    {
                        return true;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }

        public void DrawShields(CellRect cameraRect)
        {
            foreach (var shield in Shields)
            {
                shield.Draw(cameraRect);
            }
        }
        
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
        static class SpawnSetup
        {
            [HarmonyPostfix]
            static void AddToShieldManager(Pawn __instance)
            {
                if(ShieldUtils.InventoryShields(__instance)
                       .Any(shield => ShieldDeploymentUtility.CanDeploy(__instance, shield)) 
                   || ShieldUtils.EquipmentShields(__instance).Any()
                   || ShieldUtils.HediffShields(__instance).Any())
                    __instance.Map.GetComponent<ShieldManager>().Add(__instance);
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.DeSpawn))]
        static class DeSpawn
        {
            [HarmonyPrefix]
            static void RemoveFromShieldManager(Pawn __instance)
            {
                __instance.Map.GetComponent<ShieldManager>().Del(__instance);
            }
        }

        [HarmonyPatch(typeof(ThingOwner), "NotifyAdded")]
        static class ThingOwnerAdded
        {
            [HarmonyPostfix]
            static void NotifyShieldAdded(ThingOwner __instance, Thing item)
            {
                switch (item)
                {
                    case MinifiedShield shield:
                        switch (__instance?.Owner)
                        {
                            case Pawn_InventoryTracker inventory:
                                var pawn = inventory.pawn;
                                if (shield.Deploy(pawn))
                                {
                                    pawn.Map.GetComponent<ShieldManager>().Add(pawn);
                                }
                                break;
                        }
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(ThingOwner), "NotifyRemoved")]
        static class ThingOwnerRemoved
        {
            [HarmonyPostfix]
            static void NotifyShieldRemoved(ThingOwner __instance, Thing item)
            {
                switch (item)
                {
                    case MinifiedShield shield:
                        switch (__instance?.Owner)
                        {
                            case Pawn_InventoryTracker inventory:
                                var pawn = inventory.pawn;
                                if (shield.Deployed)
                                {
                                    shield.Undeploy();
                                    pawn.Map.GetComponent<ShieldManager>().Del(pawn);
                                }
                                break;
                        }
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
        static class PortableShield
        {
            [HarmonyPostfix]
            static IEnumerable<Gizmo> AddDesignator(
                IEnumerable<Gizmo> __result, 
                Pawn __instance,
                List<ThingComp> ___comps)
            {
                foreach (var gizmo in __result)
                {
                    yield return gizmo;
                }

                if (Prefs.DevMode 
                    &&__instance.Faction == Faction.OfPlayer
                    && __instance.RaceProps.baseBodySize >= 2.0f)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Deploy shield",
                        defaultDesc = "Deploy a portable shield",
                        action = () =>
                        {
                            var shield = __instance.Map.listerThings
                                .ThingsOfDef(LocalDefOf.MinifiedShieldGeneratorPortable).First();
                            shield.DeSpawn();
                            __instance.inventory.innerContainer.TryAdd(shield, 1, false);
                        }
                    };
                }
            }
        }
    }
}