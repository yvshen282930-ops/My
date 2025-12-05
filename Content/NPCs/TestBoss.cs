using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace zhashi.Content.NPCs
{
    // [AutoloadBossHead] // 如果你有地图头像图片(TestBoss_Head_Boss.png)就解开注释，没有就注释掉
    public class TestBoss : ModNPC
    {
        // 确保你的图片名叫 TestBoss.png，并放在 zhashi/Content/NPCs/ 目录下
        public override string Texture => "zhashi/Content/NPCs/TestBoss";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("娱乐测试怪");
            Main.npcFrameCount[NPC.type] = 1; // 假设只有1帧
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 50;
            NPC.defense = 20;
            NPC.lifeMax = 50000; // 5万血，耐打一点
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 0f;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1; // 自定义 AI

            NPC.boss = true;
            NPC.noGravity = true; // 飞行
            NPC.noTileCollide = true; // 穿墙
        }

        public override void AI()
        {
            // 1. 寻找玩家
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(true);
            }
            Player player = Main.player[NPC.target];

            // 2. 玩家死后逃跑
            if (player.dead)
            {
                NPC.velocity.Y -= 0.1f;
                NPC.EncourageDespawn(10);
                return;
            }

            // 3. 简单的追击 AI (平滑移动)
            float speed = 12f;
            float inertia = 20f; // 惯性，越大转弯越慢

            Vector2 direction = player.Center - NPC.Center;
            direction.Normalize();
            direction *= speed;

            NPC.velocity = (NPC.velocity * (inertia - 1) + direction) / inertia;

            // 4. 面向玩家
            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}