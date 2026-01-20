using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using zhashi.Content.Buffs.Debuffs; // 引用刚才写的Buff

namespace zhashi.Content.Globals
{
    public class TimeStopGlobalNPC : GlobalNPC
    {
        public override bool PreAI(NPC npc)
        {
            // 检查是否有“时间凝固”Buff
            if (npc.HasBuff(ModContent.BuffType<TimeStagnationBuff>()))
            {
                // --- 1. 对于普通怪物：完全静止 (时间停止) ---
                if (!npc.boss)
                {
                    npc.velocity = Vector2.Zero; // 速度归零
                    npc.position = npc.oldPosition; // 强制锁死位置，防止重力下坠
                    npc.frameCounter = 0; // 停止动画帧播放
                    return false; // 【核心】：返回 false 禁止执行原版 AI (不攻击、不移动)
                }

                // --- 2. 对于 Boss：极度减速 (时间变缓) ---
                // 直接静止 Boss 容易导致脚本卡死（比如无敌阶段无法结束），所以我们改为变慢 90%
                else
                {
                    // 只有每 10 帧才执行一次 AI，相当于动作慢了 10 倍
                    if (Main.GameUpdateCount % 10 != 0)
                    {
                        npc.position -= npc.velocity * 0.9f; // 抵消大部分移动
                        return false; // 跳过这次 AI 思考
                    }
                }
            }
            return true; // 没有 Buff 则正常行动
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            // 只有被时停的怪才变灰
            if (npc.HasBuff(ModContent.BuffType<TimeStagnationBuff>()))
            {
                drawColor = Color.Gray;
            }
        }
    }
}