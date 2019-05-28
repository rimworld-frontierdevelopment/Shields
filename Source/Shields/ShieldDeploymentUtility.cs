using Verse;

namespace FrontierDevelopments.Shields
{
    public static class ShieldDeploymentUtility
    {
        public static bool CanDeploy(Pawn pawn, MinifiedShield shield)
        {
            return pawn.RaceProps.baseBodySize > 2.0f;
        }

        public static bool CanDeploy(Pawn pawn, IShield shield)
        {
            switch (shield)
            {
                case MinifiedShield minifiedShield:
                    return CanDeploy(pawn, minifiedShield);
            }
            return false;
        }
    }
}