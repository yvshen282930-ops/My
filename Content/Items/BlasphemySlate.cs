using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace zhashi.Content.Items
{
    public class BlasphemySlate : ModItem
    {
        // 【修改】删除了借用贴图的代码，现在它会自动寻找同名的 png 图片
        // 请确保 zhashi/Content/Items/BlasphemySlate.png 存在

        public override void SetStaticDefaults()
        {
            // DisplayName 和 Tooltip 在 hjson 里设置，或者由 ModifyTooltips 覆盖
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Red;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 1. 传说描述
            TooltipLine lore = new TooltipLine(Mod, "Lore", "这块古老的石板上刻满了亵渎的文字，\n记录着通往神座的阶梯...");
            lore.OverrideColor = new Color(150, 150, 150);
            tooltips.Add(lore);

            // 2. 配方列表 (金色)
            string recipes = "";
            recipes += "\n[序列9 战士]: 水瓶 + 太阳花 + 铁/铅锭(3)";
            recipes += "\n[序列8 格斗家]: 水瓶 + 魔矿/猩红锭(5) + 凝胶(50) + 丛林孢子(3)";
            recipes += "\n[序列7 武器大师]: 水瓶 + 暗影鳞片/组织样本(10) + 黑曜石(5) + 毒刺(3)";
            recipes += "\n[序列6 黎明骑士]: 水瓶 + 神圣锭(5) + 光明之魂(10) + 太阳花(5)";
            recipes += "\n[序列5 守护者]: 水瓶 + 叶绿锭(5) + 生命果(3) + 铁皮药水(5)";
            recipes += "\n[序列4 猎魔者]: 水瓶 + 甲虫壳(5) + 灵气(10) + 暗影之魂(15)";
            recipes += "\n[序列3 银骑士]: 水瓶 + 幽灵锭(10) + 蘑菇矿锭(10) + 灵气(5) (需远古操纵机)";
            recipes += "\n[序列2 荣耀者]: 水瓶 + 月亮锭(10) + 日耀碎片(10) + 星云碎片(5) (需远古操纵机)";

            TooltipLine recipeLine = new TooltipLine(Mod, "Recipes", recipes);
            recipeLine.OverrideColor = new Color(255, 215, 0);
            tooltips.Add(recipeLine);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 10)
                .AddIngredient(ItemID.Book, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}