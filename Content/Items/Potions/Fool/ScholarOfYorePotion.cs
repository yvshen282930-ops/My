using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Fool
{
    public class ScholarOfYorePotion : LotMItem
    {
        public override string Pathway => "Fool";
        public override int RequiredSequence => 4; // 必须是诡法师

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
            Item.rare = ItemRarityID.Cyan; // 圣者级 (青色/彩虹)
            Item.value = Item.sellPrice(platinum: 5);
        }

        public override bool CanUseItem(Player player)
        {
            // 仪式：脱离现实 (处于太空层 Space)
            // 泰拉瑞亚太空层一般是 Y < 200 (小地图) 或更低，ZoneSkyHeight 判断比较准
            if (!player.ZoneSkyHeight)
            {
                return false;
            }
            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentFoolSequence = 3; // 晋升序列3

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("你仿佛成为了历史的一部分，时间在你眼中不再是直线...", 0, 255, 255);
                Main.NewText("晋升成功！序列3：古代学者！(圣者)", 255, 215, 0);
            }
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            bool inSpace = Main.LocalPlayer.ZoneSkyHeight;
            string c = inSpace ? "00FF00" : "FF0000";
            tooltips.Add(new TooltipLine(Mod, "Ritual", $"[c/{c}:仪式要求：脱离现实 (在太空层服食)]"));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.FragmentSolar, 10)  // 福根之犬
                .AddIngredient(ItemID.FragmentNebula, 10) // 雾之魔狼
                .AddIngredient(ItemID.FrostCore, 3)       // 白霜结晶
                .AddIngredient(ItemID.Ectoplasm, 10)      // 历史记录
                .AddIngredient(ItemID.Book, 5)            // 历史书
                .AddTile(TileID.LunarCraftingStation)     // 远古操纵机
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}