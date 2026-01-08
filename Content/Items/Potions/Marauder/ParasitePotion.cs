using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Buffs;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class ParasitePotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("寄生者魔药"); // 1.4.4+ 使用非凡特性会自动显示，若需强制指定可取消注释
            // Tooltip.SetDefault("序列4：寄生者\n" +
            //                  "“他”能看见宿主看见的所有事情，听到宿主听到的所有声音。\n" +
            //                  "增加生命上限、灵性上限与窃取能力。\n" +
            //                  "允许寄生于他人体内，或进行概念层面的窃取。");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.LightPurple; // 序列4半神，稀有度较高
            Item.value = Item.sellPrice(gold: 10);
        }

        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            // 1. 检查序列是否为 5
            if (modPlayer.baseMarauderSequence != 5) return false;

            // 2. 检查仪式是否完成
            if (modPlayer.parasiteRitualProgress < LotMPlayer.PARASITE_RITUAL_TARGET)
            {
                // 提示玩家还需要多少次
                int remaining = LotMPlayer.PARASITE_RITUAL_TARGET - modPlayer.parasiteRitualProgress;
                Main.NewText($"仪式未完成：你需要再通过攻击“窃取” {remaining} 次敌人的物品，以此作为供养。", 255, 100, 100);
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                // 晋升逻辑
                modPlayer.baseMarauderSequence = 4;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position); // 晋升时的咆哮
                Main.NewText("你感到身体在分解，无数微小的虫豸重组了你的血肉...", 175, 238, 238);
                Main.NewText("你晋升为 序列4：寄生者！", 255, 215, 0);
                return true;
            }
            return null;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Ectoplasm, 5)   // 夺魂怪死后的结晶
                .AddIngredient(ItemID.TruffleWorm, 1) // 傀儡邪虫
                .AddIngredient(ItemID.Amethyst, 1)    // 紫水晶
                .AddIngredient(ItemID.SoulofNight, 5) // 被囚禁的灵魂
                .AddIngredient(ItemID.Grapes, 1)      // 樱桃李 (用葡萄代替)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}