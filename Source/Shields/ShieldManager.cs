using System.Collections.Generic;
using System.Linq;
using Harmony;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ShieldManager
    {
        [HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")]
        static class ShieldManager_SavedGameLoader_LoadGameFromSaveFile
        {
            static void Prefix()
            {
                Mod.ShieldManager._shieldsMap.Clear();
            }
        }

        private readonly Dictionary<int, List<IShield>> _shieldsMap = new Dictionary<int, List<IShield>>();
        
        public IEnumerable<IShield> Shields(Map map)
        {
            try
            {
                return _shieldsMap[map.uniqueID];
            }
            catch (KeyNotFoundException)
            {
                return Enumerable.Empty<IShield>();
            }
        }

        public void Add(Map map, IShield shield)
        {
            List<IShield> shields;
            try
            {
                shields = _shieldsMap[map.uniqueID];
            }
            catch (KeyNotFoundException)
            {
                shields = new List<IShield>();
                _shieldsMap.Add(map.uniqueID, shields);
            }
            shields.Add(shield);
        }

        public void Del(Map map, IShield shield)
        {
            try
            {
                _shieldsMap[map.uniqueID].Remove(shield);
            }
            catch (KeyNotFoundException) {}
        }

        public Vector3? Block(
            Map map, 
            Vector3 origin, 
            Ray ray, 
            float limit,
            int damage)
        {
            try
            {
                foreach(var shield in _shieldsMap[map.uniqueID])
                {
                    if(shield == null || !shield.IsActive() || Mod.Settings.EnableShootingOut && shield.Collision(origin)) continue;
                    var point = shield.Collision(ray, limit);
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
            Map map,
            Vector3 origin,
            Vector3 position,
            Vector3 end,
            int damage)
        {
            try
            {
                foreach(var shield in _shieldsMap[map.uniqueID])
                {
                    if(shield == null || !shield.IsActive() || Mod.Settings.EnableShootingOut && shield.Collision(origin)) continue;
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

        public bool Shielded(Map map, Vector3 start, Vector3 end)
        {
            try
            {
                foreach(var shield in _shieldsMap[map.uniqueID])
                {
                    if(shield == null || !shield.IsActive() || Mod.Settings.EnableShootingOut && shield.Collision(start)) continue;
                    if (shield.Collision(start, end) != null)
                    {
                        return true;
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }

        public bool Shielded(Map map, Vector3 position, bool active = true)
        {
            try
            {
                foreach (var shield in _shieldsMap[map.uniqueID])
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
        
        public bool Block(Map map, Vector3 position, int damage)
        {
            try
            {
                foreach (var shield in _shieldsMap[map.uniqueID])
                {
                    if (shield?.IsActive() == true
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
        
        public bool Block(Map map, Vector3 position, Vector3 origin, int damage)
        {
            try
            {
                foreach (var shield in _shieldsMap[map.uniqueID])
                {
                    if (shield?.IsActive() == true
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
    }
}