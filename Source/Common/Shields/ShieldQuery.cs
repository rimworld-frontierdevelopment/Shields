using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields
{
    public interface IShieldQuery
    {
        IShieldQuery IsActive(bool isActive = true);
        IShieldQuery OfFaction(Faction faction, bool invert = false);
        IShieldQuery FriendlyTo(Faction faction, bool invert = false);
        IShieldQuery HostileTo(Faction faction, bool invert = false);
        IShieldQueryWithIntersects Intersects(Vector3 start, Vector3 end, bool invert = false);
        IShieldQueryWithIntersects Intersects(Vector3 position, bool invert = false);
        IEnumerable<IShield> Get();
    }

    public interface IShieldQueryWithIntersects
    {
        IShieldQueryWithIntersects IsActive(bool isActive = true);
        IShieldQueryWithIntersects OfFaction(Faction faction, bool invert = false);
        IShieldQueryWithIntersects FriendlyTo(Faction faction, bool invert = false);
        IShieldQueryWithIntersects HostileTo(Faction faction, bool invert = false);
        IShieldQueryWithIntersects Intersects(Vector3 start, Vector3 end, bool invert = false);
        IShieldQueryWithIntersects Intersects(Vector3 position, bool invert = false);
        IEnumerable<IShield> Get();
        Vector3? Block(ShieldDamages damages);
        Vector3? Block(float damage);
    }

    internal static class ShieldQueryUtils
    {
        public static bool IsActive(IShield shield, bool isActive = true)
        {
            return shield.IsActive() == isActive;
        }

        public static bool OfFaction(IShield shield, Faction faction, bool invert = false)
        {
            return shield.Faction == faction != invert;
        }

        public static bool FriendlyTo(IShield shield, Faction faction, bool invert = false)
        {
            return (shield.Faction == faction ||
                    shield.Faction.RelationKindWith(faction) == FactionRelationKind.Ally)
                   != invert;
        }

        public static bool HostileTo(IShield shield, Faction faction, bool invert = false)
        {
            return (shield.Faction != faction &&
                    shield.Faction.RelationKindWith(faction) == FactionRelationKind.Hostile)
                   != invert;
        }

        public static IShieldQueryWithIntersects Intersects(IEnumerable<IShield> shields, Vector3 start, Vector3 end, bool invert = false)
        {
            return new ShieldQueryWithIntersects(shields
                .Select(shield => new ShieldQueryWithIntersects.ShieldWithIntersects
                    {
                        Shield = shield,
                        Intersect = shield.Collision(start, end)
                    })
                .Where(elements => elements.Intersect != null != invert));
        }

        public static IShieldQueryWithIntersects Intersects(IEnumerable<IShield> shields, Vector3 position, bool invert = false)
        {
            return new ShieldQueryWithIntersects(shields
                .Select(shield =>
                {
                    Vector3? point = null;
                    if (shield.Collision(position))
                    {
                        point = position;
                    }
                    return new ShieldQueryWithIntersects.ShieldWithIntersects
                    {
                        Shield = shield,
                        Intersect = point
                    };
                })
                .Where(elements => elements.Intersect != null != invert));
        }
    }

    public class ShieldQuery : IShieldQuery
    {
        private readonly IEnumerable<IShield> _shields;

        public ShieldQuery(IEnumerable<IShield> shields)
        {
            _shields = shields;
        }

        public ShieldQuery(Map map)
        {
            _shields = map.GetComponent<ShieldManager>().Shields;
        }
            
        public IShieldQuery IsActive(bool isActive = true)
        {
            return new ShieldQuery(_shields.Where(shield => ShieldQueryUtils.IsActive(shield, isActive)));
        }

        public IShieldQuery OfFaction(Faction faction, bool invert = false)
        {
            return new ShieldQuery(_shields.Where(shield => ShieldQueryUtils.OfFaction(shield, faction, invert)));
        }

        public IShieldQuery FriendlyTo(Faction faction, bool invert = false)
        {
            return new ShieldQuery(_shields.Where(shield => ShieldQueryUtils.FriendlyTo(shield, faction, invert)));
        }

        public IShieldQuery HostileTo(Faction faction, bool invert = false)
        {
            return new ShieldQuery(_shields.Where(shield => ShieldQueryUtils.HostileTo(shield, faction, invert)));
        }

        public IShieldQueryWithIntersects Intersects(Vector3 start, Vector3 end, bool invert = false)
        {
            return ShieldQueryUtils.Intersects(_shields, start, end, invert);
        }
        
        public IShieldQueryWithIntersects Intersects(Vector3 position, bool invert = false)
        {
            return ShieldQueryUtils.Intersects(_shields, position, invert);
        }

        public IEnumerable<IShield> Get()
        {
            return _shields;
        }
    }

    public class ShieldQueryWithIntersects : IShieldQueryWithIntersects
    {
        public struct ShieldWithIntersects
        { 
            public IShield Shield;
            public Vector3? Intersect;
        }

        private readonly IEnumerable<ShieldWithIntersects> _elements;

        private IEnumerable<IShield> Shields => _elements.Select(e => e.Shield);

        public ShieldQueryWithIntersects(IEnumerable<ShieldWithIntersects> elements)
        {
            _elements = elements;
        }
        
        public IShieldQueryWithIntersects IsActive(bool isActive = true)
        {
            return new ShieldQueryWithIntersects(
                _elements.Where(e => ShieldQueryUtils.IsActive(e.Shield, isActive)));
        }

        public IShieldQueryWithIntersects OfFaction(Faction faction, bool invert = false)
        {
            return new ShieldQueryWithIntersects(
                _elements.Where(e => ShieldQueryUtils.OfFaction(e.Shield, faction, invert)));
        }

        public IShieldQueryWithIntersects FriendlyTo(Faction faction, bool invert = false)
        {
            return new ShieldQueryWithIntersects(
                _elements.Where(e => ShieldQueryUtils.FriendlyTo(e.Shield, faction, invert)));
        }

        public IShieldQueryWithIntersects HostileTo(Faction faction, bool invert = false)
        {
            return new ShieldQueryWithIntersects(
                _elements.Where(e => ShieldQueryUtils.HostileTo(e.Shield, faction, invert)));
        }

        public IShieldQueryWithIntersects Intersects(Vector3 start, Vector3 end, bool invert = false)
        {
            return ShieldQueryUtils.Intersects(Shields, start, end, invert);
        }

        public IShieldQueryWithIntersects Intersects(Vector3 position, bool invert = false)
        {
            return ShieldQueryUtils.Intersects(Shields, position, invert);
        }

        public IEnumerable<IShield> Get()
        {
            return _elements.Select(e => e.Shield);
        }

        public Vector3? Block(ShieldDamages damages)
        {
            try
            {
                return _elements
                    .Where(e => e.Intersect != null)
                    .Where(e => e.Shield.Block(damages, e.Intersect.Value) >= damages.Damage)
                    .Select(e => e.Intersect)
                    .First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public Vector3? Block(float damage)
        {
            try
            {
                return _elements
                    .Where(e => e.Intersect != null)
                    .Where(e => e.Shield.Block(damage, e.Intersect.Value) >= damage)
                    .Select(e => e.Intersect)
                    .First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}