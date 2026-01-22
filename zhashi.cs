using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.IO;
using zhashi.Content;
using zhashi.Content.UI;
using zhashi.Content.Buffs;
using zhashi.Content.Systems;

namespace zhashi
{
    // ★★★ 修复点：将枚举移到 Class 外面，直接放在 Namespace 下 ★★★
    public enum LotMNetMsg : byte
    {
        PlayerSync = 0,
        ApplySunSuppression = 1,
        SyncFavorability = 2,
        StoryDataSync = 3
    }

    public class zhashi : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            // 读取消息类型
            LotMNetMsg msgType = (LotMNetMsg)reader.ReadByte();

            switch (msgType)
            {
                // Case 0: 战斗玩家数据同步
                case LotMNetMsg.PlayerSync:
                    byte playernumber = reader.ReadByte();
                    if (playernumber >= Main.maxPlayers || !Main.player[playernumber].active) return;
                    LotMPlayer modPlayer = Main.player[playernumber].GetModPlayer<LotMPlayer>();

                    // 读取数据
                    modPlayer.baseSequence = reader.ReadInt32();
                    modPlayer.baseMarauderSequence = reader.ReadInt32();
                    modPlayer.baseFoolSequence = reader.ReadInt32();
                    modPlayer.baseHunterSequence = reader.ReadInt32();
                    modPlayer.baseMoonSequence = reader.ReadInt32();
                    modPlayer.baseSunSequence = reader.ReadInt32();
                    modPlayer.baseDemonessSequence = reader.ReadInt32();

                    modPlayer.currentSequence = reader.ReadInt32();
                    modPlayer.currentMarauderSequence = reader.ReadInt32();
                    modPlayer.currentFoolSequence = reader.ReadInt32();
                    modPlayer.currentHunterSequence = reader.ReadInt32();
                    modPlayer.currentMoonSequence = reader.ReadInt32();
                    modPlayer.currentSunSequence = reader.ReadInt32();
                    modPlayer.currentDemonessSequence = reader.ReadInt32();
                    modPlayer.spiritualityCurrent = reader.ReadSingle();

                    modPlayer.isParasitizing = reader.ReadBoolean();
                    modPlayer.parasiteTargetIndex = reader.ReadInt32();
                    modPlayer.parasiteIsTownNPC = reader.ReadBoolean();
                    modPlayer.parasiteIsPlayer = reader.ReadBoolean();
                    modPlayer.purificationProgress = reader.ReadInt32();
                    modPlayer.judgmentProgress = reader.ReadInt32();
                    modPlayer.ironBloodRitualProgress = reader.ReadInt32();
                    modPlayer.despairRitualCount = reader.ReadInt32();      // 序列4 仪式
                    modPlayer.afflictionRitualTimer = reader.ReadInt32();   // 序列5 仪式

                    modPlayer.despairRitualCount = reader.ReadInt32();

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

                    if (Main.netMode == NetmodeID.Server)
                        modPlayer.SyncPlayer(-1, whoAmI, false);
                    break;

                // Case 1: 太阳压制
                case LotMNetMsg.ApplySunSuppression:
                    int targetWho = reader.ReadByte();
                    int duration = reader.ReadInt32();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        if (targetWho >= 0 && targetWho < Main.maxPlayers)
                        {
                            Player target = Main.player[targetWho];
                            if (target.active) target.AddBuff(ModContent.BuffType<SunSuppressionDebuff>(), duration);
                        }
                    }
                    break;

                // Case 2: 好感度
                case LotMNetMsg.SyncFavorability:
                    int npcID = reader.ReadInt32();
                    int score = reader.ReadInt32();
                    if (!FavorabilitySystem.favorabilityScores.ContainsKey(npcID))
                        FavorabilitySystem.favorabilityScores[npcID] = 0;
                    FavorabilitySystem.favorabilityScores[npcID] = score;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)LotMNetMsg.SyncFavorability);
                        packet.Write(npcID);
                        packet.Write(score);
                        packet.Send(-1, whoAmI);
                    }
                    break;

                // Case 3: 剧情
                case LotMNetMsg.StoryDataSync:
                    byte storyPlayerNum = reader.ReadByte();
                    if (storyPlayerNum >= Main.maxPlayers || !Main.player[storyPlayerNum].active) return;
                    LotMStoryPlayer storyPlayer = Main.player[storyPlayerNum].GetModPlayer<LotMStoryPlayer>();

                    storyPlayer.StoryStage = reader.ReadInt32();
                    storyPlayer.DaysSinceJoined = reader.ReadInt32();
                    storyPlayer.HasDailyQuest = reader.ReadBoolean();
                    storyPlayer.QuestCompletedToday = reader.ReadBoolean();
                    storyPlayer.QuestType = reader.ReadInt32();
                    storyPlayer.QuestTargetID = reader.ReadInt32();
                    storyPlayer.QuestRequiredAmount = reader.ReadInt32();
                    storyPlayer.QuestCurrentAmount = reader.ReadInt32();
                    storyPlayer.HasReceivedStarterPotion = reader.ReadBoolean();

                    if (Main.netMode == NetmodeID.Server)
                        storyPlayer.SyncPlayer(-1, whoAmI, false);
                    break;
            }
        }
    }
}