using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Fool
{
    public class MarionettistPotion : LotMItem
    {
        public override string Pathway => "Fool";
        public override int RequiredSequence => 6; // 必须是无面人

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
            Item.rare = ItemRarityID.Pink; // 序列5级别
            Item.value = Item.sellPrice(gold: 20);
        }

        public override bool CanUseItem(Player player)
        {
            // 仪式检查：必须在海洋 (模拟美人鱼歌声)
            if (!player.ZoneBeach)
            {
                return false; // 不在海边不能喝
            }
            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.baseFoolSequence = 5; // 晋升序列5

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("你看见了无数根丝线，它们连接着万物...", 148, 0, 211);
                Main.NewText("晋升成功！序列5：秘偶大师！", 50, 255, 50);
            }
            return true;
        }

        // 添加条件提示
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            Player p = Main.LocalPlayer;
            string color = p.ZoneBeach ? "00FF00" : "FF0000";
            string status = p.ZoneBeach ? "满足" : "未满足";
            tooltips.Add(new TooltipLine(Mod, "Ritual", $"[c/{color}:仪式要求：在海洋环境服食 ({status})]"));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.SoulofFlight, 5)  // 古老怨灵粉尘
                .AddIngredient(ItemID.HallowedBar, 5)  // 石像鬼核心
                .AddIngredient(ItemID.RichMahogany, 5) // 龙纹树皮
                .AddIngredient(ItemID.Bone, 10)        // 怨灵残余
                .AddIngredient(ItemID.Lens, 2)         // 石像鬼眼睛
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}