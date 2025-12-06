using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Buffs;

namespace zhashi.Content.Projectiles
{
    public class SummoningDoorProjectile : ModProjectile
    {
        // 确保你的贴图路径正确：Content/Projectiles/SummoningDoorProjectile.png
        public override string Texture => "zhashi/Content/Projectiles/SummoningDoorProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 80;  // 根据你的贴图大小适当调整，建议设为贴图宽高的一半左右
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120; // 门存在 2 秒，播放完旋转动画后召唤
            Projectile.alpha = 255;    // 初始全透明
            Projectile.penetrate = -1;
            Projectile.scale = 0.1f;   // 初始极小，模拟从虚空拉伸出来
        }

        public override void AI()
        {
            // --- 1. 动画效果 ---

            // 淡入：alpha 越小越不透明
            if (Projectile.alpha > 0)
                Projectile.alpha -= 10;

            // 缩放：平滑变大到 1.2 倍
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1.2f, 0.1f);

            // 【核心需求】缓缓旋转
            // 0.02f 是旋转速度，数值越大转得越快
            Projectile.rotation += 0.02f;

            // --- 2. 氛围粒子 ---
            if (Main.rand.NextBool(3))
            {
                // 生成紫色/神秘粒子围绕门旋转
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(40, 40);
                int dustID = Main.rand.NextBool() ? DustID.PurpleCrystalShard : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(dustPos, dustID, Vector2.Zero, 150, default, 1.5f);
                d.noGravity = true;
                d.velocity *= 0.5f; // 缓慢飘动
            }
        }

        // 当弹幕时间结束 (timeLeft = 0) 时触发召唤
        public override void OnKill(int timeLeft)
        {
            // 确保只在服务端或单人模式生成 NPC，且只有自然消失才召唤
            if (Projectile.owner == Main.myPlayer)
            {
                SpawnSummon();
            }
        }

        private void SpawnSummon()
        {
            Vector2 spawnPos = Projectile.Center;

            // 播放召唤音效
            SoundEngine.PlaySound(SoundID.Item119, spawnPos); // 类似传送的声音

            // --- 随机事件判定 (TRPG风格) ---
            int roll = Main.rand.Next(100); // 0 ~ 99

            // 1. 灵界异变 (5% 概率): 召唤敌对生物！
            if (roll < 5)
            {
                Main.NewText("警告：召唤仪式发生未知异变！某种东西钻出来了！", 255, 50, 50);
                SoundEngine.PlaySound(SoundID.Roar, spawnPos);

                // 召唤敌对的蛾怪 (日食Boss)
                int npcId = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.Mothron);
                // 注意：这里不给它加 TamedBuff，所以它是敌对的！

                // 增加一点戏剧性效果
                for (int i = 0; i < 50; i++)
                    Dust.NewDust(spawnPos, 100, 100, DustID.Blood, 0, 0, 0, default, 2f);
            }
            // 2. 天使/强力契约 (15% 概率): 召唤极强生物
            else if (roll < 20)
            {
                Main.NewText("契约达成：高位灵界生物响应了召唤。", 255, 215, 0); // 金色提示

                // 召唤 飞龙 (Wyvern) 或 附魔剑
                int type = NPCID.WyvernHead;
                int npcId = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)spawnPos.X, (int)spawnPos.Y, type);

                if (npcId != Main.maxNPCs) // 确保生成成功
                {
                    NPC summon = Main.npc[npcId];
                    // 赋予驯服 Buff (10分钟)
                    summon.AddBuff(ModContent.BuffType<TamedBuff>(), 36000);
                    // 属性大幅强化
                    summon.damage = (int)(summon.damage * 3f);
                    summon.lifeMax = (int)(summon.lifeMax * 3f);
                    summon.life = summon.lifeMax;
                    summon.defense += 50;
                }
            }
            // 3. 正常召唤 (80% 概率)
            else
            {
                Main.NewText("召唤成功。", 150, 150, 255);

                // 召唤池：帕拉丁、红魔鬼、死灵法师、巨型陆龟
                int[] summonPool = { NPCID.Paladin, NPCID.RedDevil, NPCID.Necromancer, NPCID.GiantTortoise, NPCID.DiabolistRed };
                int type = summonPool[Main.rand.Next(summonPool.Length)];

                int npcId = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)spawnPos.X, (int)spawnPos.Y, type);

                if (npcId != Main.maxNPCs)
                {
                    NPC summon = Main.npc[npcId];
                    // 赋予驯服 Buff (5分钟)
                    summon.AddBuff(ModContent.BuffType<TamedBuff>(), 18000);

                    // 属性小幅强化
                    summon.damage = (int)(summon.damage * 1.5f);
                    summon.lifeMax = (int)(summon.lifeMax * 1.5f);
                    summon.life = summon.lifeMax;
                }
            }
        }

        // 【核心绘制】处理旋转和贴图中心对齐
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            // 计算绘制位置：屏幕坐标
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 计算原点：图片的中心
            Vector2 origin = texture.Size() / 2f;

            // 颜色处理：白色带透明
            Color color = Color.White * 0.9f;
            color.A = 200; // 稍微有点透明度

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null, // Source rectangle (null for whole image)
                color,
                Projectile.rotation, // 这里传入旋转角度
                origin,              // 这里传入旋转中心
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // 返回 false 以阻止原版默认绘制 (因为我们已经手动画了)
            return false;
        }
    }
}