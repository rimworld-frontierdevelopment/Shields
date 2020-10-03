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

        private readonly HashSet<IShield> _shields = new HashSet<IShield>();

        public void Add(IShield shield)
        {
            _shields.Add(shield);
        }

        public void Del(IShield shield)
        {
            _shields.Remove(shield);
        }

        public IEnumerable<IShield> Shields => _shields;

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
