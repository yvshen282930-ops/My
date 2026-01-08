using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Moon
{
    public class PotionsProfessorPotion : LotMItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.LightRed; // 稍微稀有一些
            Item.value = Item.sellPrice(gold: 5);
        }

        public override bool CanUseItem(Player player)
        {
            // 必须是 序列7：吸血鬼
            return player.GetModPlayer<LotMPlayer>().baseMoonSequence == 7;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.baseMoonSequence = 6; // 晋升

                CombatText.NewText(player.getRect(), Color.MediumPurple, "晋升：魔药教授", true);
                Main.NewText("你洞悉了物质与灵性的转化规律...", 200, 100, 255);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item107, player.position); // 类似魔法的声音
            }
            return true;
        }

        public override void AddRecipes()
        {
            // 配方 1：适配猩红世界 (使用灵液)
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.LifeCrystal, 1)    // 心脏
                .AddIngredient(ItemID.Ichor, 5)          // 灵液 (代表含有灵性的血液)
                .AddIngredient(ItemID.SharkFin, 3)       // 鲨鱼鳍
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1) // 亵渎石板
                .AddTile(TileID.Bottles)                 // 放置的瓶子作为制作站
                .Register();

            // 配方 2：适配腐化世界 (使用咒火)
            // 这样无论玩家的世界是血腥还是腐化，都能合成这个魔药
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.LifeCrystal, 1)
                .AddIngredient(ItemID.CursedFlame, 5)    // 咒火 (作为灵液的替代品)
                .AddIngredient(ItemID.SharkFin, 3)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}