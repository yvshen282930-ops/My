using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items.Accessories
{
    public class DemonessCard : BlasphemyCardBase
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
            mp.isDemonessCardEquipped = true;

            // [致命一击]
            player.GetCritChance(DamageClass.Generic) += 20f;

            // [魔女毒素/寒冰]
            player.frostBurn = true;
            player.GetDamage(DamageClass.Generic) += 0.10f;

            // [魅惑舞步]
            player.moveSpeed += 0.4f;
            player.accRunSpeed += 2f;

            // [痛苦之刺]
            player.starCloakItem = Item;

            // [魅惑]
            // 【核心修复】brainOfConfusion 已改为 brainOfConfusionItem
            // 这样写才能正确触发混乱之脑的闪避和混乱效果
            player.brainOfConfusionItem = Item;
        }
    }
}