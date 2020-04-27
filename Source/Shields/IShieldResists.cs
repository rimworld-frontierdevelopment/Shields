using System.Collections.Generic;

namespace FrontierDevelopments.Shields
{
    public interface IShieldResists
    {
        float? Apply(ShieldDamages damages);

        float? CanResist(ShieldDamage damage);

        float CannotResist(ShieldDamage damage);

        float CannotResist(List<ShieldDamage> damages);
    }
}