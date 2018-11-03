using System.Collections.Generic;
using System.Linq;
using Harmony;
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

        public void Add(IShield shield)
        {
            _shields.Add(shield);
        }

        public void Del(IShield shield)
        {
            _shields.Remove(shield);
        }

        public Vector3? Block(
            Vector3 origin, 
            Ray ray, 
            float limit,
            int damage)
        {
            try
            {
                foreach(var shield in _shields)
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
                foreach (var otherShield in _shields)
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
            int damage)
        {
            try
            {
                foreach(var shield in _shields)
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

        public bool Shielded(Vector3 start, Vector3 end)
        {
            try
            {
                foreach(var shield in _shields)
                {
                    if(shield == null || !shield.IsActive() || Mod.Settings.EnableShootingOut && shield.Collision(start)) continue;
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
                foreach (var shield in _shields)
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
        
        public bool Block(Vector3 position, int damage)
        {
            try
            {
                foreach (var shield in _shields)
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
        
        public bool Block(Vector3 position, Vector3 origin, int damage)
        {
            try
            {
                foreach (var shield in _shields)
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
    }
}