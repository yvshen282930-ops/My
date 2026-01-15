using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;

namespace zhashi.Content.Systems
{
    public class RuinsGenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int shinyIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            if (shinyIndex != -1)
            {
                tasks.Insert(shinyIndex + 1, new PassLegacy("Generating LotM Cathedral", GenerateCathedral));
            }
        }

        private void GenerateCathedral(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "正在具现愚者的古老教堂...";

            // 检查玩家是否安装了 StructureHelper
            // 如果没装，就不生成，防止报错崩溃
            if (!ModLoader.TryGetMod("StructureHelper", out Mod structureHelper))
            {
                return;
            }

            // 在地图上随机找点
            int count = Main.rand.Next(2, 4);
            int placed = 0;
            int attempts = 0;

            while (placed < count && attempts < 2000)
            {
                attempts++;
                int x = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
                int y = (int)Main.worldSurface;

                while (y < Main.maxTilesY - 200 && !Main.tile[x, y].HasTile) y++;

                Tile tile = Main.tile[x, y];
                bool isValidGround = tile.TileType == TileID.Grass || tile.TileType == TileID.Stone || tile.TileType == TileID.Dirt || tile.TileType == TileID.SnowBlock;

                if (isValidGround)
                {
                    // === 核心调用 ===
                    // 这里的 "Assets/Structures/FoolCathedral" 是文件路径（不带后缀）
                    // StructureHelper 会自动处理所有方块、墙壁、斜坡

                    bool success = GenerateStructureSafe(structureHelper, "Assets/Structures/FoolCathedral", x, y - 1);

                    if (success)
                    {
                        placed++;
                        // 如果需要在生成后往箱子里塞东西，可以在这里根据坐标找附近的箱子
                    }
                }
            }
        }

        // 封装一个安全的方法来调用 StructureHelper
        private bool GenerateStructureSafe(Mod structureHelper, string path, int x, int y)
        {
            // StructureHelper.Generator.GenerateStructure(string filePath, Point16 position, Mod mod)
            // 这是 Structure Helper 提供的标准生成方法

            // 使用 Call 方法进行跨模组调用 (最安全，不依赖强引用)
            // 参数文档通常在 Structure Helper 的 Wiki 或源码里
            // 常用调用格式: ["Generate", 文件路径, 坐标X, 坐标Y, 模组实例]

            object result = structureHelper.Call("Generate", path, new Point16(x, y), Mod);

            return result != null && (bool)result;
        }
    }
}