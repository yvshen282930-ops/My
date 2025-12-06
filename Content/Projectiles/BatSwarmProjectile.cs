using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Projectiles
{
    public class BatSwarmProjectile : ModProjectile
    {
        public override string Texture => "zhashi/Content/Projectiles/BatSwarmProjectile";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4; // 4帧动画
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2; // 只要技能开着就一直存在
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.scale = Main.rand.NextFloat(0.7f, 1.1f); // 大小差异化
            // 启用无敌帧，防止太多蝙蝠同时打一个怪导致伤害丢失（骗伤）
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead) { Projectile.Kill(); return; }
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            if (!modPlayer.isBatSwarm) { Projectile.Kill(); return; }

            Projectile.timeLeft = 2;

            // --- 1. 动画播放 ---
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            // --- 2. 混沌云团算法 (解决圆环和跟随慢的问题) ---

            // 使用弹幕ID作为随机种子，让每只蝙蝠有独立的运动轨迹
            float seed = Projectile.whoAmI * 1.3f;
            float time = (float)Main.timeForVisualEffects * 0.05f;

            // 动态半径：随时间呼吸，且每只不一样
            float radiusX = 60f + (float)Math.Sin(time * 0.5f + seed) * 40f;
            float radiusY = 50f + (float)Math.Cos(time * 0.3f + seed) * 30f;

            // 计算相对于玩家的目标位置
            // 这里的算法会产生不规则的椭圆+扰动，看起来像虫群/蝙蝠群
            Vector2 offset = new Vector2(
                (float)Math.Sin(time + seed) * radiusX,
                (float)Math.Cos(time * 0.8f + seed * 1.5f) * radiusY
            );

            // 目标绝对位置
            Vector2 targetPos = player.Center + offset;

            // --- 3. 快速跟随逻辑 ---
            // 使用 Lerp 插值，因子设为 0.2 (非常快)，解决慢一拍的问题
            // 如果距离过远（传送），直接瞬移
            if (Vector2.Distance(Projectile.Center, targetPos) > 500f)
            {
                Projectile.Center = targetPos;
            }
            else
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.2f);
            }

            // --- 4. 索敌攻击微调 ---
            // 如果附近有敌人，稍微向敌人偏移一点点，增加攻击欲望
            NPC target = FindClosestNPC(400f);
            if (target != null)
            {
                Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.Center += attackDir * 3f; // 稍微往敌人的方向“吸”过去
            }

            // --- 5. 朝向与旋转 ---
            // 计算这一帧的位移作为伪速度
            Vector2 delta = Projectile.Center - Projectile.oldPosition;
            if (delta.X != 0) Projectile.spriteDirection = delta.X > 0 ? -1 : 1;
            Projectile.rotation = delta.X * 0.1f;

            // --- 6. 粒子 ---
            if (Main.rand.NextBool(30))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0, 0, 150, default, 0.6f);
                d.noGravity = true;
            }
        }

        // 简单的索敌辅助
        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float sqrMaxDist = maxDist * maxDist;
            foreach (var npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && !npc.dontTakeDamage)
                {
                    float sqrDist = Vector2.DistanceSquared(npc.Center, Projectile.Center);
                    if (sqrDist < sqrMaxDist)
                    {
                        sqrMaxDist = sqrDist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        // 伤害修正
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration += 20; // 强力破甲
        }
    }
}