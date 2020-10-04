using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ShieldManager : MapComponent
    {
        public ShieldManager(Map map) : base(map)
        {
        }

        private readonly HashSet<IShieldManageable> _shields = new HashSet<IShieldManageable>();

        public void Add(IShieldManageable shield)
        {
            _shields.Add(shield);
        }

        public void Del(IShieldManageable shield)
        {
            _shields.Remove(shield);
        }

        public IEnumerable<IShieldManageable> Shields => _shields;

        public void DrawShields(CellRect cameraRect)
        {
            var shields = Shields.ToList();
            
            foreach (var shield in shields)
            {
                shield.FieldPreDraw();
            }

            foreach (var shield in shields)
            {
                shield.FieldDraw(cameraRect);
            }

            foreach (var shield in shields)
            {
                shield.FieldPostDraw();
            }
        }

        public ShieldQuery Query()
        {
            return new ShieldQuery(_shields);
        }
    }
}
