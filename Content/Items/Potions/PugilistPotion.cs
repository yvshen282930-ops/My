using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions
{
    public class PugilistPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green; // 绿色稀有度
            Item.value = Item.buyPrice(gold: 1);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 1. 检查是否为序列9
            if (modPlayer.baseSequence == 9)
            {
                modPlayer.baseSequence = 8;
                // 播放一声吼叫，更有其实
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你服用了格斗家魔药，肌肉与骨骼发出了爆鸣声... 晋升成功：序列8 格斗家！", 255, 150, 50);
                return true;
            }
            // 2. 检查能不能喝
            else if (modPlayer.baseSequence == 10)
            {
                Main.NewText("你的身体太孱弱了，先成为战士吧。", 200, 50, 50);
                return true; // 消耗掉药水作为惩罚，或者 return false 不消耗
            }
            else
            {
                Main.NewText("你已经是更高序列的存在，无需服用。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            // === 配方 1：对应腐化世界 (魔矿) ===
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.DemoniteBar, 5) // 魔矿锭 (克苏鲁之眼掉落矿石烧制)
                .AddIngredient(ItemID.Gel, 50)        // 50个凝胶 (象征史莱姆王)
                .AddIngredient(ItemID.JungleSpores, 3)// 丛林孢子
                .AddTile(TileID.Bottles)
                .Register();

            // === 配方 2：对应猩红世界 (猩红矿) ===
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.CrimtaneBar, 5) // 猩红矿锭 (克苏鲁之眼掉落矿石烧制)
                .AddIngredient(ItemID.Gel, 50)
                .AddIngredient(ItemID.JungleSpores, 3)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}