using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Moon
{
    public class LifeGiverPotion : LotMItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 38;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Purple; // 红色/紫色稀有度
            Item.value = Item.sellPrice(platinum: 1);
        }

        public override bool CanUseItem(Player player)
        {
            // 必须是 序列3：召唤大师
            if (player.GetModPlayer<LotMPlayer>().currentMoonSequence != 3) return false;

            // 【仪式条件】：濒死状态 (血量低于 5%)
            if (player.statLife > player.statLifeMax2 * 0.05f)
            {
                return false; // 身体还太健康，无法完成“置之死地而后生”的仪式
            }

            return true;
        }

        public override bool ConsumeItem(Player player)
        {
            // 如果血量不满足，不消耗物品（虽然CanUseItem已经挡住了，双重保险）
            return player.statLife <= player.statLifeMax2 * 0.05f;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentMoonSequence = 2; // 晋升：创生者

                // 瞬间满血复活 (仪式完成)
                player.statLife = player.statLifeMax2;
                player.HealEffect(player.statLifeMax2);

                CombatText.NewText(player.getRect(), Color.LightGreen, "晋升：创生者！", true);
                Main.NewText("你感受到了万物滋长的喜悦，你即是生命本身。", 50, 255, 50);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);
            }
            return true;
        }

        public override void AddRecipes()
        {
            // 配方：月亮领主后
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.FragmentSolar, 10)
                .AddIngredient(ItemID.FragmentVortex, 10)
                .AddIngredient(ItemID.FragmentNebula, 10)
                .AddIngredient(ItemID.FragmentStardust, 10)
                .AddIngredient(ItemID.LunarBar, 5) // 夜明锭
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}