using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Shields.Linear;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class ShieldManager : MapComponent
    {
        public IEnumerable<IShield> Shields => _shields;
        
        public IEnumerable<IShieldField> Fields
        {
            get
            {
                foreach (var field in _fields)
                    yield return field;
                foreach (var link in _linearLinks)
                    yield return link;
            }
        }
        
        public ShieldManager(Map map) : base(map)
        {
        }

        private readonly HashSet<IShieldField> _fields = new HashSet<IShieldField>();
        private HashSet<LinearShieldLink> _linearLinks = new HashSet<LinearShieldLink>();
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
            switch (field)
            {
                case LinearShieldLink link:
                    _linearLinks.Add(link);
                    break;
                default:
                    _fields.Add(field);
                    break;
            }
        }

        public void Del(IShieldField field)
        {
            switch (field)
            {
                case LinearShieldLink link:
                    _linearLinks.Remove(link);
                    break;
                default:
                    _fields.Remove(field);
                    break;
            }
        }

        public void Add(IEnumerable<IShieldField> fields)
        {
            fields.Do(Add);
        }

        public void Del(IEnumerable<IShieldField> fields)
        {
            fields.Do(Del);
        }

        public void Add(IShield shield)
        {
            _shields.Add(shield);
        }

        public void Del(IShield shield)
        {
            _shields.Remove(shield);
        }

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

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref _linearLinks, "linearLinks", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if(_linearLinks == null)
                    _linearLinks = new HashSet<LinearShieldLink>();
            }
        }

        public override void MapComponentTick()
        {
            _linearLinks.Do(link => link.Tick());
        }
    }
}
