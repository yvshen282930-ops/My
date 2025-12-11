using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using zhashi.Content; // [修复1] 必须引用 LotMPlayer 所在的命名空间

namespace zhashi.Content.Items
{
    public class BlasphemySlate : ModItem
    {
        // 存储当前石板的配方索引
        // -1 代表混沌状态（未观测/未锁定）
        public int recipeIndex = -1;

        // 配方数据库
        private static readonly List<(int level, string name, string content, Color color)> RecipeDatabase = new()
        {
            // --- 愚者途径 (紫色) ---
            (9, "序列9 占卜家", "水瓶 + 太阳花 + 铜/锡锭(3)", Color.MediumPurple),
            (8, "序列8 小丑", "水瓶 + 玫瑰 + 金/铂金锭(3) + 凝胶(10)", Color.MediumPurple),
            (7, "序列7 魔术师", "水瓶 + 烟雾弹(5) + 丝绸(10) + 陨石锭(5)", Color.MediumPurple),
            (6, "序列6 无面人", "水瓶 + 骨头(20) + 组织样本/暗影鳞片(10) + 变性药水", Color.MediumPurple),
            (5, "序列5 秘偶大师", "水瓶 + 蜘蛛牙(10) + 灵气(5) + 黑暗之魂(10)", Color.MediumPurple),
            (4, "序列4 诡法师", "水瓶 + 力量之魂(10) + 灵气(10) + 红毛桦(5) + 丝绸(5)", Color.MediumPurple),
            (3, "序列3 古代学者", "水瓶 + 日耀碎片(10) + 星云碎片(10) + 白霜结晶(3) + 灵气(10)", Color.MediumPurple),
            (2, "序列2 奇迹师", "水瓶 + 夜明锭(20) + 灵气(20) + 金表 + 坠落之星(10)", Color.MediumPurple),
            (1, "序列1 诡秘侍者", "水瓶 + 夜明锭(20) + 灵界特产(九种稀有材料聚合)", Color.MediumPurple),

            // --- 巨人途径 (金色) ---
            (9, "序列9 战士", "水瓶 + 太阳花 + 铁/铅锭(3)", Color.Gold),
            (8, "序列8 格斗家", "水瓶 + 魔矿/猩红锭(5) + 凝胶(50) + 丛林孢子(3)", Color.Gold),
            (7, "序列7 武器大师", "水瓶 + 暗影鳞片/组织样本(10) + 黑曜石(5) + 毒刺(3)", Color.Gold),
            (6, "序列6 黎明骑士", "水瓶 + 神圣锭(5) + 光明之魂(10) + 太阳花(5)", Color.Gold),
            (5, "序列5 守护者", "水瓶 + 叶绿锭(5) + 生命果(3) + 铁皮药水(5)", Color.Gold),
            (4, "序列4 猎魔者", "水瓶 + 甲虫壳(5) + 灵气(10) + 暗影之魂(15)", Color.Gold),
            (3, "序列3 银骑士", "水瓶 + 幽灵锭(10) + 蘑菇矿锭(10) + 灵气(5)", Color.Gold),
            (2, "序列2 荣耀者", "水瓶 + 月亮锭(10) + 日耀碎片(10) + 星云碎片(5)", Color.Gold),

            // --- 猎人途径 (红色) ---
            (9, "序列9 猎人", "水瓶 + 闪耀根 + 弓/枪", Color.IndianRed),
            (8, "序列8 挑衅者", "水瓶 + 腐肉/椎骨(5) + 铜/锡锭(5)", Color.IndianRed),
            (7, "序列7 纵火家", "水瓶 + 狱石锭(5) + 火焰花(3) + 炸弹(10)", Color.IndianRed),
            (6, "序列6 阴谋家", "水瓶 + 骨头(30) + 晶状体(5) + 书(1)", Color.IndianRed),
            (5, "序列5 收割者", "水瓶 + 恐惧之魂(10) + 鲨鱼鳍(5) + 非法枪械部件", Color.IndianRed),
            (4, "序列4 铁血骑士", "水瓶 + 视域之魂(10) + 狱石锭(20) + 活火块(20)", Color.IndianRed),
            (3, "序列3 战争主教", "水瓶 + 叶绿锭(15) + 战争药水 + 丛林蜥蜴砖(10)", Color.IndianRed),
            (2, "序列2 天气术士", "水瓶 + 天气符文(特殊掉落) + 灵气(10)", Color.IndianRed),
            (1, "序列1 征服者", "水瓶 + 征服者特性(特殊掉落) + 夜明锭(10)", Color.IndianRed),

            // --- 月亮途径 (深红) ---
            (9, "序列9 药师", "水瓶 + 太阳花 + 蘑菇(5)", Color.Crimson),
            (8, "序列8 驯兽师", "水瓶 + 丛林孢子(5) + 毒刺(2) + 鲨鱼牙项链", Color.Crimson),
            (7, "序列7 吸血鬼", "水瓶 + 蝙蝠翼(2) + 弱效治疗药水(5) + 红玉(3)", Color.Crimson),
            (6, "序列6 魔药教授", "水瓶 + 炼金台 + 咒火/灵液(10) + 玻璃瓶(10)", Color.Crimson),
            (5, "序列5 深红学者", "水瓶 + 暗影之魂(15) + 夜之钥匙 + 月光草(5)", Color.Crimson),
            (4, "序列4 巫王", "水瓶 + 破损蝙蝠翼 + 灵气(10) + 恐惧之魂(5)", Color.Crimson),
            (3, "序列3 召唤大师", "水瓶 + 真名拓本(特殊掉落) + 幽灵锭(10)", Color.Crimson),
            (2, "序列2 创生者", "水瓶 + 星旋碎片(10) + 星尘碎片(10) + 生命果(5)", Color.Crimson),
            (1, "序列1 美神", "水瓶 + 棱镜(光之女皇掉落) + 夜明锭(20) + 爱情药水", Color.Crimson),

            // --- 错误途径 (深蓝) ---
            (9, "序列9 偷盗者", "水瓶 + 毒刺 + 粉凝胶 + 蓝宝石", Color.DarkBlue),
        };

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
        }

        public override void SaveData(TagCompound tag) => tag["SlateRecipeIndex"] = recipeIndex;
        public override void LoadData(TagCompound tag) => recipeIndex = tag.GetInt("SlateRecipeIndex");

        public override void UpdateInventory(Player player)
        {
            if (recipeIndex == -1)
            {
                RollRecipeForPlayer(player);
            }
        }

        private void RollRecipeForPlayer(Player player)
        {
            // 确保 LotMPlayer 是 public class 且位于引用空间内
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 获取玩家当前最强的序列等级 (数值越小越强)
            // 注意：请确保这些字段在 LotMPlayer 中已定义且初始值为高数值(如10或99)，否则默认0会导致直接判定为序列1
            int currentSeq = modPlayer.currentSequence;
            if (modPlayer.currentFoolSequence < currentSeq) currentSeq = modPlayer.currentFoolSequence;
            if (modPlayer.currentHunterSequence < currentSeq) currentSeq = modPlayer.currentHunterSequence;
            if (modPlayer.currentMoonSequence < currentSeq) currentSeq = modPlayer.currentMoonSequence;

            int targetLevel = currentSeq - 1;
            if (targetLevel < 1) targetLevel = 1;

            List<int> weightedPool = new List<int>();

            for (int i = 0; i < RecipeDatabase.Count; i++)
            {
                int itemLevel = RecipeDatabase[i].level;

                if (itemLevel == targetLevel)
                {
                    for (int k = 0; k < 50; k++) weightedPool.Add(i);
                }
                else if (itemLevel == targetLevel + 1)
                {
                    for (int k = 0; k < 10; k++) weightedPool.Add(i);
                }
                else if (itemLevel == targetLevel - 1)
                {
                    for (int k = 0; k < 5; k++) weightedPool.Add(i);
                }
                else
                {
                    weightedPool.Add(i);
                }
            }

            recipeIndex = weightedPool[Main.rand.Next(weightedPool.Count)];
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine lore = new TooltipLine(Mod, "Lore", "这块古老的石板上刻满了亵渎的文字...");
            lore.OverrideColor = new Color(150, 150, 150);
            tooltips.Add(lore);

            if (recipeIndex == -1)
            {
                int tempIndex = Main.rand.Next(RecipeDatabase.Count);
                var tempData = RecipeDatabase[tempIndex];

                TooltipLine chaosLine = new TooltipLine(Mod, "Chaos",
                    $"[混沌状态] 拾取以观测...\n>>> {tempData.name} ???");
                chaosLine.OverrideColor = Color.Lerp(Color.Gray, tempData.color, 0.5f);
                tooltips.Add(chaosLine);
            }
            else if (recipeIndex >= 0 && recipeIndex < RecipeDatabase.Count)
            {
                var data = RecipeDatabase[recipeIndex];

                TooltipLine titleLine = new TooltipLine(Mod, "RecipeTitle", $"\n>>> {data.name} <<<");
                titleLine.OverrideColor = data.color;
                tooltips.Add(titleLine);

                TooltipLine contentLine = new TooltipLine(Mod, "RecipeContent", $"所需材料: {data.content}");
                contentLine.OverrideColor = Color.White;
                tooltips.Add(contentLine);
            }
        }

        // [修复2] 加回合成配方，否则无法制作
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