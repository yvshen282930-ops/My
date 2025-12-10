using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;
using zhashi.Content.Buffs; // 引用 Buff

namespace zhashi.Content.Items.Potions.Fool
{
    public class AttendantPotion : LotMItem
    {
        public override string Pathway => "Fool";
        public override int RequiredSequence => 2;

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
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(platinum: 50);
        }

        // 【核心修改】检测转化 NPC 的数量
        public override bool CanUseItem(Player player)
        {
            int convertedCount = 0;

            // 遍历所有 NPC
            foreach (NPC npc in Main.ActiveNPCs)
            {
                // 如果是城镇NPC，且拥有"秘偶化"Buff
                if (npc.townNPC && npc.HasBuff(ModContent.BuffType<MarionetteTownNPCBuff>()))
                {
                    // 距离判定 (可选，必须在身边才算)
                    if (npc.Distance(player.Center) < 2000f)
                    {
                        convertedCount++;
                    }
                }
            }

            // 要求：10个
            if (convertedCount < 10)
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
                modPlayer.currentFoolSequence = 1;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("城镇在你的意志下起舞，诡秘的帷幕已然拉开...", 148, 0, 211);
                Main.NewText("晋升成功！序列1：诡秘侍者！(天使之王)", 255, 0, 255);
            }
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            int convertedCount = 0;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.townNPC && npc.HasBuff(ModContent.BuffType<MarionetteTownNPCBuff>()) && npc.Distance(Main.LocalPlayer.Center) < 2000f)
                {
                    convertedCount++;
                }
            }

            string c = convertedCount >= 10 ? "00FF00" : "FF0000";
            tooltips.Add(new TooltipLine(Mod, "Ritual", $"[c/{c}:仪式要求：转化NPC为秘偶 ({convertedCount}/10)]"));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)

                // 【核心修复】将 LuminiteBar 改为 LunarBar
                .AddIngredient(ItemID.LunarBar, 20)       // 诡秘侍者特性 (夜明锭)

                // 九种灵界特产
                .AddIngredient(ItemID.Ectoplasm, 5)       // 灵气
                .AddIngredient(ItemID.SoulofFlight, 5)    // 飞翔之魂
                .AddIngredient(ItemID.SoulofLight, 5)     // 光明之魂
                .AddIngredient(ItemID.SoulofNight, 5)     // 暗影之魂
                .AddIngredient(ItemID.UnicornHorn, 1)     // 独角兽角
                .AddIngredient(ItemID.PixieDust, 10)      // 精灵尘
                .AddIngredient(ItemID.CrystalShard, 5)    // 水晶碎块
                .AddIngredient(ItemID.CursedFlame, 5)     // 咒火
                .AddIngredient(ItemID.Ichor, 5)           // 灵液

                .AddTile(TileID.LunarCraftingStation)     // 远古操纵机
                .Register();
        }
    }
}