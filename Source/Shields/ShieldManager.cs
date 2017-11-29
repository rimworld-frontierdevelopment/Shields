using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ShieldManager
    {
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

        public bool ImpactShield(Map map, Vector2 origin, Ray2D ray, float limit, Func<IShield, Vector2, bool> onColission)
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

        public bool ImpactShield(Map map, Vector2 position, Func<IShield, Vector2, bool> onColission)
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
        
        public bool ImpactShield(Map map, Vector2 position, Vector2 origin, Vector2 destination, Func<IShield, Vector2, bool> onColission)
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