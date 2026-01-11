using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using zhashi.Content.Items;
using zhashi.Content.Items.Accessories; // 【新增】引用卡牌所在的命名空间

namespace zhashi.Content.Systems
{
    public class BlasphemyCardGen : ModSystem
    {
        public override void PostWorldGen()
        {
            // =====================================================
            // 1. 定义生成名单
            // =====================================================
            List<int> cardsToSpawn = new List<int>();

            cardsToSpawn.Add(ModContent.ItemType<FoolCard>());
)
            cardsToSpawn.Add(ModContent.ItemType<StrengthCard>());

            cardsToSpawn.Add(ModContent.ItemType<LoversCard>());

            // === 未来有新牌直接在这里继续 Add ===
            // cardsToSpawn.Add(ModContent.ItemType<HangedManCard>());


            // =====================================================
            // 2. 收集全图宝箱
            // =====================================================
            List<int> validChestIndexes = new List<int>();

            for (int i = 0; i < Main.maxChests; i++)
            {
                Chest chest = Main.chest[i];
                if (chest != null)
                {
                    Tile tile = Main.tile[chest.x, chest.y];
                    // 包含普通箱子、金箱子、生物群落箱等大部分容器
                    if (tile.TileType == TileID.Containers || tile.TileType == TileID.Containers2)
                    {
                        validChestIndexes.Add(i);
                    }
                }
            }

            if (validChestIndexes.Count == 0) return;

            // =====================================================
            // 3. 执行生成
            // =====================================================
            UnifiedSpawnHelper(cardsToSpawn, validChestIndexes);
        }

        private void UnifiedSpawnHelper(List<int> itemsToSpawn, List<int> validChests)
        {
            UnifiedRandom rand = WorldGen.genRand;

            foreach (int itemID in itemsToSpawn)
            {
                if (validChests.Count == 0) break;

                bool success = false;
                int attempts = 0;

                while (!success && validChests.Count > 0 && attempts < 50)
                {
                    int randomPick = rand.Next(validChests.Count);
                    int chestIndex = validChests[randomPick];
                    Chest chest = Main.chest[chestIndex];

                    for (int slot = 0; slot < 40; slot++)
                    {
                        if (chest.item[slot].type == ItemID.None)
                        {
                            chest.item[slot].SetDefaults(itemID);
                            success = true;
                            break;
                        }
                    }

                    // 无论成功与否，移除这个箱子，确保不会把两张牌生成在同一个箱子里
                    validChests.RemoveAt(randomPick);
                    attempts++;
                }
            }
        }
    }
}