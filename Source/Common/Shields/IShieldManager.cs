using System.Collections;
using System.Collections.Generic;
using Verse;

namespace FrontierDevelopments.Shields
{
    public interface IShieldManager
    {
        IEnumerable<Map> AssociatedMaps { get; }
        void AssociateWithMap(Map map);
        IEnumerable<IShieldField> Fields { get; }
        HashSet<IShield> AllEmitters { get; }
        void Add(IShieldField field);
        void Del(IShieldField field);
        void Add(IEnumerable<IShieldField> fields);
        void Del(IEnumerable<IShieldField> fields);
        void Add(IShield shield);
        void Del(IShield shield);
        IEnumerable<Map> PresentOnMaps(IShield shield);
        IEnumerable<Map> PresentOnMaps(IShieldField fhield);
        void DrawShields(CellRect cameraRect, Map map);
        FieldQuery Query(Map map);
        void Tick();
    }
}