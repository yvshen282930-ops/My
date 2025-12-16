using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Fool
{
    public class BizarroSorcererPotion : LotMItem
    {
        public override string Pathway => "Fool";
        public override int RequiredSequence => 5; // 必须是秘偶大师

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
            Item.rare = ItemRarityID.Yellow; // 半神级
            Item.value = Item.sellPrice(platinum: 1);
        }

        public override bool CanUseItem(Player player)
        {
            // 仪式判定：
            // 1. 谋杀半神：必须已经击败世纪之花 (Plantera)
            // 2. 观众：周围必须有至少 10 个 NPC (城镇NPC或敌人均可)

            bool bossDowned = NPC.downedPlantBoss;

            int audienceCount = 0;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.Distance(player.Center) < 2000f) // 屏幕范围内
                {
                    audienceCount++;
                }
            }

            // 自己不算，至少要有3个"观众"
            if (!bossDowned || audienceCount < 10)
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
                modPlayer.currentFoolSequence = 4; // 晋升序列4

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("你分裂了，你重组了，你成为了诡异的主宰...", 128, 0, 128);
                Main.NewText("晋升成功！序列4：诡法师！(半神)", 255, 215, 0);
            }
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            bool bossDowned = NPC.downedPlantBoss;
            int audienceCount = 0;
            foreach (NPC npc in Main.ActiveNPCs) if (npc.Distance(Main.LocalPlayer.Center) < 2000f) audienceCount++;

            string c1 = bossDowned ? "00FF00" : "FF0000";
            string c2 = audienceCount >= 3 ? "00FF00" : "FF0000";

            tooltips.Add(new TooltipLine(Mod, "Ritual1", $"[c/{c1}:仪式条件1：谋杀半神 (击败世纪之花)]"));
            tooltips.Add(new TooltipLine(Mod, "Ritual2", $"[c/{c2}:仪式条件2：观众在场 (周围生物>10) ({audienceCount})]"));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.SoulofMight, 10)  // 诡术邪怪主眼 (力量之魂)
                .AddIngredient(ItemID.Ectoplasm, 10)    // 灵界掠夺者 (灵气/花后材料)
                .AddIngredient(ItemID.RichMahogany, 5)  // 红毛桦
                .AddIngredient(ItemID.Vine, 1)          // 葡萄藤
                .AddIngredient(ItemID.Silk, 5)          // 橡皮面具 (丝绸)
                .AddTile(TileID.MythrilAnvil)           // 秘银砧
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}