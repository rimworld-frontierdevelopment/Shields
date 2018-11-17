using Verse;

namespace FrontierDevelopments.General
{
    public interface IHeatsink
    {
        bool OverTemperature { get; }
        float Temp { get; }
        void PushHeat(float wattDays);
    }

    public class HeatsinkUtility
    {
        public static IHeatsink FindHeatsink(ThingWithComps parent)
        {
            switch (parent)
            {
                case IHeatsink parentHeatsink:
                    return parentHeatsink;
                default:
                    foreach (var comp in parent.AllComps)
                    {
                        switch (comp)
                        {
                            case IHeatsink compHeatsink:
                                return compHeatsink;
                        }
                    }
                    break;
            }

            return null;
        }
    }
}