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
        public static IHeatsink Find(ThingWithComps parent)
        {
            switch (parent)
            {
                case IHeatsink parentHeatsink:
                    return parentHeatsink;
                default:
                    return FindComp(parent);
            }
        }

        public static IHeatsink FindComp(ThingWithComps parent)
        {
            foreach (var comp in parent.AllComps)
            {
                switch (comp)
                {
                    case IHeatsink compHeatSink:
                        return compHeatSink;
                }
            }
            return null;
        }
    }
}