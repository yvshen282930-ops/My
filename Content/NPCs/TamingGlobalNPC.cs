using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO; 
using zhashi.Content.Buffs;
using zhashi.Content;

namespace zhashi.Content.NPCs
{
    public class TamingGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int attackCooldown = 0;
        public int originalDamage = -1;
        public int ownerIndex = -1;

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
                // 【核心逻辑修复】
                // 1. 如果没有主人索引，或者索引不合法，暂时按默认逻辑处理或解除Buff
                if (ownerIndex < 0 || ownerIndex >= Main.maxPlayers)
                {
                    // 尝试寻找最近的玩家作为临时主人，或者直接返回
                    // 这里简单处理：如果丢失主人，Buff可能就失效了，或者直接return true让它按原版AI走
                    // 但为了防吞怪，我们先让它呆着
                    npc.velocity *= 0.9f;
                    return false;
                }

                Player owner = Main.player[ownerIndex];

                // 2. 检查主人是否在线/存活
                if (!owner.active || owner.dead)
                {
                    // 主人没了，解除驯服，或者让怪消失
                    npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<TamedBuff>()));
                    return true; // 恢复原版AI
                }

                // 3. 只有服务器和主人客户端需要执行“永久存在”逻辑
                // 其他客户端只需要负责渲染
                var modPlayer = owner.GetModPlayer<LotMPlayer>();

                // 血仆永久逻辑
                if (modPlayer.currentMoonSequence <= 7)
                {
                    int buffIndex = npc.FindBuffIndex(ModContent.BuffType<TamedBuff>());
                    if (buffIndex != -1) npc.buffTime[buffIndex] = 30000;
                }

                // 4. 防止被系统刷没 (Despawn)
                npc.timeLeft = 2; // 只要Buff还在，就强行续命

                if (originalDamage == -1) originalDamage = npc.damage;

                // 属性修正
                npc.friendly = true;
                npc.dontTakeDamageFromHostiles = false;
                npc.target = -1; // 清除针对玩家的仇恨

                // 伤害加成
                float damageMult = 1.0f;
                if (modPlayer.currentMoonSequence <= 7) damageMult = 1.5f;

                if (npc.damage < npc.defDamage * damageMult) npc.damage = (int)(npc.defDamage * damageMult);
                if (npc.damage < 10) npc.damage = 10;

                // --- AI 逻辑 ---
                NPC target = FindClosestEnemy(npc, owner); // 传入 owner 以辅助判断

                if (target != null)
                {
                    // 战斗模式
                    npc.target = target.whoAmI;
                    MoveTowards(npc, target.Center, 6f, 15f);
                    CheckCollisionDamage(npc, target);
                }
                else
                {
                    // 跟随模式 (跟随 owner，而不是 Main.LocalPlayer)
                    npc.target = -1;

                    float dist = npc.Distance(owner.Center);

                    // 距离过远传送逻辑
                    if (dist > 1500f)
                    {
                        npc.Center = owner.Center;
                        npc.velocity = Vector2.Zero;
                        // 传送后需要同步位置，防止客户端看到的还在远处
                        npc.netUpdate = true;
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

                // 屏蔽原版 AI
                if (npc.aiStyle == 3 || npc.aiStyle == 1 || npc.aiStyle == 26 || npc.aiStyle == 14 || npc.aiStyle == 2)
                {
                    return false;
                }
            }
            else
            {
                // 如果没有Buff，重置主人（可选）
                ownerIndex = -1;
            }
            return true;
        }
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(ownerIndex);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            ownerIndex = binaryReader.ReadInt32();
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

                if (ownerIndex != -1 && Main.player[ownerIndex].active)
                {
                    if (Main.player[ownerIndex].GetModPlayer<LotMPlayer>().currentMoonSequence <= 7)
                    {
                        int heal = damage / 10;
                        if (heal < 1) heal = 1;
                        me.life += heal;
                        if (me.life > me.lifeMax) me.life = me.lifeMax;
                        if (Main.rand.NextBool(3)) me.HealEffect(heal);
                    }
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

        private NPC FindClosestEnemy(NPC me, Player owner)
        {
            NPC closest = null;
            float minDist = 1000f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                // 确保不攻击主人、友方、自己、以及其他被驯服的怪
                if (other.active && !other.friendly && other.whoAmI != me.whoAmI && other.lifeMax > 5 && !other.dontTakeDamage && !other.HasBuff(ModContent.BuffType<TamedBuff>()))
                {
                    // 优先攻击离怪物自己近的
                    float dist = Vector2.Distance(me.Center, other.Center);
                    if (dist < minDist) { minDist = dist; closest = other; }
                }
            }
            return closest;
        }
    }
}