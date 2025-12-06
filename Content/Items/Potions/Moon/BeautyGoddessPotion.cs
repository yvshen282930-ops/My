using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Moon
{
    public class BeautyGoddessPotion : LotMItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 38;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Red; // 最高稀有度
            Item.value = Item.sellPrice(platinum: 5);
        }

        public override bool CanUseItem(Player player)
        {
            // 必须是 序列2：创生者
            if (player.GetModPlayer<LotMPlayer>().currentMoonSequence != 2) return false;

            // 【仪式条件】：深埋地底 + 沉睡 (模拟棺材)
            // Player.sleeping.isSleeping 检查玩家是否在床上睡觉
            // ZoneRockLayerHeight 检查是否在岩石层及以下
            if (!player.sleeping.isSleeping || !player.ZoneRockLayerHeight)
            {
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentMoonSequence = 1; // 晋升：美神

                // 强制转变为女性
                player.Male = false;

                CombatText.NewText(player.getRect(), Color.HotPink, "晋升：美神！", true);
                Main.NewText("世间的一切光辉都汇聚于你，万物因你的容颜而屏息。", 255, 105, 180);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item123, player.position);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.LunarBar, 20)      // 夜明锭
                .AddIngredient(ItemID.FragmentNebula, 10)
                .AddIngredient(ItemID.FragmentSolar, 10)
                .AddIngredient(ItemID.FragmentStardust, 10)
                .AddIngredient(ItemID.FragmentVortex, 10)
                .AddIngredient(ItemID.LifeCrystal, 5)    // 生命象征
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}