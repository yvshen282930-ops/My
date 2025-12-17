using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using zhashi.Content.Items; // 引用你的物品命名空间

namespace zhashi.Content.Globals
{
    // 1. 处理怪物和Boss的掉落
    public class BlasphemyGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // === 需求：每个Boss都有 10% (1/10) 概率掉落 ===
            if (npc.boss)
            {
                // ItemDropRule.Common(物品ID, 分母) -> 1/10 概率
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlasphemySlate>(), 10));
            }

            // === 额外补充 1：红魔鬼 (地狱) ===
            // 设定：地狱的强力恶魔携带亵渎之物很合理
            // 概率：1% (1/100)
            if (npc.type == NPCID.RedDevil)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlasphemySlate>(), 100));
            }

            // === 额外补充 2：地牢幽魂 (花后地牢) ===
            // 设定：古老的灵体可能携带着过去的石板
            // 概率：2% (1/50)
            if (npc.type == NPCID.DungeonSpirit)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlasphemySlate>(), 50));
            }
        }
    }

    // 2. 处理物品（宝箱匣/摸彩袋）的掉落
    public class BlasphemyGlobalItem : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            // === 需求：钓鱼宝箱匣 (Crates) 1% (1/100) 概率 ===
            // ItemID.Sets.IsFishingCrate 包含了原版所有的木匣、铁匣、金匣以及困难模式对应的匣子
            if (ItemID.Sets.IsFishingCrate[item.type])
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlasphemySlate>(), 100));
            }

            // === 补充机制：Boss 宝藏袋 (Expert/Master 模式) ===
            // 在专家/大师模式下，Boss本体可能不掉东西，而是掉宝藏袋。
            // 为了保证 10% 的概率依然有效，我们需要给宝藏袋也加上掉落规则。
            if (ItemID.Sets.BossBag[item.type])
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlasphemySlate>(), 10));
            }
        }
    }
}