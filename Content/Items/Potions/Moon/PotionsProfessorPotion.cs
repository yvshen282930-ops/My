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
            return player.GetModPlayer<LotMPlayer>().currentMoonSequence == 7;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentMoonSequence = 6; // 晋升

                CombatText.NewText(player.getRect(), Color.MediumPurple, "晋升：魔药教授", true);
                Main.NewText("你洞悉了物质与灵性的转化规律...", 200, 100, 255);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item107, player.position); // 类似魔法的声音
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.LifeCrystal, 1)    // 心脏
                .AddIngredient(ItemID.Ichor, 5)          // 血液
                                                         // 【核心修复】改为 SharkFin (鲨鱼鳍)
                                                         // 原版没有 SharkTooth。或者你可以改用 ItemID.Vertebrae (椎骨)
                .AddIngredient(ItemID.SharkFin, 3)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}