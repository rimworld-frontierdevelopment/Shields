using System.Collections.Generic;
using System.Data;
using System.Linq;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class DeployShieldGizmo : Command_Action
    {
        public DeployShieldGizmo(Pawn pawn, IList<IShield> shields)
        {
            action = delegate
            {
                if (shields.Count > 1)
                {
                    Find.WindowStack.Add(ShowSelectShield(pawn, shields));
                }
                else if (shields.Any())
                {
                    ShieldDeploymentUtility.DeployShield(pawn, shields.First());
                }
            };
        }

        private FloatMenu ShowSelectShield(Pawn pawn, IList<IShield> shields)
        {
            return new FloatMenu(
                    shields.Select(shield =>
                    {
                        return new FloatMenuOption(
                            shield.Label,
                            delegate
                            {
                                ShieldDeploymentUtility.DeployShield(pawn, shield);
                            });
                    }).ToList()
                );
        }
    }
}