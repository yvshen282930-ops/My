using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Sun
{
    public class JusticeMentorPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名字在 .hjson 中设置
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 36;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Cyan; // 青色 (序列3 圣者)
            Item.value = Item.sellPrice(platinum: 1);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                if (modPlayer.currentSunSequence == 4)
                {
                    // === 仪式判定 ===
                    // 1. 必须在白天 (正义属于阳光)
                    // 2. 必须持有“正义的证明” (这里用 复仇者徽章 AvengerEmblem 代表践行正义的决心)
                    // 3. 身上不能有邪恶 Debuff

                    bool isDay = Main.dayTime;
                    bool hasEmblem = player.HasItem(ItemID.AvengerEmblem);
                    bool isClean = !player.HasBuff(BuffID.Bleeding) && !player.HasBuff(BuffID.Poisoned);

                    if (isDay && hasEmblem && isClean)
                    {
                        modPlayer.currentSunSequence = 3;
                        Main.NewText("你确立了自己的秩序，与世界签订了契约...", 255, 215, 0);
                        Main.NewText("晋升圣者！序列3：正义导师。", 255, 215, 0);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);

                        // 圣者晋升特效：巨大的金色光柱
                        for (int i = 0; i < 100; i++)
                        {
                            Vector2 speed = new Vector2(0, -10f).RotatedByRandom(0.5f);
                            Dust d = Dust.NewDustPerfect(player.Center, DustID.GoldFlame, speed, 0, default, 4f);
                            d.noGravity = true;
                        }
                        return true;
                    }
                    else
                    {
                        if (!isDay) Main.NewText("仪式失败：正义需要在阳光下见证。", 150, 150, 150);
                        else if (!hasEmblem) Main.NewText("仪式失败：你缺少践行正义的证明 (需持有复仇者徽章)。", 150, 150, 150);
                        else Main.NewText("仪式失败：你的身体不够纯净。", 150, 150, 150);
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
                // 主材料：炽光巨人/太阳神鸟 -> 日耀碎片 + 异能物质(Ectoplasm)
                .AddIngredient(ItemID.FragmentSolar, 15)
                .AddIngredient(ItemID.Ectoplasm, 10)
                // 辅助材料：万物礼赞者 -> 生命果 + 世纪之花掉落物
                .AddIngredient(ItemID.LifeFruit, 5)
                .AddIngredient(ItemID.SporeSac, 1) // 孢子囊 (代表植物怪物)
                .AddIngredient(ItemID.HallowedBar, 20) // 神圣锭
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}