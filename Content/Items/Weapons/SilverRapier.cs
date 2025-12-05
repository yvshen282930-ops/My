using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Weapons
{
    public class SilverRapier : ModItem
    {
        // 寿命计时器
        public int lifeTime = 0;

        public override void SetDefaults()
        {
            Item.damage = 180;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useTurn = true;
            Item.knockBack = 4;
            Item.value = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.None;
        }

        // 物品栏内自动销毁逻辑
        public override void UpdateInventory(Player player)
        {
            lifeTime++;
            if (lifeTime >= 600) // 10秒
            {
                Item.TurnToAir();
                Main.NewText("银白细剑消散了...", 192, 192, 192);
                SoundEngine.PlaySound(SoundID.Shatter, player.position);
            }
        }

        // 【关键修复】合成扣除灵性逻辑
        public override void OnCreated(ItemCreationContext context)
        {
            // 安全检查：如果在主菜单加载，或者玩家无效，直接退出，防止蓝屏崩溃！
            if (Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active)
                return;

            var player = Main.LocalPlayer;

            // 安全尝试获取 ModPlayer，如果获取失败也不要报错
            if (player.TryGetModPlayer<LotMPlayer>(out var modPlayer))
            {
                int cost = (int)(modPlayer.spiritualityCurrent / 2);
                modPlayer.spiritualityCurrent -= cost;

                Main.NewText($"具现银白细剑消耗了 {cost} 点灵性！", 0, 255, 255);
            }
        }

        public override bool CanUseItem(Player player)
        {
            // 使用安全获取
            if (player.TryGetModPlayer<LotMPlayer>(out var modPlayer))
            {
                if (modPlayer.currentSequence > 3) return false;

                int cost = 0;
                if (player.altFunctionUse == 2) cost = 1000;
                else cost = 100;

                // 灵性不足自毁
                if (modPlayer.spiritualityCurrent < cost)
                {
                    SoundEngine.PlaySound(SoundID.Shatter, player.position);
                    Main.NewText("灵性枯竭... 银白细剑崩解了！", 255, 50, 50);
                    for (int i = 0; i < 30; i++) Dust.NewDust(player.position, player.width, player.height, DustID.Silver, 0, 0, 0, default, 2f);
                    Item.TurnToAir();
                    return false;
                }

                modPlayer.TryConsumeSpirituality(cost);

                // 右键瞬移
                if (player.altFunctionUse == 2)
                {
                    Vector2 targetPos = Main.MouseWorld;
                    if (player.Distance(targetPos) < 1000f && Collision.CanHit(player.position, player.width, player.height, targetPos, player.width, player.height))
                    {
                        for (int i = 0; i < 20; i++) Dust.NewDust(player.position, player.width, player.height, DustID.Silver, 0, 0, 0, default, 2f);
                        player.Teleport(targetPos, 1);
                        for (int i = 0; i < 30; i++) { Vector2 speed = Main.rand.NextVector2Circular(5f, 5f); Dust.NewDustPerfect(player.Center, DustID.Silver, speed, 0, default, 2f); }
                        SoundEngine.PlaySound(SoundID.Item71, player.position);

                        foreach (NPC npc in Main.npc)
                        {
                            if (npc.active && !npc.friendly && npc.Distance(player.Center) < 200f)
                            {
                                player.ApplyDamageToNPC(npc, Item.damage * 2, 5f, player.direction, false);
                                npc.AddBuff(BuffID.Frostburn, 300);
                            }
                        }
                    }
                    else return false;
                }
                return true;
            }
            return false;
        }

        public override bool AltFunctionUse(Player player) { return true; }

        public override void AddRecipes()
        {
            // 使用土块作为免费材料
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 1)
                .Register();
        }
    }
}