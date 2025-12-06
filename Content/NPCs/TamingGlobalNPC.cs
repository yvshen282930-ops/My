using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Buffs;
using zhashi.Content;

namespace zhashi.Content.NPCs
{
    public class TamingGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int attackCooldown = 0;
        public int originalDamage = -1;

        public override void ResetEffects(NPC npc)
        {
            // 移除此处的逻辑，统一在 PreAI 处理
        }

        // 【核心修复1】强制禁止接触伤害
        // 只要有驯服 Buff，无论它想不想打你，系统都判定为打不到
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (npc.HasBuff(ModContent.BuffType<TamedBuff>()))
            {
                return false;
            }
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }

        public override bool PreAI(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<TamedBuff>()))
            {
                // --- 1. 永久存在逻辑 ---
                Player player = Main.LocalPlayer;
                if (player.active && !player.dead)
                {
                    var modPlayer = player.GetModPlayer<LotMPlayer>();
                    // 吸血鬼及以上：血仆永久
                    if (modPlayer.currentMoonSequence <= 7)
                    {
                        int buffIndex = npc.FindBuffIndex(ModContent.BuffType<TamedBuff>());
                        if (buffIndex != -1) npc.buffTime[buffIndex] = 30000;
                    }
                }

                if (originalDamage == -1) originalDamage = npc.damage;

                // --- 2. 属性修正 ---
                npc.friendly = true;
                npc.dontTakeDamageFromHostiles = false;

                // 【核心修复2】强制清除仇恨
                // 每一帧都告诉它：你没有攻击目标（针对玩家）
                npc.target = -1;

                // 序列7加成：伤害提升
                float damageMult = 1.0f;
                if (player.GetModPlayer<LotMPlayer>().currentMoonSequence <= 7) damageMult = 1.5f;

                if (npc.damage < npc.defDamage * damageMult) npc.damage = (int)(npc.defDamage * damageMult);
                if (npc.damage < 10) npc.damage = 10;

                // --- 3. 索敌与AI接管 ---
                NPC target = FindClosestEnemy(npc);

                if (target != null)
                {
                    // 战斗模式
                    // 欺骗原版AI，让它以为敌怪是它的攻击目标（有助于触发某些怪的远程攻击）
                    npc.target = target.whoAmI;

                    MoveTowards(npc, target.Center, 6f, 15f);
                    CheckCollisionDamage(npc, target);
                }
                else
                {
                    // 跟随模式
                    // 再次清除目标，防止它闲着没事想打玩家
                    npc.target = -1;

                    Player owner = Main.LocalPlayer;
                    float dist = npc.Distance(owner.Center);

                    if (dist > 1500f)
                    {
                        npc.Center = owner.Center;
                        npc.velocity = Vector2.Zero;
                    }
                    else if (dist > 200f)
                    {
                        MoveTowards(npc, owner.Center, 5f, 20f);
                    }
                    else
                    {
                        npc.velocity.X *= 0.9f;
                        if (npc.velocity.Y == 0 && Math.Abs(npc.velocity.X) < 0.1f)
                        {
                            npc.velocity.X = 0;
                            npc.direction = npc.spriteDirection = (owner.Center.X > npc.Center.X) ? 1 : -1;
                        }
                    }
                }

                // --- 4. 屏蔽原版 AI ---
                // 屏蔽常见 AI，防止乱跑
                if (npc.aiStyle == 3 || npc.aiStyle == 1 || npc.aiStyle == 26 || npc.aiStyle == 14 || npc.aiStyle == 2)
                {
                    return false;
                }
            }
            return true;
        }

        private void MoveTowards(NPC npc, Vector2 targetPos, float speedBase, float inertia)
        {
            float speed = speedBase;
            if (npc.aiStyle == 3) speed += 2f;

            Vector2 direction = targetPos - npc.Center;
            direction.Normalize();
            direction *= speed;

            npc.velocity.X = (npc.velocity.X * (inertia - 1) + direction.X) / inertia;

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

            if (npc.velocity.X != 0)
                npc.direction = npc.spriteDirection = (targetPos.X > npc.Center.X) ? 1 : -1;
        }

        private void CheckCollisionDamage(NPC me, NPC target)
        {
            if (attackCooldown > 0) { attackCooldown--; return; }

            if (me.getRect().Intersects(target.getRect()))
            {
                int damage = me.damage;
                int hitDirection = (target.Center.X > me.Center.X) ? 1 : -1;
                target.SimpleStrikeNPC(damage, hitDirection, false, 0f, DamageClass.Generic, false, 0f, true);

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

        // 允许被驯服的怪物攻击其他敌人
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