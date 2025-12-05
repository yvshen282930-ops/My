using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace zhashi.Content.Projectiles
{
    // 这是从天上掉下来的那把光剑
    public class DawnBladeProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 60;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.light = 1.2f; // 更亮
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            DrawOffsetX = 0;
            DrawOriginOffsetY = 0;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.4f; // 加速更快
            if (Projectile.velocity.Y > 20f) Projectile.velocity.Y = 20f; // 最大速度更快

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 更密集的拖尾
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), DustID.GoldFlame, -Projectile.velocity * 0.1f, 100, default, 1.8f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 更有力的爆炸声
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.position); // 额外的轰鸣声

            // === 升级版华丽爆炸 ===

            // 1. 向四周扩散的冲击波圈
            for (int i = 0; i < 30; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(10f, 10f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold, speed, 0, default, 2.5f);
                d.noGravity = true;
                // 让粒子沿切线方向拉伸，形成“光刃切开空气”的感觉
                d.velocity *= 1.2f;
            }

            // 2. 向上喷发的能量柱
            for (int i = 0; i < 20; i++)
            {
                Vector2 speed = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-5f, -12f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, speed, 100, default, 3f);
                d.noGravity = true;
            }

            // 3. 屏幕震动 (轻微)
            // 在多人模式下需要小心使用，这里简单加一个本地效果
            if (Main.myPlayer == Projectile.owner)
            {
                Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(Projectile.Center, new Vector2(0, 1), 0.5f, 6f, 10, 1000f));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240); // 升级为更强的“狱火”Buff (OnFire3)，持续4秒
        }
    }
}