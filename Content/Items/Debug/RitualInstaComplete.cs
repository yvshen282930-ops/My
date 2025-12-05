using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content; // 引用玩家数据

namespace zhashi.Content.Items.Debug
{
    public class RitualInstaComplete : ModItem
    {
        // 借用原版秒表贴图
        public override string Texture => "Terraria/Images/Item_" + ItemID.Stopwatch;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.HoldUp; // 举起动作
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.rare = ItemRarityID.Red; // 红色稀有度，代表开发者物品
            Item.consumable = false; // 无限使用
            Item.value = 0;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // === 一键拉满所有仪式进度 ===

            // 1. 巨人途径 序列5 守护者 (承伤)
            modPlayer.guardianRitualProgress = LotMPlayer.GUARDIAN_RITUAL_TARGET;

            // 2. 巨人途径 序列4 猎魔者 (击杀红魔鬼)
            modPlayer.demonHunterRitualProgress = LotMPlayer.DEMON_HUNTER_RITUAL_TARGET;

            // 3. 猎人途径 序列4 铁血骑士 (统率击杀)
            modPlayer.ironBloodRitualProgress = LotMPlayer.IRON_BLOOD_RITUAL_TARGET;

            // 提示信息
            Main.NewText("【测试】所有晋升仪式已强制完成！", 0, 255, 0);
            Main.NewText("巨人序列5/4，猎人序列4 均已达标。", 0, 255, 0);

            // 播放音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);

            return true;
        }

        // 不写合成表，只能通过作弊模组拿取
    }
}