using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace zhashi.Content.Items
{
    public class TestSummon : ModItem
    {
        // 借用原版眼球贴图，防止还要画图
        public override string Texture => "Terraria/Images/Item_" + ItemID.SuspiciousLookingEye;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("测试召唤物");
            // Tooltip.SetDefault("召唤那个娱乐用的Boss\n用 1 个土块制作");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 20;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false; // 无限使用，不消耗
        }

        public override bool? UseItem(Player player)
        {
            // 播放吼叫声
            SoundEngine.PlaySound(SoundID.Roar, player.position);

            // 直接在玩家头顶生成
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<NPCs.TestBoss>());

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 1) // 1个土块
                .Register();
        }
    }
}