using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Items;

namespace zhashi.Content.Systems
{
    public class BlasphemySlateGen : ModSystem
    {
        public override void PostWorldGen()
        {
            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {
                Chest chest = Main.chest[chestIndex];
                if (chest != null && Main.tile[chest.x, chest.y].TileType == TileID.Containers)
                {
                    // 20% 概率生成
                    if (Main.rand.NextBool(3))
                    {
                        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                        {
                            if (chest.item[inventoryIndex].type == ItemID.None)
                            {
                                // 创建物品实例
                                int type = ModContent.ItemType<BlasphemySlate>();
                                chest.item[inventoryIndex].SetDefaults(type);

                                // 【关键修复】获取 ModItem 实例并手动设为 -1
                                // 这样在箱子里时，它就是"未观测状态"
                                if (chest.item[inventoryIndex].ModItem is BlasphemySlate slate)
                                {
                                    slate.recipeIndex = -1;
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}