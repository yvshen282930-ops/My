using System;
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

        public int attackCooldown = 0;
        public int originalDamage = -1;

        // 【核心修改】将续命逻辑移至 PreAI，确保在 Buff 消失前执行
        public override bool PreAI(NPC npc)
        {
            // 检查怪物是否有“驯服”Buff
            if (npc.HasBuff(ModContent.BuffType<TamedBuff>()))
            {
                // --- 1. 永久存在逻辑 (吸血鬼) ---
                Player player = Main.LocalPlayer;
                if (player.active && !player.dead)
                {
                    var modPlayer = player.GetModPlayer<LotMPlayer>();
                    // 如果是 序列7：吸血鬼 或更高阶
                    if (modPlayer.currentMoonSequence <= 7)
                    {
                        // 找到 Buff 的索引
                        int buffIndex = npc.FindBuffIndex(ModContent.BuffType<TamedBuff>());
                        if (buffIndex != -1)
                        {
                            // 强行锁定时间为 30000 帧 (如果不死，永远不会耗尽)
                            npc.buffTime[buffIndex] = 30000;
                        }
                    }
                }

                // --- 2. 基础属性初始化 ---
                if (originalDamage == -1) originalDamage = npc.damage;

                npc.friendly = true;
                npc.dontTakeDamageFromHostiles = false;

                // 序列7加成：伤害提升
                float damageMult = 1.0f;
                if (Main.LocalPlayer.GetModPlayer<LotMPlayer>().currentMoonSequence <= 7) damageMult = 1.5f;

                if (npc.damage < npc.defDamage * damageMult) npc.damage = (int)(npc.defDamage * damageMult);
                if (npc.damage < 10) npc.damage = 10;

                // --- 3. 索敌 ---
                NPC target = FindClosestEnemy(npc);

                // --- 4. 行为逻辑 ---
                if (target != null)
                {
                    // 战斗模式：追敌人
                    MoveTowards(npc, target.Center, 6f, 15f);
                    CheckCollisionDamage(npc, target);
                }
                else
                {
                    // 跟随模式：追玩家
                    Player owner = Main.LocalPlayer;
                    float dist = npc.Distance(owner.Center);

                    // 距离太远直接传送 (防丢失)
                    if (dist > 1500f)
                    {
                        npc.Center = owner.Center;
                        npc.velocity = Vector2.Zero;
                    }
                    // 距离较远开始跟随
                    else if (dist > 200f)
                    {
                        MoveTowards(npc, owner.Center, 5f, 20f);
                    }
                    // 距离近了减速停下
                    else
                    {
                        npc.velocity.X *= 0.9f;
                        if (npc.velocity.Y == 0 && Math.Abs(npc.velocity.X) < 0.1f)
                        {
                            npc.velocity.X = 0;
                            // 闲置时看向玩家
                            npc.direction = npc.spriteDirection = (owner.Center.X > npc.Center.X) ? 1 : -1;
                        }
                    }
                }

                // --- 5. 屏蔽原版 AI ---
                if (npc.aiStyle == 3 || npc.aiStyle == 1 || npc.aiStyle == 26 || npc.aiStyle == 14)
                {
                    return false;
                }
            }
            return true;
        }

        // 移动逻辑 (包含跳跃)
        private void MoveTowards(NPC npc, Vector2 targetPos, float speedBase, float inertia)
        {
            float speed = speedBase;
            if (npc.aiStyle == 3) speed += 2f;

            Vector2 direction = targetPos - npc.Center;
            direction.Normalize();
            direction *= speed;

            // 水平移动
            npc.velocity.X = (npc.velocity.X * (inertia - 1) + direction.X) / inertia;

            // 跳跃判定
            bool isTargetAbove = targetPos.Y < npc.Center.Y - 50;
            bool onGround = npc.velocity.Y == 0;
            bool hitWall = npc.collideX;

            if (onGround)
            {
                if (hitWall || isTargetAbove)
                {
                    npc.velocity.Y = -7.5f;
                    npc.velocity.X += npc.direction * 1.5f;
                }
            }

            // 更新朝向
            if (npc.velocity.X != 0)
                npc.direction = npc.spriteDirection = (targetPos.X > npc.Center.X) ? 1 : -1;
        }

        // 碰撞伤害逻辑
        private void CheckCollisionDamage(NPC me, NPC target)
        {
            if (attackCooldown > 0) { attackCooldown--; return; }

            if (me.getRect().Intersects(target.getRect()))
            {
                int damage = me.damage;
                int hitDirection = (target.Center.X > me.Center.X) ? 1 : -1;
                target.SimpleStrikeNPC(damage, hitDirection, false, 0f, DamageClass.Generic, false, 0f, true);

                // 吸血鬼加成：吸血
                if (Main.LocalPlayer.GetModPlayer<LotMPlayer>().currentMoonSequence <= 7)
                {
                    int heal = damage / 10;
                    if (heal < 1) heal = 1;
                    me.life += heal;
                    if (me.life > me.lifeMax) me.life = me.lifeMax;
                    if (Main.rand.NextBool(3)) me.HealEffect(heal);
                }

                me.velocity.X *= -0.5f;
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
            if (npc.HasBuff(ModContent.BuffType<TamedBuff>()) && !target.friendly) return true;
            return base.CanHitNPC(npc, target);
        }

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
                    if (dist < minDist) { minDist = dist; closest = other; }
                }
            }
            return closest;
        }
    }
}