using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields
{
    public static class ExplosionUtility
    {
        public static void DoSmokeExplosion(Thing projectile, SoundDef explosionSound)
        {
            DoSmokeExplosion(projectile, projectile.Position, projectile.Map, explosionSound);
        }
        
        public static void DoSmokeExplosion(Thing instigator, IntVec3 position, Map map, SoundDef explosionSound)
        {
            GenExplosion.DoExplosion(
                position,
                map,
                1,
                DamageDefOf.Smoke,
                instigator,
                explosionSound: explosionSound);
        }
    }
}