using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter // 建议加上 .Hunter 区分命名空间，或者保持 zhashi.Content.Items.Potions 也可以
{
    // 【核心修复】类名必须是 HunterPotion，不能是 WarriorPotion
    public class HunterPotion : LotMItem
    {
        public override string Pathway => "Hunter"; // 设定途径
        public override int RequiredSequence => 10;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("猎人魔药");
            // Tooltip.SetDefault("序列9：猎人\n获得敏锐的感官与卓越的身体素质");
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
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
            Item.buffType = BuffID.WellFed;
            Item.buffTime = 300;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 【核心逻辑】检查是否已是非凡者 (防止多重途径)
                if (modPlayer.IsBeyonder)
                {
                    Main.NewText("你的灵性已定型，无法开启第二条途径，强行服用只会导致失控！", 255, 50, 50);
                    return true; // 消耗掉作为惩罚
                }

                // 晋升逻辑：猎人途径
                modPlayer.currentHunterSequence = 9;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("你的感官瞬间变得敏锐，仿佛能嗅到空气中猎物的气息...", 200, 100, 50);
                Main.NewText("晋升成功！序列9：猎人！", 255, 100, 0);
            }
            return true;
        }

        public override void AddRecipes()
        {
            // 猎人配方：水瓶 + 麦芽酒 + 太阳花 + 凝胶(5)
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Ale, 1)          // 麦芽酒
                .AddIngredient(ItemID.Daybloom, 1)     // 太阳花
                .AddIngredient(ItemID.Gel, 5)          // 凝胶
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}