using Terraria.ModLoader;
using System.IO;
using Terraria.ID;
using Terraria;
using zhashi.Content;
using zhashi.Content.UI;
using zhashi.Content.Buffs;

namespace zhashi
{
    // 定义网络消息类型
    public enum LotMNetMsg : byte
    {
        PlayerSync = 0,
        ApplySunSuppression = 1,
        SyncFavorability = 2  // <--- 新增的好感度同步 ID
    }

    public class zhashi : Mod
    {
        public enum LotMNetMsg : byte
        {
            PlayerSync = 0,
            ApplySunSuppression = 1,
            SyncFavorability = 2
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            // 读取消息类型
            LotMNetMsg msgType = (LotMNetMsg)reader.ReadByte();

            switch (msgType)
            {
                // ==========================================================
                // Case 0: 玩家数据同步 (保持原样)
                // ==========================================================
                case LotMNetMsg.PlayerSync:
                    byte playernumber = reader.ReadByte();
                    if (playernumber >= Main.maxPlayers || !Main.player[playernumber].active) return;
                    LotMPlayer modPlayer = Main.player[playernumber].GetModPlayer<LotMPlayer>();

                    // 读取所有 LotMPlayer 的数据 (保持你原有的代码逻辑)
                    modPlayer.baseSequence = reader.ReadInt32();
                    modPlayer.baseMarauderSequence = reader.ReadInt32();
                    modPlayer.baseFoolSequence = reader.ReadInt32();
                    modPlayer.baseHunterSequence = reader.ReadInt32();
                    modPlayer.baseMoonSequence = reader.ReadInt32();
                    modPlayer.baseSunSequence = reader.ReadInt32();

                    modPlayer.currentSequence = reader.ReadInt32();
                    modPlayer.currentMarauderSequence = reader.ReadInt32();
                    modPlayer.currentFoolSequence = reader.ReadInt32();
                    modPlayer.currentHunterSequence = reader.ReadInt32();
                    modPlayer.currentMoonSequence = reader.ReadInt32();
                    modPlayer.currentSunSequence = reader.ReadInt32();
                    modPlayer.spiritualityCurrent = reader.ReadSingle();

                    modPlayer.isParasitizing = reader.ReadBoolean();
                    modPlayer.parasiteTargetIndex = reader.ReadInt32();
                    modPlayer.parasiteIsTownNPC = reader.ReadBoolean();
                    modPlayer.parasiteIsPlayer = reader.ReadBoolean();
                    modPlayer.purificationProgress = reader.ReadInt32();
                    modPlayer.judgmentProgress = reader.ReadInt32();
                    modPlayer.ironBloodRitualProgress = reader.ReadInt32();

                    modPlayer.spiritWorms = reader.ReadInt32();

                    modPlayer.isSpiritVisionActive = reader.ReadBoolean();
                    modPlayer.isSpiritForm = reader.ReadBoolean();
                    modPlayer.graftingMode = reader.ReadInt32();
                    modPlayer.spiritThreadTargetIndex = reader.ReadInt32();

                    modPlayer.isDeceitDomainActive = reader.ReadBoolean();
                    modPlayer.isTimeClockActive = reader.ReadBoolean();

                    modPlayer.isTamingActive = reader.ReadBoolean();
                    modPlayer.isVampireWings = reader.ReadBoolean();
                    modPlayer.isBatSwarm = reader.ReadBoolean();
                    modPlayer.isMoonlightized = reader.ReadBoolean();
                    modPlayer.isFullMoonActive = reader.ReadBoolean();
                    modPlayer.isCreationDomain = reader.ReadBoolean();

                    modPlayer.isFireForm = reader.ReadBoolean();
                    modPlayer.isCalamityGiant = reader.ReadBoolean();
                    modPlayer.isFlameCloakActive = reader.ReadBoolean();

                    modPlayer.isGuardianStance = reader.ReadBoolean();
                    modPlayer.isMercuryForm = reader.ReadBoolean();
                    modPlayer.dawnArmorActive = reader.ReadBoolean();

                    modPlayer.isSinging = reader.ReadBoolean();
                    modPlayer.isSunMessenger = reader.ReadBoolean();
                    modPlayer.isPassiveStealEnabled = reader.ReadBoolean();

                    // 如果我是服务器，我要把这份数据转发给其他玩家
                    if (Main.netMode == NetmodeID.Server)
                    {
                        modPlayer.SyncPlayer(-1, whoAmI, false);
                    }
                    break;

                // ==========================================================
                // Case 1: 太阳途径压制 Buff (保持原样)
                // ==========================================================
                case LotMNetMsg.ApplySunSuppression:
                    int targetWho = reader.ReadByte();
                    int duration = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        Player target = Main.player[targetWho];
                        if (target.active)
                        {
                            target.AddBuff(ModContent.BuffType<SunSuppressionDebuff>(), duration);
                        }
                    }
                    break;

                // ==========================================================
                // Case 2: 好感度同步 (这里是合并后的新逻辑)
                // ==========================================================
                case LotMNetMsg.SyncFavorability:
                    int npcID = reader.ReadInt32();
                    int score = reader.ReadInt32();

                    // 2. 更新本地数据
                    if (!FavorabilitySystem.favorabilityScores.ContainsKey(npcID))
                        FavorabilitySystem.favorabilityScores[npcID] = 0;

                    FavorabilitySystem.favorabilityScores[npcID] = score;

                    // 3. 如果我是服务器，转发给其他所有客户端
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)LotMNetMsg.SyncFavorability);
                        packet.Write(npcID);
                        packet.Write(score);
                        packet.Send(-1, whoAmI); // 发给除了发送者以外的所有人
                    }
                    break;
            }
        }
    }
}