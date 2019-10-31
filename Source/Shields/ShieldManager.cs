using System.Collections.Generic;
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

        private readonly HashSet<IShield> _shields = new HashSet<IShield>();

        public void Add(IShield shield)
        {
            _shields.Add(shield);
        }

        public void Del(IShield shield)
        {
            _shields.Remove(shield);
        }

        private IEnumerable<IShield> Shields => _shields;

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
                    if (point != null && !ShouldPassThrough(shield, point.Value) && shield.Block(damage, point.Value) >= damage)
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
                    if (point != null && shield.Block(damage, point.Value) >= damage)
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
                    if (point != null && shield.Block(damages, point.Value) >= damages.Damage)
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
                        && shield.Block(damage, position) >= damage)
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
                        && shield.Block(damage, position) >= damage)
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
                        && shield.Block(damages, position) >= damages.Damage)
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
    }
}