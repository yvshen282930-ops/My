using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions
{
    public class GuardianPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.buyPrice(gold: 15);
        }

        // 1. 动态修改物品说明：实时显示你的仪式进度
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<LotMPlayer>();

            // 只有当玩家是序列6时，才显示仪式进度
            if (modPlayer.baseSequence == 6)
            {
                // 计算颜色：完成显示绿色，未完成显示红色
                string statusColor = (modPlayer.guardianRitualProgress >= LotMPlayer.GUARDIAN_RITUAL_TARGET) ? "00FF00" : "FF0000";
                string progressText = $"[c/{statusColor}:仪式进度: {modPlayer.guardianRitualProgress} / {LotMPlayer.GUARDIAN_RITUAL_TARGET}]";

                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 需在城镇NPC附近(50格内)承受伤害以履行守护之责。"));
                tooltips.Add(new TooltipLine(Mod, "RitualProgress", progressText));
            }
        }

        // 2. 核心判断：能否使用物品？
        // 如果这里返回 false，你连左键动作都做不出来，药水也不会消耗
        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 如果是序列6，必须检查仪式进度
            if (modPlayer.baseSequence == 6)
            {
                if (modPlayer.guardianRitualProgress < LotMPlayer.GUARDIAN_RITUAL_TARGET)
                {
                    // 仪式未完成，禁止使用，并弹出提示
                    // 只有玩家自己能看到这个提示
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Main.NewText($"你还未理解守护的重量... (进度: {modPlayer.guardianRitualProgress}/{LotMPlayer.GUARDIAN_RITUAL_TARGET})", 255, 50, 50);
                        Main.NewText("提示：请在任何城镇NPC附近承受伤害来增加进度。", 200, 200, 200);
                    }
                    return false; // 禁止使用！
                }
            }

            // 其他情况（比如不是序列6，或者是更高序列）交给 UseItem 去处理文本提示
            return true;
        }

        // 3. 物品使用后的效果 (晋升逻辑)
        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.baseSequence == 6)
            {
                // 因为 CanUseItem 已经检查过进度了，能进到这里说明进度一定达标了
                modPlayer.baseSequence = 5;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你的皮肤泛起金属的光泽，你发誓守护这一切...", 255, 215, 0);
                Main.NewText("晋升成功：序列5 守护者！", 255, 215, 0);
                Main.NewText("获得能力：【守护姿态】 (按住 X 键开启)", 255, 255, 255);
                Main.NewText("被动能力：免疫幻觉，攻击无视大量防御，并在受到攻击时保护队友。", 200, 200, 200);
                return true;
            }
            else if (modPlayer.baseSequence > 6)
            {
                Main.NewText("你的晨曦之力还不够纯粹，无法承担守护的重任。", 200, 50, 50);
                return true; // 消耗掉药水作为惩罚
            }
            else
            {
                Main.NewText("你早已知晓守护的真谛。", 200, 200, 200);
                return true; // 消耗掉
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.ChlorophyteBar, 5)
                .AddIngredient(ItemID.LifeFruit, 3)
                .AddIngredient(ItemID.IronskinPotion, 5)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}