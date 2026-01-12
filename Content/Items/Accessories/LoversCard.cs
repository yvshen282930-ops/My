using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content; // 确保引用了 LotMPlayer
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
            Item.rare = ItemRarityID.Red; // 红色稀有度
            Item.value = Item.sellPrice(platinum: 5); // 价值
            Item.maxStack = 1;
        }

        public override void SafeUpdateAccessory(Player player, LotMPlayer mp, bool hideVisual)
        {

            mp.isLoversCardEquipped = true;

            player.buffImmune[BuffID.Confused] = true; // 免疫困惑
            player.aggro -= 400; // 极大幅度降低仇恨（难以被敌人锁定）

            if (mp.baseMarauderSequence <= 9)
            {

                player.GetArmorPenetration(DamageClass.Generic) += 25;
                // 增加暴击率，模拟“精准打击弱点”
                player.GetCritChance(DamageClass.Generic) += 12;

                player.manaMagnet = true; // 自带吸魔效果
                player.goldRing = true;   // 自带金币戒指效果
                Player.defaultItemGrabRange += 200; // 增加拾取距离 (原版约为20-40，+200非常远)

                player.blackBelt = true;
                player.endurance += 0.10f; // 10% 减伤

                player.moveSpeed += 0.25f; // 移动速度 +25%
                player.accRunSpeed += 3f;  // 最大奔跑速度提升

                player.lifeRegen += 4;
                player.manaRegenDelayBonus += 1; // 减少回魔延迟
            }
        }
    }
}