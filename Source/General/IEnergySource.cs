using Verse;

namespace FrontierDevelopments.General
{
    public interface IEnergySource
    {
        bool IsActive();
        bool WantActive { get; }
        float BaseConsumption { get; set; }
        float EnergyAvailable { get; }
        float Draw(float amount);
        void Drain(float amount);
    }

    public class EnergySourceUtility
    {
        public static IEnergySource Find(ThingWithComps parent)
        {
            switch (parent)
            {
                case IEnergySource parentSource:
                    return parentSource;
                default:
                    foreach (var comp in parent.AllComps)
                    {
                        switch (comp)
                        {
                            case IEnergySource compSource:
                                return compSource;
                        }
                    }
                    break;
            }

            return null;
        }
    }   
}
