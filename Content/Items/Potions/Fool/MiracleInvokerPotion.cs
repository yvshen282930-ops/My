using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Fool
{
    public class MiracleInvokerPotion : LotMItem
    {
        public override string Pathway => "Fool";
        public override int RequiredSequence => 3; // 必须是古代学者

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
            Item.rare = ItemRarityID.Red; // 天使级 (红色)
            Item.value = Item.sellPrice(platinum: 20);
        }

        public override bool CanUseItem(Player player)
        {
            // 仪式：在具有历史气息的地方 (地牢)
            if (!player.ZoneDungeon)
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
                modPlayer.baseFoolSequence = 2; // 晋升序列2

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("你向命运许愿，命运回应了你...", 255, 255, 0);
                Main.NewText("晋升成功！序列2：奇迹师！(天使)", 255, 215, 0);
            }
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            bool inDungeon = Main.LocalPlayer.ZoneDungeon;
            string c = inDungeon ? "00FF00" : "FF0000";
            tooltips.Add(new TooltipLine(Mod, "Ritual", $"[c/{c}:仪式要求：重现历史 (在地牢服食)]"));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.LunarOre, 20)       // 乌黯魔狼 (夜明矿)
                .AddIngredient(ItemID.Ectoplasm, 20)      // 奇迹特性
                .AddIngredient(ItemID.GoldWatch, 1)       // 时之虫
                .AddIngredient(ItemID.FallenStar, 10)     // 星之虫
                .AddTile(TileID.LunarCraftingStation)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}