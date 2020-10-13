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
        private readonly HashSet<IShield> _shields = new HashSet<IShield>();

        public HashSet<IShield> AllEmitters
        {
            get
            {
                IEnumerable<IShield> Union()
                {
                    foreach (var shield in _shields)
                    {
                        yield return shield;
                    }

                    foreach (var shield in _fields.SelectMany(field => field.Emitters))
                    {
                        yield return shield;
                    }
                }

                return Union().ToHashSet();
            }
        }

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

        public void Add(IShield shield)
        {
            _shields.Add(shield);
        }

        public void Del(IShield shield)
        {
            _shields.Remove(shield);
        }

        public IEnumerable<IShieldField> Fields => _fields;
        public IEnumerable<IShield> Shields => _shields;

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
            return new FieldQuery(Fields);
        }
    }
}
