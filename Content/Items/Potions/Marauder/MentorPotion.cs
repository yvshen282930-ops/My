using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Buffs;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class MentorPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("欺瞒导师魔药"); 
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Lime; // 序列3 圣者
            Item.value = Item.sellPrice(gold: 20);
        }

        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 1. 必须是序列4
            if (modPlayer.currentMarauderSequence != 4) return false;

            // 2. 检查仪式：9个冤魂
            if (modPlayer.mentorRitualProgress < LotMPlayer.MENTOR_RITUAL_TARGET)
            {
                int left = LotMPlayer.MENTOR_RITUAL_TARGET - modPlayer.mentorRitualProgress;
                Main.NewText($"仪式未完成：利用“混乱”误导并致死 {left} 个强大的灵魂...", 0, 255, 127);
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                modPlayer.currentMarauderSequence = 3;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你感觉思维变得冰冷而滑腻，规则在你眼中出现了无数漏洞...", 0, 255, 127);
                Main.NewText("晋升成功：序列3 欺瞒导师！", 255, 215, 0);
                return true;
            }
            return null;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Ectoplasm, 10)       // 亵渎祭司核心
                .AddIngredient(ItemID.PutridScent, 1)      // 沼泽相关的腐臭
                .AddIngredient(ItemID.VialofVenom, 5)      // 血液/毒液
                .AddIngredient(ItemID.JungleSpores, 20)    // 菌毯/丛林
                .AddIngredient(ItemID.GoldBar, 5)          // 黄金
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}