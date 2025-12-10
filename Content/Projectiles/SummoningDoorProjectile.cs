using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent; // 必须引用：用于获取原版资源
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Buffs;

namespace zhashi.Content.Projectiles
{
    public class SummoningDoorProjectile : ModProjectile
    {
        // 【修复】原版月亮传送门 ID 是 MoonlordTurret (642)
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MoonlordTurret;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.scale = 0.1f;
        }

        public override void AI()
        {
            // 贴图动画
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            // 淡入与缩放
            if (Projectile.alpha > 0) Projectile.alpha -= 10;
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1.5f, 0.1f);
            Projectile.rotation += 0.02f;

            // 氛围粒子
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(40, 40);
                int dustID = Main.rand.NextBool() ? DustID.BlueCrystalShard : DustID.DungeonSpirit;
                Dust d = Dust.NewDustPerfect(dustPos, dustID, Vector2.Zero, 150, default, 1.5f);
                d.noGravity = true;
                d.velocity *= 0.5f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                SpawnSummon();
            }
        }

        private void SpawnSummon()
        {
            Vector2 spawnPos = Projectile.Center;
            SoundEngine.PlaySound(SoundID.Item119, spawnPos);

            int roll = Main.rand.Next(100); // 0 ~ 99

            // =================================================
            // 【新增】1% 概率召唤 BOSS (月亮领主)
            // =================================================
            if (roll < 1)
            {
                Main.NewText("【警告】古老的神祇透过门扉注视着你...", 255, 0, 0); // 红色警告
                SoundEngine.PlaySound(SoundID.Roar, spawnPos);

                // 召唤月亮领主头部 (你可以改成 NPCID.DukeFishron 猪鲨，或者其他)
                NPC.NewNPC(Projectile.GetSource_FromThis(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.MoonLordHead);

                // 大量特效
                for (int i = 0; i < 100; i++)
                    Dust.NewDust(spawnPos, 100, 100, DustID.LunarOre, 0, 0, 0, default, 3f);
            }
            // 2. 灵界异变 (5% 概率, roll 1~5)
            else if (roll < 6)
            {
                Main.NewText("警告：召唤仪式发生异变！", 255, 100, 100);
                SoundEngine.PlaySound(SoundID.Roar, spawnPos);
                NPC.NewNPC(Projectile.GetSource_FromThis(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.Mothron);
                for (int i = 0; i < 50; i++)
                    Dust.NewDust(spawnPos, 100, 100, DustID.Blood, 0, 0, 0, default, 2f);
            }
            // 3. 强力契约 (14% 概率, roll 6~19)
            else if (roll < 20)
            {
                Main.NewText("契约达成：高位灵界生物响应了召唤。", 255, 215, 0);
                int type = NPCID.WyvernHead;
                int npcId = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)spawnPos.X, (int)spawnPos.Y, type);

                if (npcId != Main.maxNPCs)
                {
                    NPC summon = Main.npc[npcId];
                    summon.AddBuff(ModContent.BuffType<TamedBuff>(), 36000); // 驯服
                    summon.damage = (int)(summon.damage * 3f);
                    summon.lifeMax = (int)(summon.lifeMax * 3f);
                    summon.life = summon.lifeMax;
                    summon.defense += 50;
                }
            }
            // 4. 正常召唤 (80% 概率)
            else
            {
                Main.NewText("召唤成功。", 150, 150, 255);
                int[] summonPool = { NPCID.Paladin, NPCID.RedDevil, NPCID.Necromancer, NPCID.GiantTortoise, NPCID.DiabolistRed };
                int type = summonPool[Main.rand.Next(summonPool.Length)];
                int npcId = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)spawnPos.X, (int)spawnPos.Y, type);

                if (npcId != Main.maxNPCs)
                {
                    NPC summon = Main.npc[npcId];
                    summon.AddBuff(ModContent.BuffType<TamedBuff>(), 18000); // 驯服
                    summon.damage = (int)(summon.damage * 1.5f);
                    summon.lifeMax = (int)(summon.lifeMax * 1.5f);
                    summon.life = summon.lifeMax;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取原版月亮传送门贴图
            Texture2D texture = TextureAssets.Projectile[ProjectileID.MoonlordTurret].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = sourceRect.Size() / 2f;

            // 绘制主体
            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                sourceRect,
                Color.White * 0.9f,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // 绘制发光层
            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                sourceRect,
                new Color(100, 255, 255, 0) * 0.5f,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.1f,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}