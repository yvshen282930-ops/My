using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Buffs;

namespace zhashi.Content.NPCs
{
    public class TamingGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // 攻击冷却计时器，防止每帧都造成伤害（秒杀）
        public int attackCooldown = 0;

        // 记录原始伤害
        public int originalDamage = -1;

        public override void ResetEffects(NPC npc)
        {
            // 每帧重置
        }

        // 核心：修改 NPC 的行为
        public override bool PreAI(NPC npc)
        {
            // 检查怪物是否有“驯服”Buff
            if (npc.HasBuff(ModContent.BuffType<TamedBuff>()))
            {
                // 保存原始伤害
                if (originalDamage == -1) originalDamage = npc.damage;

                // 1. 强制阵营转换
                npc.friendly = true;
                npc.dontTakeDamageFromHostiles = false;

                // 2. 强制赋予伤害值 (不再倍率增加，只保证不为0)
                if (npc.damage < npc.defDamage)
                {
                    npc.damage = npc.defDamage;
                }
                // 保底伤害，防止完全打不动
                if (npc.damage < 10) npc.damage = 10;

                // 3. 索敌逻辑
                NPC target = FindClosestEnemy(npc);

                // 4. 移动与攻击逻辑
                if (target != null)
                {
                    // --- 移动部分 ---
                    float speed = 6f;
                    if (npc.aiStyle == 3) speed = 8f; // 战士类跑快点

                    float inertia = 15f;

                    Vector2 direction = target.Center - npc.Center;
                    direction.Normalize();
                    direction *= speed;

                    npc.velocity = (npc.velocity * (inertia - 1) + direction) / inertia;

                    // 跳跃
                    if (npc.velocity.X != 0)
                    {
                        Tile tile = Framing.GetTileSafely((int)(npc.Center.X + npc.velocity.X) / 16, (int)(npc.Bottom.Y) / 16);
                        if (tile.HasTile && Main.tileSolid[tile.TileType] || target.Center.Y < npc.Center.Y - 100)
                        {
                            if (npc.velocity.Y == 0) npc.velocity.Y = -7f;
                        }
                    }

                    // 朝向 (手动处理，这就足够了，不需要 faceTarget)
                    npc.direction = npc.spriteDirection = (target.Center.X > npc.Center.X) ? 1 : -1;

                    // --- 手动碰撞伤害 ---
                    // 如果物理引擎失效，我们手动让它们打架
                    CheckCollisionDamage(npc, target);
                }
                else
                {
                    // 没敌人时跟随玩家
                    Player owner = Main.LocalPlayer;
                    if (npc.Distance(owner.Center) > 200f)
                    {
                        Vector2 dir = owner.Center - npc.Center;
                        dir.Normalize();
                        npc.velocity = (npc.velocity * 29f + dir * 8f) / 30f;

                        // 跟随玩家时的朝向
                        if (npc.velocity.X != 0)
                            npc.direction = npc.spriteDirection = (npc.velocity.X > 0) ? 1 : -1;
                    }
                }

                // 5. 屏蔽原版 AI (防止乱跑)
                if (npc.aiStyle == 3 || npc.aiStyle == 1 || npc.aiStyle == 26 || npc.aiStyle == 14)
                {
                    return false;
                }
            }
            return true;
        }

        // 手动检测碰撞并造成伤害
        private void CheckCollisionDamage(NPC me, NPC target)
        {
            // 如果在冷却中，不造成伤害
            if (attackCooldown > 0)
            {
                attackCooldown--;
                return;
            }

            // 获取两个怪物的碰撞箱 (Hitbox)
            Rectangle myRect = me.getRect();
            Rectangle targetRect = target.getRect();

            // 如果发生重叠 (撞上了)
            if (myRect.Intersects(targetRect))
            {
                // 计算伤害：直接使用当前伤害值
                int damage = me.damage;

                // 击退方向
                int hitDirection = (target.Center.X > me.Center.X) ? 1 : -1;

                // 强制造成伤害！这会直接扣血并跳出数字
                target.SimpleStrikeNPC(damage, hitDirection, false, 0f, DamageClass.Generic, false, 0f, true);

                // 简单的撞击反馈 (稍微弹开一点)
                me.velocity.X *= -0.5f;

                // 设置攻击冷却 (30帧 = 0.5秒)，避免一秒钟造成60次伤害
                attackCooldown = 30;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<TamedBuff>()))
            {
                drawColor = Color.Lerp(drawColor, Color.HotPink, 0.5f);
            }
        }

        public override bool CanHitNPC(NPC npc, NPC target)
        {
            if (npc.HasBuff(ModContent.BuffType<TamedBuff>()) && !target.friendly)
            {
                return true;
            }
            return base.CanHitNPC(npc, target);
        }

        // 稍微加一点穿透，不然打高防怪还是会刮痧 (你可以把这个删掉如果想要绝对的原版伤害)
        public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<TamedBuff>()))
            {
                modifiers.ArmorPenetration += 5;
            }
        }

        private NPC FindClosestEnemy(NPC me)
        {
            NPC closest = null;
            float minDist = 1000f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && !other.friendly && other.whoAmI != me.whoAmI && other.lifeMax > 5 && !other.dontTakeDamage && !other.HasBuff(ModContent.BuffType<TamedBuff>()))
                {
                    float dist = Vector2.Distance(me.Center, other.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = other;
                    }
                }
            }
            return closest;
        }
    }
}