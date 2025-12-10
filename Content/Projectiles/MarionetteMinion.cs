using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace zhashi.Content.Projectiles
{
    public class MarionetteMinion : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.TargetDummy;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;

            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 50;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.DamageType = DamageClass.Summon;

            // 使用原版史莱姆 AI (保证跳跃能力)
            Projectile.aiStyle = 26;
            AIType = ProjectileID.BabySlime;

            // 独立无敌帧
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead) { Projectile.Kill(); return; }

            // 1. 强制修正属性 (对抗原版AI的自动重置)
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.rotation = 0f; // 禁止旋转

            // 处理朝向
            if (Projectile.velocity.X > 0.1f) Projectile.spriteDirection = -1;
            else if (Projectile.velocity.X < -0.1f) Projectile.spriteDirection = 1;

            // 粒子特效
            if (Main.rand.NextBool(20))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SilverCoin, 0, 0, 0, default, 0.8f);
                d.noGravity = true;
            }

            // 防卡住
            if (Projectile.Distance(player.Center) > 2000f)
            {
                Projectile.Center = player.Center;
                Projectile.velocity = Vector2.Zero;
            }

            // ========================================================
            // 【核心修复】手动强行检测伤害
            // 既然原版 AI 可能会关闭伤害，我们就在这里手动打人
            // ========================================================
            CheckContactDamage(player);
        }

        // 手动伤害判定函数
        private void CheckContactDamage(Player player)
        {
            // 获取自己的碰撞箱
            Rectangle myRect = Projectile.getRect();

            // 遍历所有 NPC
            foreach (NPC target in Main.ActiveNPCs)
            {
                // 如果是敌人，且接触到了，且敌人不无敌
                if (!target.friendly && !target.dontTakeDamage && target.lifeMax > 5 && myRect.Intersects(target.getRect()))
                {
                    // 检查是否处于无敌帧中 (防止一秒打60次)
                    if (target.immune[Projectile.owner] == 0 && Projectile.localNPCImmunity[target.whoAmI] == 0)
                    {
                        // 计算伤害
                        int damage = Projectile.damage;

                        // 应用伤害 (这也是 ApplyDamageToNPC 的底层逻辑)
                        target.SimpleStrikeNPC(damage, 0, Main.rand.NextBool(4), 0, DamageClass.Summon, true, player.luck);

                        // 设置无敌帧
                        target.immune[Projectile.owner] = 20;
                        Projectile.localNPCImmunity[target.whoAmI] = 20;

                        // 视觉反馈：撞击效果
                        Projectile.velocity.X *= -0.5f;
                        Projectile.velocity.Y = -4f;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, target.Center); // 打击声
                    }
                }
            }
        }

        // 墙壁碰撞微调
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.2f;
            }
            return false;
        }

        // 贴图绘制 (保持不变)
        public override bool PreDraw(ref Color lightColor)
        {
            int shaderId = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BrightSilverDye);
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.npcFrameCount[NPCID.TargetDummy];
            Rectangle sourceRect = new Rectangle(0, 0, texture.Width, frameHeight);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f + 18f);
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            GameShaders.Armor.GetSecondaryShader(shaderId, Main.LocalPlayer).Apply(null);
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, Color.White, 0f, origin, Projectile.scale, effects, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}