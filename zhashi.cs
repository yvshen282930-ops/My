using Terraria.ModLoader;
using System.IO;
using Terraria.ID;
using Terraria;
using zhashi.Content;
using zhashi.Content.Buffs; // 确保引用了 Buff 所在的命名空间

namespace zhashi
{
    // 将枚举放在类外是好习惯
    public enum LotMNetMsg : byte
    {
        PlayerSync,
        ApplySunSuppression
    }

    public class zhashi : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            LotMNetMsg msgType = (LotMNetMsg)reader.ReadByte();

            switch (msgType)
            {
                case LotMNetMsg.PlayerSync:
                    byte playernumber = reader.ReadByte();

                    // 安全检查
                    if (playernumber >= Main.maxPlayers || !Main.player[playernumber].active)
                        return;

                    LotMPlayer modPlayer = Main.player[playernumber].GetModPlayer<LotMPlayer>();


                    // 1. 基础序列等级
                    modPlayer.currentSequence = reader.ReadInt32();
                    modPlayer.currentMarauderSequence = reader.ReadInt32();
                    modPlayer.currentFoolSequence = reader.ReadInt32();
                    modPlayer.currentHunterSequence = reader.ReadInt32();
                    modPlayer.currentMoonSequence = reader.ReadInt32();
                    modPlayer.currentSunSequence = reader.ReadInt32(); // 修正：按通常逻辑放在灵性之前

                    // 2. 灵性值 (你之前的代码里读了两次，这里保留一次)
                    modPlayer.spiritualityCurrent = reader.ReadSingle();

                    // 3. 寄生相关数据
                    modPlayer.isParasitizing = reader.ReadBoolean();
                    modPlayer.parasiteTargetIndex = reader.ReadInt32();
                    modPlayer.parasiteIsTownNPC = reader.ReadBoolean();
                    modPlayer.parasiteIsPlayer = reader.ReadBoolean();

                    // 4. 进度条/仪式数据
                    modPlayer.purificationProgress = reader.ReadInt32();
                    modPlayer.judgmentProgress = reader.ReadInt32();
                    modPlayer.ironBloodRitualProgress = reader.ReadInt32();

                    modPlayer.spiritWorms = reader.ReadInt32(); // 资源

                    // 愚者
                    modPlayer.isSpiritVisionActive = reader.ReadBoolean();
                    modPlayer.isSpiritForm = reader.ReadBoolean();
                    modPlayer.graftingMode = reader.ReadInt32(); // 注意是 Int
                    modPlayer.spiritThreadTargetIndex = reader.ReadInt32(); // 注意是 Int

                    // 月亮
                    modPlayer.isTamingActive = reader.ReadBoolean();
                    modPlayer.isVampireWings = reader.ReadBoolean();
                    modPlayer.isBatSwarm = reader.ReadBoolean();
                    modPlayer.isMoonlightized = reader.ReadBoolean();
                    modPlayer.isFullMoonActive = reader.ReadBoolean();
                    modPlayer.isCreationDomain = reader.ReadBoolean();

                    // 错误
                    modPlayer.isDeceitDomainActive = reader.ReadBoolean();
                    modPlayer.isTimeClockActive = reader.ReadBoolean();

                    // 猎人
                    modPlayer.isFireForm = reader.ReadBoolean();
                    modPlayer.isCalamityGiant = reader.ReadBoolean();
                    modPlayer.isFlameCloakActive = reader.ReadBoolean();

                    // 巨人
                    modPlayer.isGuardianStance = reader.ReadBoolean();
                    modPlayer.isMercuryForm = reader.ReadBoolean();
                    modPlayer.dawnArmorActive = reader.ReadBoolean();

                    // 太阳
                    modPlayer.isSinging = reader.ReadBoolean();
                    modPlayer.isSunMessenger = reader.ReadBoolean();

                    // 服务器转发逻辑
                    if (Main.netMode == NetmodeID.Server)
                    {
                        // 服务器收到后，转发给除了发送者以外的所有人
                        modPlayer.SyncPlayer(-1, whoAmI, false);
                    }
                    break;

                case LotMNetMsg.ApplySunSuppression:
                    int targetWho = reader.ReadByte();
                    int duration = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        Player target = Main.player[targetWho];
                        if (target.active)
                        {
                            // 假设 SunSuppressionDebuff 在 Content.Buffs 命名空间下
                            // 如果报错找不到类型，请检查类名是否正确
                            target.AddBuff(ModContent.BuffType<SunSuppressionDebuff>(), duration);
                        }
                    }
                    break;
            }
        }
    }
}