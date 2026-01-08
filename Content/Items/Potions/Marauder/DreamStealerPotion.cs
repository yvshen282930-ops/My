using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class DreamStealerPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("魔药：窃梦家");
            // Tooltip.SetDefault("序列5 窃梦家\n" +
            //                  "你可以窃取梦境、记忆甚至他人意图。\n" +
            //                  "获得“窃取意图”（攻击致使敌人呆滞）与“伪装”（战斗中窃取神灵回应以获得增益）。\n" +
            //                  "“梦境是现实的倒影，也是我的猎场。”");
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
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Pink; // 序列5
            Item.value = Item.sellPrice(gold: 10);
            Item.buffType = BuffID.Suffocation; // 服用副作用：致死的窒息感
            Item.buffTime = 300; // 5秒窒息
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.baseMarauderSequence == 6)
            {
                modPlayer.baseMarauderSequence = 5;
                Main.NewText("思维的边界开始模糊，你听到了周围无数梦境的低语...", 100, 100, 255);
                return true;
            }
            else if (modPlayer.baseMarauderSequence < 6)
            {
                Main.NewText("你已经是梦境的主宰，无需再次服用。", 150, 150, 150);
                return false;
            }
            else
            {
                Main.NewText("你的灵性不足以支撑这种窃取，先成为盗火人！", 255, 50, 50);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.BlackLens, 1)    // 食梦鼠心脏
                .AddIngredient(ItemID.SoulofNight, 5)  // 堕落之气
                .AddIngredient(ItemID.Sapphire, 1)     // 天青石
                .AddIngredient(ItemID.Moonglow, 3)     // 薰衣草
                .AddTile(TileID.DemonAltar)            // 恶魔祭坛合成，象征仪式
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}