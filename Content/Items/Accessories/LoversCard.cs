using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
// 引用你的基类命名空间
using zhashi.Content.Items;

namespace zhashi.Content.Items.Accessories
{
    public class LoversCard : BlasphemyCardBase
    {
        public override void SafeSetDefaults()
        {
            Item.width = 28;
            Item.height = 34;
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(platinum: 5);
            Item.maxStack = 1;
        }

        public override void SafeUpdateAccessory(Player player, LotMPlayer mp, bool hideVisual)
        {
            mp.isLoversCardEquipped = true;

            // === 通用效果：反占卜 (Anti-Divination) ===
            // 任何途径的人戴上都能生效
            // 效果：免疫精神干扰(困惑)，并难以被敌人锁定(降低仇恨)
            player.buffImmune[BuffID.Confused] = true;
            player.aggro -= 400; // 大幅降低敌人仇恨

            // === 专属神性加成 ===
            // 只有【错误途径】且达到【序列5 (混乱导师)】及以上才能触发
            if (mp.baseErrorSequence <= 5)
            {
                // [Bug 利用]：极高的护甲穿透
                player.GetArmorPenetration(DamageClass.Generic) += 25;

                // [精准打击]：增加 12% 暴击率
                player.GetCritChance(DamageClass.Generic) += 12;

                // [窃取与距离]：极大增加物品拾取范围
                player.manaMagnet = true; // 吸星星
                player.goldRing = true;   // 吸金币
                Player.defaultItemGrabRange += 160;

                // [欺诈命运]：获得忍者黑带的闪避效果
                player.blackBelt = true;

                // [时间加速]：移动速度大幅提升
                player.moveSpeed += 0.20f;
                player.accRunSpeed += 2f;

                // [逻辑扭曲]：减伤
                player.endurance += 0.10f;

                // [时之虫]：生命回复
                player.lifeRegen += 4;
            }
        }

        // === 已删除 AddRecipes，使其无法合成 ===
    }
}