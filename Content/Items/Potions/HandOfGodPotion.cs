using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Items.Materials;

namespace zhashi.Content.Items.Potions
{
    public class HandOfGodPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.consumable = true;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item3;
            Item.value = Item.sellPrice(gold: 50);
        }

        public override bool CanUseItem(Player player)
        {
            // 必须是序列2 荣耀战神 才能服用
            return player.GetModPlayer<LotMPlayer>().currentSequence == 2;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                // 1. 晋升逻辑
                modPlayer.currentSequence = 1;

                // 2. 给予武器 (核心需求：自动塞入背包)
                // 【关键修复】这里的方法名从 GetSource_UseItem 改为 GetSource_ItemUse
                player.QuickSpawnItem(player.GetSource_ItemUse(Item), ModContent.ItemType<Weapons.HandOfGod>());

                // 3. 文本与特效
                Main.NewText("你已晋升为序列1：神明之手！", 255, 100, 0);
                Main.NewText("你获得了神罚的权柄（武器已放入背包）。", 200, 200, 200);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                return true;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HandOfGodCharacteristic>(), 1)
                .AddIngredient(ItemID.BottledWater, 9)
                .AddIngredient(ItemID.LifeFruit, 5)
                .AddIngredient(ItemID.Ectoplasm, 20)
                .AddTile(TileID.DemonAltar)
                .AddCondition(Condition.InGraveyard)
                .Register();
        }
    }
}