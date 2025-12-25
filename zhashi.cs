using Terraria.ModLoader;
using System.IO;
using Terraria;
using zhashi.Content; // 确保引用了 LotMPlayer 的位置

namespace zhashi
{
    // 【修改】将枚举改名，并放在类外面，防止与类名冲突
    public enum LotMNetMsg : byte
    {
        PlayerSync
    }

    public class zhashi : Mod
    {
        // 处理接收到的网络数据包
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            // 1. 读取消息类型
            LotMNetMsg msgType = (LotMNetMsg)reader.ReadByte();

            switch (msgType)
            {
                case LotMNetMsg.PlayerSync:
                    // A. 读取玩家 ID
                    byte playernumber = reader.ReadByte();
                    
                    // 安全检查：防止 ID 越界或玩家未激活
                    if (playernumber >= Main.maxPlayers || !Main.player[playernumber].active) 
                        return; 
                    
                    // 获取 ModPlayer
                    LotMPlayer modPlayer = Main.player[playernumber].GetModPlayer<LotMPlayer>();

                    // B. 【关键】按顺序读取变量
                    // 必须与 LotMPlayer.cs 中的 SyncPlayer 写入顺序一模一样！
                    modPlayer.currentSequence = reader.ReadInt32();
                    modPlayer.currentMarauderSequence = reader.ReadInt32();
                    modPlayer.currentFoolSequence = reader.ReadInt32();
                    modPlayer.currentHunterSequence = reader.ReadInt32();
                    modPlayer.currentMoonSequence = reader.ReadInt32();
                    modPlayer.spiritualityCurrent = reader.ReadSingle();
                    
                    // 寄生数据
                    modPlayer.isParasitizing = reader.ReadBoolean();
                    modPlayer.parasiteTargetIndex = reader.ReadInt32();
                    modPlayer.parasiteIsTownNPC = reader.ReadBoolean();
                    modPlayer.parasiteIsPlayer = reader.ReadBoolean();

                    // C. 服务器转发 (如果是服务器收到，就转发给其他玩家)
                    if (Main.netMode == Terraria.ID.NetmodeID.Server)
                    {
                        // 这里的 -1 表示发给所有人，whoAmI 表示“除了发送者”
                        // 最后一个参数 true/false 取决于你的 SyncPlayer 定义，通常设为 false 即可
                        modPlayer.SyncPlayer(-1, whoAmI, false);
                    }
                    break;
            }
        }
    }
}