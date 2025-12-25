using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Sun
{
    public class WhiteAngelPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Purple; // 紫色 (序列1 天使之王)
            Item.value = Item.sellPrice(platinum: 20);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                if (modPlayer.currentSunSequence == 2)
                {
                    // === 仪式：百万信仰与神圣秩序 ===

                    // 1. 时间：必须是正午 (12:00 PM - 2:00 PM)
                    bool isNoon = Main.dayTime && Main.time >= 27000 && Main.time <= 36000;

                    // 2. 信徒：至少 15 个城镇 NPC
                    int townNPCs = 0;
                    for (int i = 0; i < Main.maxNPCs; i++) { if (Main.npc[i].active && Main.npc[i].townNPC) townNPCs++; }
                    bool hasFollowers = townNPCs >= 15;

                    // 3. 象征物：泰拉刃 / 终极棱镜 / 彩虹猫之刃
                    bool hasSymbol = player.HasItem(ItemID.TerraBlade) || player.HasItem(ItemID.LastPrism) || player.HasItem(ItemID.Meowmere);

                    if (isNoon && hasFollowers && hasSymbol)
                    {
                        modPlayer.currentSunSequence = 1;
                        Main.NewText("百万信徒的祈祷声汇聚成洪流...", 255, 255, 255);
                        Main.NewText("你化身为纯白的秩序，神圣之国降临人间！", 255, 255, 0);
                        Main.NewText("晋升天使之王！序列1：纯白天使。", 255, 255, 255);

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);

                        // 晋升特效
                        for (int i = 0; i < 300; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2Circular(30, 30);
                            Dust d = Dust.NewDustPerfect(player.Center, DustID.WhiteTorch, speed, 0, default, 5f);
                            d.noGravity = true;
                        }
                        return true;
                    }
                    else
                    {
                        if (!isNoon) Main.NewText("仪式失败：必须在烈日当空的正午进行 (12:00-14:00)。", 150, 150, 150);
                        else if (!hasFollowers) Main.NewText($"仪式失败：你的信徒不够多 ({townNPCs}/15)。", 150, 150, 150);
                        else Main.NewText("仪式失败：背包中缺少秩序或光明的圣物 (泰拉刃/终极棱镜/彩虹猫之刃)。", 150, 150, 150);
                        return false;
                    }
                }
                else
                {
                    Main.NewText("你的灵性不足以容纳这份权柄。", 150, 150, 150);
                    return false;
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                // 主材料
                .AddIngredient(ItemID.FragmentSolar, 50)
                .AddIngredient(ItemID.EmpressButterfly, 1) // 七彩草蛉
                .AddIngredient(ItemID.HallowedBar, 50)     // 神圣锭

                // 【新增】夜明锭 50个
                .AddIngredient(ItemID.LunarBar, 50)

                // 辅助材料
                .AddIngredient(ItemID.LihzahrdPowerCell, 5) // 能源核心
                .AddIngredient(ItemID.SoulofLight, 50)      // 极致的光
                .AddIngredient(ItemID.Ectoplasm, 30)        // 灵性
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}