using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace zhashi.Content.Projectiles.Demoness
{
    public class DemonessSpiderSilk : ModProjectile
    {
        // 【核心修改】这里直接指定使用原版"蜘蛛网唾液"的贴图
        // 这样你就不用自己做 DemonessSpiderSilk.png 了
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WebSpit;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // 只有一半的几率产生粉色或白色的丝线粒子
            if (Main.rand.NextBool(2))
            {
                // DustID.Web 是白色，DustID.PinkSlime 是粉色
                int dustType = Main.rand.NextBool() ? DustID.Web : DustID.PinkSlime;
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, Vector2.Zero, 100, default, 0.8f);
                d.noGravity = true;
            }

            // 追踪最近的敌人
            NPC closestNPC = FindClosestNPC(500f);
            if (closestNPC != null)
            {
                Vector2 direction = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                // 追踪速度
                Projectile.velocity = (Projectile.velocity * 25f + direction * 18f) / 26f;
            }
        }

        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC target = Main.npc[k];
                if (target.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }
            return closestNPC;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.boss) target.AddBuff(BuffID.Webbed, 120);
            else target.AddBuff(BuffID.Slow, 180);

            for (int i = 0; i < 10; i++)
                Dust.NewDust(target.position, target.width, target.height, DustID.Web, 0, 0, 0, default, 1.2f);
        }
    }
}