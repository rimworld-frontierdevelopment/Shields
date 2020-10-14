using System.Collections.Generic;
using Verse;

namespace FrontierDevelopments.Shields
{
    public abstract class BaseShieldManager : MapComponent
    {
        protected BaseShieldManager(Map map) : base(map)
        {
        }

        public abstract void Add(IShieldField field);
        public abstract void Del(IShieldField field);
        public abstract void Add(IEnumerable<IShieldField> fields);
        public abstract void Del(IEnumerable<IShieldField> fields);
        public abstract void Add(IShield shield);
        public abstract void Del(IShield shield);
        public abstract void DrawShields(CellRect cameraRect);
        public abstract FieldQuery Query();
    }
}