using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;     // 用于 NPCID
using Terraria.Audio;  // 【关键修复】用于 SoundEngine
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.NPCs
{
    public class HunterRitualGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            // 1. 排除无效目标
            if (npc.friendly || npc.SpawnedFromStatue || npc.lifeMax < 5 || npc.type == NPCID.TargetDummy)
                return;

            // 2. 寻找击杀者
            if (npc.lastInteraction == -1)
                return;

            Player killer = Main.player[npc.lastInteraction];

            if (!killer.active || killer.dead)
                return;

            // 3. 检查仪式条件
            LotMPlayer modPlayer = killer.GetModPlayer<LotMPlayer>();

            if (modPlayer.currentHunterSequence == 5)
            {
                // 必须有至少 5 个随从
                if (killer.numMinions >= 5)
                {
                    if (modPlayer.ironBloodRitualProgress < LotMPlayer.IRON_BLOOD_RITUAL_TARGET)
                    {
                        modPlayer.ironBloodRitualProgress++;

                        // 视觉反馈: 红色字体 "+1 铁血"
                        CombatText.NewText(killer.getRect(), new Color(255, 50, 50), "+1 铁血", false, false);

                        if (modPlayer.ironBloodRitualProgress == LotMPlayer.IRON_BLOOD_RITUAL_TARGET)
                        {
                            Main.NewText("铁血仪式条件已达成！你可以晋升了！", 50, 255, 50);
                            // 播放音效
                            SoundEngine.PlaySound(SoundID.MaxMana, killer.position);
                        }
                    }
                }
            }
        }
    }
}