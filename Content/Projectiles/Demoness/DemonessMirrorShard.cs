using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace zhashi.Content.Projectiles.Demoness
{
    public class DemonessMirrorShard : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("魔女之镜碎片");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2; // 可以穿透1个敌人
            Projectile.timeLeft = 120; // 2秒后消失
            Projectile.aiStyle = -1;  // 自定义AI
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            // 1. 重力效果
            Projectile.velocity.Y += 0.2f;

            // 2. 旋转
            Projectile.rotation += 0.2f * Projectile.direction;

            // 3. 视觉：生成玻璃碎片粒子
            if (Main.rand.NextBool(3))
            {
                // 使用玻璃粒子 (Glass) 和 黑色粒子 (Shadowflame) 混合
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, 0, 0, 100, default, 1f);
                d.noGravity = true;
            }
            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0, 0, 100, default, 0.8f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中特效：玻璃破碎声 + 黑色火焰
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);

            target.AddBuff(BuffID.ShadowFlame, 120); // 附带2秒暗影焰

            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, 0, 0, 0, default, 1.2f);
            }
        }

        // 撞墙反弹一次
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0) return true;

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            // 简单的反弹逻辑
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.6f; // 反弹并减速
            }
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.6f;
            }
            return false;
        }
    }
}