using Terraria;
using Terraria.ModLoader;

namespace zhashi.Content
{
    public class ConquerorSpawnSystem : ModSystem
    {
        public static bool StopSpawning = false;

        // 每帧检查
        public override void PreUpdateNPCs()
        {
            if (StopSpawning)
            {
                Player p = Main.LocalPlayer;
                // 如果玩家死了，或者退出了，强制恢复刷怪
                if (p.dead || !p.active)
                {
                    StopSpawning = false;
                    // 可以加一句提示
                    // Main.NewText("征服者威压消散...", 150, 150, 150);
                }
            }
        }

        // 离开世界时重置，绝对安全
        public override void OnWorldUnload()
        {
            StopSpawning = false;
        }
    }

    public class ConquerorSpawnRateControl : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (ConquerorSpawnSystem.StopSpawning)
            {
                spawnRate = int.MaxValue;
                maxSpawns = 0;
            }
        }
    }
}