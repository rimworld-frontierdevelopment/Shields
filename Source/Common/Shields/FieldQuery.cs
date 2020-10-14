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
        IEnumerable<IShieldField> Get();
    }

    public interface IShieldQueryWithIntersects
    {
        IShieldQueryWithIntersects IsActive(bool isActive = true);
        IShieldQueryWithIntersects OfFaction(Faction faction, bool invert = false);
        IShieldQueryWithIntersects FriendlyTo(Faction faction, bool invert = false);
        IShieldQueryWithIntersects HostileTo(Faction faction, bool invert = false);
        IShieldQueryWithIntersects Intersects(Vector3 start, Vector3 end, bool invert = false);
        IShieldQueryWithIntersects Intersects(Vector3 position, bool invert = false);
        IEnumerable<IShieldField> Get();
        bool Block(ShieldDamages damages, Action<IShieldField, Vector3> onBlock = null);
        bool Block(float damage, Action<IShieldField, Vector3> onBlock = null);
    }

    internal static class ShieldQueryUtils
    {
        public static bool IsActive(IShieldField shield, bool isActive = true)
        {
            return shield.IsActive() == isActive;
        }

        public static bool OfFaction(IShieldField shield, Faction faction, bool invert = false)
        {
            return shield.Faction == faction != invert;
        }

        public static bool FriendlyTo(IShieldField shield, Faction faction, bool invert = false)
        {
            return (shield.Faction == faction ||
                    shield.Faction.RelationKindWith(faction) == FactionRelationKind.Ally)
                   != invert;
        }

        public static bool HostileTo(IShieldField shield, Faction faction, bool invert = false)
        {
            return (shield.Faction != faction &&
                    shield.Faction.RelationKindWith(faction) == FactionRelationKind.Hostile)
                   != invert;
        }

        public static IShieldQueryWithIntersects Intersects(IEnumerable<IShieldField> shields, Vector3 start, Vector3 end, Map map, bool invert = false)
        {
            return new ShieldQueryWithIntersects(shields
                .Select(shield => new ShieldQueryWithIntersects.ShieldWithIntersects
                    {
                        Shield = shield,
                        Intersect = shield.Collision(start, end)
                    })
                .Where(elements => elements.Intersect != null != invert), map);
        }

        public static IShieldQueryWithIntersects Intersects(IEnumerable<IShieldField> shields, Vector3 position, Map map, bool invert = false)
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
                .Where(elements => elements.Intersect != null != invert), map);
        }
    }

    public class FieldQuery : IShieldQuery
    {
        private readonly Map _map;
        private readonly IEnumerable<IShieldField> _shields;

        public FieldQuery(IEnumerable<IShieldField> shields, Map map)
        {
            _map = map;
            _shields = shields.Where(shield => shield.PresentOnMap(map));
        }

        public FieldQuery(Map map)
        {
            _map = map;
            _shields = ShieldManager.For(map).Fields.Where(shield => shield.PresentOnMap(map));
        }
            
        public IShieldQuery IsActive(bool isActive = true)
        {
            return new FieldQuery(_shields.Where(shield => ShieldQueryUtils.IsActive(shield, isActive)), _map);
        }

        public IShieldQuery OfFaction(Faction faction, bool invert = false)
        {
            return new FieldQuery(_shields.Where(shield => ShieldQueryUtils.OfFaction(shield, faction, invert)), _map);
        }

        public IShieldQuery FriendlyTo(Faction faction, bool invert = false)
        {
            return new FieldQuery(_shields.Where(shield => ShieldQueryUtils.FriendlyTo(shield, faction, invert)), _map);
        }

        public IShieldQuery HostileTo(Faction faction, bool invert = false)
        {
            return new FieldQuery(_shields.Where(shield => ShieldQueryUtils.HostileTo(shield, faction, invert)), _map);
        }

        public IShieldQueryWithIntersects Intersects(Vector3 start, Vector3 end, bool invert = false)
        {
            return ShieldQueryUtils.Intersects(_shields, start, end, _map, invert);
        }
        
        public IShieldQueryWithIntersects Intersects(Vector3 position, bool invert = false)
        {
            return ShieldQueryUtils.Intersects(_shields, position, _map, invert);
        }

        public IEnumerable<IShieldField> Get()
        {
            return _shields;
        }
    }

    public class ShieldQueryWithIntersects : IShieldQueryWithIntersects
    {
        public struct ShieldWithIntersects
        { 
            public IShieldField Shield;
            public Vector3? Intersect;
        }

        private readonly Map _map;
        private readonly IEnumerable<ShieldWithIntersects> _elements;

        private IEnumerable<IShieldField> Shields => _elements.Select(e => e.Shield);

        public ShieldQueryWithIntersects(IEnumerable<ShieldWithIntersects> elements, Map map)
        {
            _map = map;
            _elements = elements;
        }
        
        public IShieldQueryWithIntersects IsActive(bool isActive = true)
        {
            return new ShieldQueryWithIntersects(
                _elements.Where(e => ShieldQueryUtils.IsActive(e.Shield, isActive)), _map);
        }

        public IShieldQueryWithIntersects OfFaction(Faction faction, bool invert = false)
        {
            return new ShieldQueryWithIntersects(
                _elements.Where(e => ShieldQueryUtils.OfFaction(e.Shield, faction, invert)), _map);
        }

        public IShieldQueryWithIntersects FriendlyTo(Faction faction, bool invert = false)
        {
            return new ShieldQueryWithIntersects(
                _elements.Where(e => ShieldQueryUtils.FriendlyTo(e.Shield, faction, invert)), _map);
        }

        public IShieldQueryWithIntersects HostileTo(Faction faction, bool invert = false)
        {
            return new ShieldQueryWithIntersects(
                _elements.Where(e => ShieldQueryUtils.HostileTo(e.Shield, faction, invert)), _map);
        }

        public IShieldQueryWithIntersects Intersects(Vector3 start, Vector3 end, bool invert = false)
        {
            return ShieldQueryUtils.Intersects(Shields, start, end, _map, invert);
        }

        public IShieldQueryWithIntersects Intersects(Vector3 position, bool invert = false)
        {
            return ShieldQueryUtils.Intersects(Shields, position, _map, invert);
        }

        public IEnumerable<IShieldField> Get()
        {
            return _elements.Select(e => e.Shield);
        }

        public bool Block(ShieldDamages damages, Action<IShieldField, Vector3> onBlock)
        {
            try
            {
                var result = _elements
                    .Where(e => e.Intersect != null)
                    .First(e => e.Shield.Block(damages, e.Intersect.Value) >= damages.Damage);
                if (result.Intersect != null)
                {
                    onBlock?.Invoke(result.Shield, result.Intersect.Value);
                    return true;
                }
            }
            catch(InvalidOperationException) {}
            return false;
        }

        public bool Block(float damage, Action<IShieldField, Vector3> onBlock)
        {
            try
            {
                var result = _elements
                    .Where(e => e.Intersect != null)
                    .First(e => e.Shield.Block(damage, e.Intersect.Value) >= damage);
                if (result.Intersect != null)
                {
                    onBlock?.Invoke(result.Shield, result.Intersect.Value);
                    return true;
                }
            }
            catch(InvalidOperationException) {}
            return false;
        }
    }
}