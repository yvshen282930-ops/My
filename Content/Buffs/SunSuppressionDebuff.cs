using Terraria;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Buffs
{
    public class SunSuppressionDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // 这是一个负面效果
            Main.pvpBuff[Type] = true; // 允许在 PVP 中生效
            Main.buffNoSave[Type] = false; // 退出重进依然存在 (因为持续时间很长)

            // 它是“诅咒”，不能被普通护士清除 (可选)
            Terraria.ID.BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 遍历所有途径，如果已经开启(序列<10)，则强制+1 (变弱)
            // 确保不会超过 10 (凡人)

            // 巨人
            if (modPlayer.currentSequence < 10)
                modPlayer.currentSequence = (modPlayer.currentSequence + 1 > 10) ? 10 : modPlayer.currentSequence + 1;

            // 猎人
            if (modPlayer.currentHunterSequence < 10)
                modPlayer.currentHunterSequence = (modPlayer.currentHunterSequence + 1 > 10) ? 10 : modPlayer.currentHunterSequence + 1;

            // 月亮
            if (modPlayer.currentMoonSequence < 10)
                modPlayer.currentMoonSequence = (modPlayer.currentMoonSequence + 1 > 10) ? 10 : modPlayer.currentMoonSequence + 1;

            // 愚者
            if (modPlayer.currentFoolSequence < 10)
                modPlayer.currentFoolSequence = (modPlayer.currentFoolSequence + 1 > 10) ? 10 : modPlayer.currentFoolSequence + 1;

            // 错误
            if (modPlayer.currentMarauderSequence < 10)
                modPlayer.currentMarauderSequence = (modPlayer.currentMarauderSequence + 1 > 10) ? 10 : modPlayer.currentMarauderSequence + 1;

            // 太阳
            if (modPlayer.currentSunSequence < 10)
                modPlayer.currentSunSequence = (modPlayer.currentSunSequence + 1 > 10) ? 10 : modPlayer.currentSunSequence + 1;
        }
    }
}