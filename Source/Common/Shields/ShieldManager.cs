using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ShieldManager : MapComponent
    {
        public ShieldManager(Map map) : base(map)
        {
        }

        private readonly HashSet<IShieldField> _fields = new HashSet<IShieldField>();

        public void Add(IShieldField field)
        {
            _fields.Add(field);
        }

        public void Del(IShieldField field)
        {
            _fields.Remove(field);
        }
        
        public void Add(IEnumerable<IShieldField> fields)
        {
            _fields.AddRange(fields);
        }
        
        public void Del(IEnumerable<IShieldField> fields)
        {
            fields.Do(field => _fields.Remove(field));
        }

        public IEnumerable<IShieldField> Fields => _fields;

        public void DrawShields(CellRect cameraRect)
        {
            var fields = Fields.ToList();
            
            foreach (var field in fields)
            {
                field.FieldPreDraw();
            }

            foreach (var field in fields)
            {
                field.FieldDraw(cameraRect);
            }

            foreach (var field in fields)
            {
                field.FieldPostDraw();
            }
        }

        public FieldQuery Query()
        {
            return new FieldQuery(_fields);
        }
    }
}
