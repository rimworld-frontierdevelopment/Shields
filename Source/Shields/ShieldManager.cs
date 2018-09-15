using System;
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

        public bool ImpactShield(Map map, Vector3 origin, Ray ray, float limit, Func<IShield, Vector3, bool> onColission)
        {
            try
            {
                foreach(var shield in _shieldsMap[map.uniqueID])
                {
                    if (shield?.IsActive() == true && !shield.Collision(origin))
                    {
                        var point = shield.Collision(ray, limit);
                        if (point != null)
                        {
                            if (onColission(shield, point.Value)) return true;
                        }
                    }
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }

        public bool ImpactShield(Map map, Vector3 position)
        {
            try
            {
                foreach (var shield in _shieldsMap[map.uniqueID])
                {
                    if (shield?.IsActive() == true && shield.Collision(position)) return true;
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }
        
        public bool ImpactShield(Map map, Vector3 position, Func<IShield, Vector3, bool> onColission)
        {
            try
            {
                foreach (var shield in _shieldsMap[map.uniqueID])
                {
                    if (shield?.IsActive() == true
                        && shield.Collision(position)
                        && onColission(shield, position)) return true;
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }
        
        public bool ImpactShield(Map map, Vector3 position, Vector3 origin, Vector3 destination, Func<IShield, Vector3, bool> onColission)
        {
            try
            {
                foreach (var shield in _shieldsMap[map.uniqueID])
                {
                    if (shield?.IsActive() == true
                        && !shield.Collision(origin)
                        && shield.Collision(destination)
                        && shield.Collision(position)
                        && onColission(shield, position)) return true;
                }
            }
            catch (KeyNotFoundException) {}
            return false;
        }
    }
}