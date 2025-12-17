using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent.ItemDropRules;
using zhashi;
using zhashi.Content.Projectiles;
using zhashi.Content.Items.Weapons;
using zhashi.Content.Buffs;
using zhashi.Content.Items;

namespace zhashi.Content
{
    public class LotMPlayer : ModPlayer
    {
        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            return new[] {
                // 确保 RoselleDiary 这个类名和 Content/Items/RoselleDiary.cs 里的类名一致
                new Item(ModContent.ItemType<RoselleDiary>())
            };
        }

        // ===================================================
        // 1. 核心变量定义
        // ===================================================
        public int currentSequence = 10;       // 巨人/战士途径 (9-2)
        public int currentHunterSequence = 10; // 猎人途径 (9-1)
        public int currentMoonSequence = 10;   // 月亮途径 (9-1)
        public int currentFoolSequence = 10;   // 愚者途径 (9-1)
        public int currentMarauderSequence = 10; // 错误途径 (9-1)


        public bool IsBeyonder => currentSequence < 10 || currentHunterSequence < 10 || currentMoonSequence < 10 || currentFoolSequence < 10 || currentMarauderSequence < 10;

        // 灵性系统
        public float spiritualityCurrent = 100;
        public int spiritualityMax = 100;
        private int spiritualityRegenTimer = 0;

        // --- 巨人途径技能状态 ---
        public bool dawnArmorActive = false;
        public bool dawnArmorBroken = false;
        public int dawnArmorCurrentHP = 250;
        public int MaxDawnArmorHP => (int)(500 * GetSequenceMultiplier(currentSequence));
        public int dawnArmorCooldownTimer = 0;
        public const int DAWN_ARMOR_COOLDOWN_MAX = 900;
        public bool isGuardianStance = false;
        public bool isMercuryForm = false;
        public int twilightResurrectionCooldown = 0;
        public const int TWILIGHT_RESURRECTION_MAX = 18000;

        // --- 猎人途径技能状态 ---
        public bool isFlameCloakActive = false;
        public bool arsonistFireImmune = false;
        public bool isFireEnchanted = false;
        public int fireTeleportCooldown = 0;
        public const int FIRE_TELEPORT_MAX = 60;
        public int fireballChargeTimer = 0;
        public bool isChargingFireball = false;
        public bool isFireForm = false;
        public bool isArmyOfOne = false;
        public bool isCalamityGiant = false;
        public int glacierCooldown = 0;
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            // 如果正在进行序列5的仪式，死亡会导致前功尽弃
            if (currentHunterSequence == 5)
            {
                if (ironBloodRitualProgress > 0)
                {
                    ironBloodRitualProgress = 0;
                    Main.NewText("你已死亡，铁血仪式进度归零...", 255, 50, 50);
                }
            }
        }

        // --- 月亮途径技能状态 ---
        public bool isTamingActive = false;
        public bool isVampireWings = false;    // 序列7：黑暗之翼
        public int abyssShackleCooldown = 0;   // 序列7：深渊枷锁
        public int elixirCooldown = 0;         // 序列6：生命灵液
        public bool isFullMoonActive = false;  // 序列5：满月领域
        public bool isMoonlightized = false;   // 序列5：月光化
        public bool isBatSwarm = false;        // 序列4：蝙蝠化身
        public int paperFigurineCooldown = 0;  // 序列4：月亮纸人
        public int darknessGazeCooldown = 0;   // 序列4：黑暗凝视
        public int summonGateCooldown = 0;     // 序列3：召唤之门
        public bool isCreationDomain = false;  // 序列2：创生领域
        public int purifyCooldown = 0;         // 序列2：净化大地

        // --- 愚者途径技能状态 ---
        public bool isSpiritVisionActive = false; // 灵视开关
        public int divinationCooldown = 0;        // 占卜冷却
        public int damageTransferCooldown = 0; // 伤害转移冷却
        public int flameJumpCooldown = 0;      // 火焰跳跃冷却
        public int distortCooldown = 0;           // 干扰直觉冷却
        public bool isFacelessActive = false;     // 无面伪装开关
        public int spiritThreadTargetIndex = -1; // 当前控制的目标 NPC 索引
        public int spiritThreadTimer = 0;        // 控制进度计时器
        public const int CONTROL_TIME_REQUIRED = 180; // 需要控制3秒 (3 * 60)
        public int swapCooldown = 0;              // 互换冷却
        public int spiritControlCooldown = 0;     // 控灵冷却
        public bool isSpiritForm = false;         // 是否处于灵体状态
        public int graftingMode = 0;              // 嫁接模式: 0=无, 1=反弹(空间), 2=必杀(攻击)
        public int graftingCooldown = 0;          // 嫁接冷却
        public int realmRange = 1500;             // 诡秘之境范围
        public bool waitingForTeleport = false; // 标记：是否正在等待点击传送
        // 灵之虫系统
        public int spiritWorms = 50;              // 当前灵之虫数量
        public const int MAX_SPIRIT_WORMS = 50;
        public int wormRegenTimer = 0;            // 回复计时器
        public int historyUses = 0;               // 历史投影当前维持数量
        public int borrowUsesDaily = 0;           // 昨日重现今日已用次数
        public bool isBorrowingPower = false;     // 是否正在借用力量
        public int borrowTimer = 0;               // 借用力量剩余时间
        public int selectedWish = 0;              // 当前选择的愿望 (0-3)
        public int miracleCooldown = 0;           // 愿望/奇迹冷却
        public bool fateDisturbanceActive = false;// 干扰命运开关
        public int wishCastTimer = 0;             // 按键长按计时

        // --- 错误途径技能状态 ---
        public int dreamWalkCooldown = 0;      // 梦境穿行冷却
        public const int DREAM_WALK_MAX = 180; // 3秒冷却
        public bool isParasitizing = false;     // 是否正在寄生
        public int parasiteTargetIndex = -1;    // 寄生目标的 NPC 索引
        public bool parasiteIsTownNPC = false;  // 寄生的是否为城镇 NPC
        public int wormificationCooldown = 0;   // 半虫化不死能力的冷却
        public const int WORMIFICATION_COOLDOWN_MAX = 36000; // 10分钟冷却
        public int conceptStealCooldown = 0;    // 概念窃取（窃取距离/位置）冷却
        public int parasiteRitualProgress = 0; // 寄生者仪式进度
        public const int PARASITE_RITUAL_TARGET = 9; // 目标次数
        // --- 错误途径 序列3 欺瞒导师 ---
        public int mentorRitualProgress = 0;   // 仪式进度
        public const int MENTOR_RITUAL_TARGET = 9; // 需要误导9个冤魂
        public bool isDeceitDomainActive = false; // 欺瞒领域开关
        public int deceitCooldown = 0;         // 技能冷却
        public int trojanRitualTimer = 0;      // 仪式计时器
        public const int TROJAN_RITUAL_TARGET = 18000; // 目标：5分钟 (60帧 * 300秒)
        public int fateTheftCooldown = 0;      // 命运窃取冷却
        public bool isTrojanResurrection = false; // 是否触发了木马替死
        // --- 错误途径：窃取系统 ---
        public bool stealMode = false;         // 是否开启窃取模式
        public int stealAggroTimer = 0;        // 被发现后的惩罚计时器
        public int wormRitualTimer = 0;        // 仪式计时
        public const int WORM_RITUAL_TARGET = 25200; // 7分钟 (60帧 * 420秒)
        public int timeTheftCooldown = 0;      // 窃取时间冷却
        public bool isTimeClockActive = false; // 时之虫领域是否开启
        // 【新增】标记当前寄生目标是否为玩家
        public bool parasiteIsPlayer = false;

        // ===================================================
        // 【新增】狗的数据存储 (绑定在玩家身上)
        // ===================================================
        public string DogName = "旺财";
        public int DogPathway = 0;
        public int DogSequence = 10;
        public int DogBonusHP = 0;
        public Item[] DogInventory = new Item[3];

        // 标记日期变化
        public bool lastDayState = false;

        // --- 仪式与杂项 ---
        public int guardianRitualProgress = 0;
        public const int GUARDIAN_RITUAL_TARGET = 1000;
        public int demonHunterRitualProgress = 0;
        public const int DEMON_HUNTER_RITUAL_TARGET = 10;
        public int ironBloodRitualProgress = 0;
        public const int IRON_BLOOD_RITUAL_TARGET = 100;
        public int weatherRitualCount = 0;
        public int weatherRitualTimer = 0;
        public bool weatherRitualComplete = false;
        public bool conquerorRitualComplete = false;
        public int stealthTimer = 0;
        public int shakeTime = 0;
        public float shakePower = 0f;
        public int attendantRitualProgress = 0;
        public const int ATTENDANT_RITUAL_TARGET = 10;
        public bool attendantRitualComplete = false;

        // ===================================================
        // 2. 数据存档与读取
        // ===================================================
        public override void SaveData(TagCompound tag)
        {
            tag["CurrentSequence"] = currentSequence;
            tag["HunterSequence"] = currentHunterSequence;
            tag["MoonSequence"] = currentMoonSequence;
            tag["FoolSequence"] = currentFoolSequence;
            tag["Spirituality"] = spiritualityCurrent;
            tag["GuardianRitual"] = guardianRitualProgress;
            tag["DemonHunterRitual"] = demonHunterRitualProgress;
            tag["IronBloodRitual"] = ironBloodRitualProgress;
            tag["WeatherRitualComplete"] = weatherRitualComplete;
            tag["ConquerorRitual"] = conquerorRitualComplete;
            tag["ResurrectionCooldown"] = twilightResurrectionCooldown;
            tag["BorrowUses"] = borrowUsesDaily;
            tag["MarauderSequence"] = currentMarauderSequence;
            tag["AttendantRitual"] = attendantRitualProgress;
            tag["AttendantRitualComplete"] = attendantRitualComplete;
            tag["MarauderSequence"] = currentMarauderSequence;
            tag["WormificationCD"] = wormificationCooldown;
            tag["ParasiteRitual"] = parasiteRitualProgress;
            tag["MentorRitual"] = mentorRitualProgress;
            tag["TrojanRitual"] = trojanRitualTimer;
            tag["WormRitual"] = wormRitualTimer;
            tag["MyDog_Name"] = DogName;
            tag["MyDog_Pathway"] = DogPathway;
            tag["MyDog_Sequence"] = DogSequence;
            tag["MyDog_BonusHP"] = DogBonusHP;

            if (DogInventory == null) DogInventory = new Item[3];
            for (int i = 0; i < 3; i++)
            {
                if (DogInventory[i] == null) DogInventory[i] = new Item();
                tag[$"MyDog_Inv_{i}"] = ItemIO.Save(DogInventory[i]);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("CurrentSequence")) currentSequence = tag.GetInt("CurrentSequence");
            if (tag.ContainsKey("HunterSequence")) currentHunterSequence = tag.GetInt("HunterSequence");
            if (tag.ContainsKey("MoonSequence")) currentMoonSequence = tag.GetInt("MoonSequence");
            if (tag.ContainsKey("FoolSequence")) currentFoolSequence = tag.GetInt("FoolSequence");

            if (tag.ContainsKey("Spirituality")) spiritualityCurrent = tag.GetFloat("Spirituality");
            if (tag.ContainsKey("GuardianRitual")) guardianRitualProgress = tag.GetInt("GuardianRitual");
            if (tag.ContainsKey("DemonHunterRitual")) demonHunterRitualProgress = tag.GetInt("DemonHunterRitual");
            if (tag.ContainsKey("IronBloodRitual")) ironBloodRitualProgress = tag.GetInt("IronBloodRitual");
            if (tag.ContainsKey("WeatherRitualComplete")) weatherRitualComplete = tag.GetBool("WeatherRitualComplete");
            if (tag.ContainsKey("ConquerorRitual")) conquerorRitualComplete = tag.GetBool("ConquerorRitual");
            if (tag.ContainsKey("ResurrectionCooldown")) twilightResurrectionCooldown = tag.GetInt("ResurrectionCooldown");
            if (tag.ContainsKey("BorrowUses")) borrowUsesDaily = tag.GetInt("BorrowUses");
            if (tag.ContainsKey("MarauderSequence")) currentMarauderSequence = tag.GetInt("MarauderSequence");
            if (tag.ContainsKey("AttendantRitual")) attendantRitualProgress = tag.GetInt("AttendantRitual");
            if (tag.ContainsKey("AttendantRitualComplete")) attendantRitualComplete = tag.GetBool("AttendantRitualComplete");
            if (tag.ContainsKey("MarauderSequence")) currentMarauderSequence = tag.GetInt("MarauderSequence");
            if (tag.ContainsKey("WormificationCD")) wormificationCooldown = tag.GetInt("WormificationCD");
            if (tag.ContainsKey("ParasiteRitual")) parasiteRitualProgress = tag.GetInt("ParasiteRitual");
            if (tag.ContainsKey("MentorRitual")) mentorRitualProgress = tag.GetInt("MentorRitual");
            if (tag.ContainsKey("TrojanRitual")) trojanRitualTimer = tag.GetInt("TrojanRitual");
            if (tag.ContainsKey("WormRitual")) wormRitualTimer = tag.GetInt("WormRitual");
            if (tag.ContainsKey("MyDog_Name")) DogName = tag.GetString("MyDog_Name");
            if (tag.ContainsKey("MyDog_Pathway")) DogPathway = tag.GetInt("MyDog_Pathway");
            if (tag.ContainsKey("MyDog_Sequence")) DogSequence = tag.GetInt("MyDog_Sequence");
            if (tag.ContainsKey("MyDog_BonusHP")) DogBonusHP = tag.GetInt("MyDog_BonusHP");

            DogInventory = new Item[3];
            for (int i = 0; i < 3; i++)
            {
                if (tag.ContainsKey($"MyDog_Inv_{i}"))
                    DogInventory[i] = ItemIO.Load(tag.GetCompound($"MyDog_Inv_{i}"));
                else
                {
                    DogInventory[i] = new Item();
                    DogInventory[i].SetDefaults(0);
                }
            }
        }

        // ===================================================
        // 3. 属性重置与核心逻辑
        // ===================================================
        public override void PreUpdate()
        {
            if (Player.dead) waitingForTeleport = false;
            if (waitingForTeleport)
            {
                // 视觉提示：玩家身边产生一些空间波纹，提示处于技能状态中
                if (Main.rand.NextBool(5))
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.Vortex, 0, 0, 0, default, 1f);

                // 检测鼠标左键点击 (按下并释放的一瞬间触发)
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Vector2 targetPos = Main.MouseWorld;

                    // 核心逻辑：如果大地图(Map)是打开的，计算地图坐标
                    if (Main.mapFullscreen)
                    {
                        // 获取地图缩放比例
                        float scale = Main.mapFullscreenScale;
                        // 计算鼠标相对于屏幕中心的偏移量 (Pixel)
                        float dx = (Main.mouseX - Main.screenWidth / 2f) / scale;
                        float dy = (Main.mouseY - Main.screenHeight / 2f) / scale;

                        // Main.mapFullscreenPos 是地图中心的 Tile 坐标 (1 Tile = 16 Pixels)
                        // 目标世界坐标 = (地图中心Tile坐标 + 鼠标偏移Tile量) * 16
                        targetPos = new Vector2(
                            (Main.mapFullscreenPos.X + dx) * 16f,
                            (Main.mapFullscreenPos.Y + dy) * 16f
                        );

                        // 传送后自动关闭地图，方便玩家立刻看到位置
                        Main.mapFullscreen = false;
                    }

                    // 执行传送
                    Player.Teleport(targetPos, 1);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.position); // 播放传送音效
                    Main.NewText("空间跨越成功！", 0, 255, 255);

                    // 消耗完成，关闭状态
                    waitingForTeleport = false;
                }
            }
            // ============================
            
            if (isDeceitDomainActive)
            {
                // 持续消耗灵性
                if (!TryConsumeSpirituality(1.5f, true))
                {
                    isDeceitDomainActive = false;
                    Main.NewText("灵性枯竭，欺瞒领域消散。", 150, 150, 150);
                }
                else
                {
                    // 视觉特效：扭曲的空气
                    if (Main.GameUpdateCount % 10 == 0)
                    {
                        Vector2 pos = Player.Center + Main.rand.NextVector2Circular(400, 400);
                        Dust d = Dust.NewDustPerfect(pos, DustID.Vortex, Vector2.Zero, 150, default, 0.5f);
                        d.noGravity = true;
                    }

                    // 1. 误导生物 (混乱)
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 500f)
                        {
                            npc.AddBuff(BuffID.Confused, 60); // 强行误导
                            // 欺诈规则：让敌人防御力由于“判断错误”而失效
                            npc.defense = (int)(npc.defDefense * 0.5f);
                        }
                    }

                    // 2. 误导攻击 (弹幕偏转)
                    // 遍历敌对弹幕，使其偏离玩家
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile p = Main.projectile[i];
                        if (p.active && p.hostile && p.Distance(Player.Center) < 200f)
                        {
                            // 给一个与玩家方向相反的力
                            Vector2 push = (p.Center - Player.Center).SafeNormalize(Vector2.Zero) * 2f;
                            p.velocity += push;
                            // 甚至可能直接把弹幕“骗”成友军 (仅限非Boss弹幕)
                            if (currentMarauderSequence <= 2 && Main.rand.NextBool(50))
                            {
                                p.hostile = false;
                                p.friendly = true;
                            }
                        }
                    }
                }
                if (currentMarauderSequence == 2)
                {
                    // 条件：欺瞒领域开启 + 处于城镇中 (周围有NPC)
                    if (isDeceitDomainActive && Player.townNPCs >= 3f)
                    {
                        wormRitualTimer++;

                        // 视觉提示：每60秒提示一次
                        if (wormRitualTimer % 3600 == 0)
                        {
                            int minutes = wormRitualTimer / 3600;
                            Main.NewText($"周边的时光正在发生错乱... ({minutes}/7 分钟)", 150, 150, 255);
                        }

                        if (wormRitualTimer == WORM_RITUAL_TARGET)
                        {
                            Main.NewText("仪式完成：古老的壁钟虚影已笼罩这座城市！(7/7)", 0, 255, 255);
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item119, Player.position); // 神秘的声音
                        }
                    }
                }
            }
            // 序列1：美神 强制转变为女性
            if (currentMoonSequence <= 1) Player.Male = false;
            base.PreUpdate();
            if (Main.dayTime && !lastDayState)
            {
                if (Main.time < 60)
                {
                    borrowUsesDaily = 0;
                    Main.NewText("新的一天，过去的力量已重置。", 200, 200, 255);
                }
            }
            lastDayState = Main.dayTime;

            // 2. 借用力量计时
            if (isBorrowingPower)
            {
                borrowTimer--;
                if (borrowTimer <= 0)
                {
                    isBorrowingPower = false;
                    Main.NewText("借来的力量消退了...", 150, 150, 150);
                }
            }

            // =================================================
            // 3. 【核心修复】灵体化穿墙逻辑 (Noclip)
            // =================================================
            if (isSpiritForm)
            {
                // 消除重力和惯性
                Player.gravity = 0f;
                Player.velocity = Vector2.Zero;
                Player.fallStart = (int)(Player.position.Y / 16f); // 防止解除时受到摔落伤害

                // 手动控制移动 (无视墙壁)
                float speed = 12f; // 灵体飞行速度

                // 检测按键
                if (Player.controlLeft) Player.position.X -= speed;
                if (Player.controlRight) Player.position.X += speed;
                if (Player.controlUp) Player.position.Y -= speed;
                if (Player.controlDown) Player.position.Y += speed;

                // 既然是灵体，就不要受其他物理效果影响了
                Player.noKnockback = true;
            }
            if (currentFoolSequence <= 1)
            {
                if (Main.GameUpdateCount % 30 == 0)
                {
                    ProcessRealmOfMysteries();
                }
            }

            base.PreUpdate();
        }

        public override void ResetEffects()
        {
            CalculateMaxSpirituality();
            HandleSpiritualityRegen();
            HandleDawnArmorLogic();

            // 冷却倒计时
            if (twilightResurrectionCooldown > 0) twilightResurrectionCooldown--;
            if (fireTeleportCooldown > 0) fireTeleportCooldown--;
            if (glacierCooldown > 0) glacierCooldown--;
            if (abyssShackleCooldown > 0) abyssShackleCooldown--;
            if (elixirCooldown > 0) elixirCooldown--;
            if (paperFigurineCooldown > 0) paperFigurineCooldown--;
            if (darknessGazeCooldown > 0) darknessGazeCooldown--;
            if (summonGateCooldown > 0) summonGateCooldown--;
            if (purifyCooldown > 0) purifyCooldown--;
            if (divinationCooldown > 0) divinationCooldown--;
            if (damageTransferCooldown > 0) damageTransferCooldown--;
            if (flameJumpCooldown > 0) flameJumpCooldown--;
            if (distortCooldown > 0) distortCooldown--;
            if (swapCooldown > 0) swapCooldown--;
            if (spiritControlCooldown > 0) spiritControlCooldown--;
            if (miracleCooldown > 0) miracleCooldown--;
            if (graftingCooldown > 0) graftingCooldown--;
            if (dreamWalkCooldown > 0) dreamWalkCooldown--;
            if (wormificationCooldown > 0) wormificationCooldown--;
            if (conceptStealCooldown > 0) conceptStealCooldown--;
            if (fateTheftCooldown > 0) fateTheftCooldown--;
            if (timeTheftCooldown > 0) timeTheftCooldown--;

            // ==========================================
            // 3. 灵之虫自动再生系统 (整合版)
            // ==========================================
            if (currentFoolSequence <= 4)
            {
                // 动态设定上限和回复速度
                int wormCap = 50;       // 序列4 默认
                int regenSpeed = 1800;  // 序列4: 30秒回1条

                if (currentFoolSequence <= 3)
                {
                    wormCap = 600;
                    regenSpeed = 300;   // 序列3: 5秒回1条
                }

                if (currentFoolSequence <= 2)
                {
                    wormCap = 1200;
                    regenSpeed = 60;    // 序列2: 1秒回1条 (极快)
                }

                // 执行回复逻辑
                if (spiritWorms < wormCap)
                {
                    wormRegenTimer++;
                    if (wormRegenTimer >= regenSpeed)
                    {
                        spiritWorms++;
                        wormRegenTimer = 0;
                    }
                }
            }
            // 灵肉转化：每帧消耗灵性 (支持平衡性削弱)
            if (isSpiritForm)
            {
                // 1. 获取配置：是否开启削弱 (需创建 LotMConfig.cs)
                bool nerf = ModContent.GetInstance<LotMConfig>().NerfDivineAbilities;

                // 2. 决定消耗：削弱模式下消耗激增 (50/帧), 原著模式 (5/帧)
                float cost = nerf ? 50.0f : 5.0f;

                if (!TryConsumeSpirituality(cost, true))
                {
                    isSpiritForm = false;
                    Main.NewText("灵性枯竭，被迫回归血肉之躯。", 255, 50, 50);
                }
                else
                {
                    // 3. 决定防御机制
                    if (nerf)
                    {
                        // 【平衡模式】
                        // 移除完全无敌，改为高额减伤和闪避
                        Player.endurance += 0.6f; // 60% 免伤
                        Player.statDefense += 200; // 额外防御
                        // 注意：这里绝对不能写 Player.immune = true;
                    }
                    else
                    {
                        // 【原著模式】
                        // 物理免疫 (无敌)
                        Player.immune = true;
                    }

                    // 视觉效果保持一致
                    Lighting.AddLight(Player.Center, 0.6f, 0.6f, 0.8f);
                }
            }

            // 嫁接模式特效
            if (graftingMode != 0)
            {
                // 持续消耗灵性维持嫁接概念
                if (!TryConsumeSpirituality(2.0f, true))
                {
                    graftingMode = 0;
                    Main.NewText("嫁接中断。", 150, 150, 150);
                }
            }
            if (weatherRitualTimer > 0)
            {
                weatherRitualTimer--;
                if (weatherRitualTimer == 0 && !weatherRitualComplete)
                {
                    if (weatherRitualCount > 0)
                        Main.NewText("符文共鸣消散了... (需快速连续触发)", 200, 200, 200);
                    weatherRitualCount = 0;
                }
            }
            // --- 窃取惩罚逻辑 ---
            if (stealAggroTimer > 0)
            {
                stealAggroTimer--;

                // 每秒 (60帧) 受到一次攻击
                if (stealAggroTimer % 60 == 0)
                {
                    Player.Hurt(PlayerDeathReason.ByCustomReason("被愤怒的店主暴打！"), 20, 0);
                    SoundEngine.PlaySound(SoundID.Item14, Player.position);
                    for (int i = 0; i < 10; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke, 0, 0, 0, default, 1.5f);
                }

                // 惩罚期间强制断开对话
                if (Player.talkNPC != -1)
                {
                    Player.SetTalkNPC(-1);
                    Main.playerInventory = false;
                    stealMode = false;
                }
            }

            // 【核心修复】自动关闭逻辑
            // 只有当完全结束对话（talkNPC == -1）时才自动关闭。
            // 这样允许你在对话界面开启模式，然后再点“商店”按钮。
            if (Player.talkNPC == -1)
            {
                stealMode = false;
            }

            // 状态自动解除
            if (currentSequence > 3) isMercuryForm = false;
            if (currentHunterSequence > 4) { isFireForm = false; isArmyOfOne = false; }
            if (currentHunterSequence > 2) isCalamityGiant = false;
            if (currentMoonSequence > 7) isVampireWings = false;
            if (currentMoonSequence > 5) { isFullMoonActive = false; isMoonlightized = false; }
            if (currentMoonSequence > 4) isBatSwarm = false;
            if (currentMoonSequence > 2) isCreationDomain = false;

            arsonistFireImmune = false;

            ApplySequenceStats();
            ApplyMarauderStats();
            CheckConquerorRitual();

        }

        // ===================================================
        // 4. 数值加成系统
        // ===================================================
        private void ApplySequenceStats()
        {
            // 1. 获取动态世界等级系数 (平衡性系统的核心)
            // 需要先创建 Content/Systems/BalanceSystem.cs (参考上一步的回答)
            // 如果您还没创建 BalanceSystem，这里会报错，请先去创建那个文件
            float worldMult = Systems.BalanceSystem.GetWorldTierMultiplier();

            float giantMult = GetSequenceMultiplier(currentSequence);
            float hunterMult = GetSequenceMultiplier(currentHunterSequence);
            float moonMult = GetSequenceMultiplier(currentMoonSequence);
            float foolMult = GetSequenceMultiplier(currentFoolSequence);
            float marauderMult = GetSequenceMultiplier(currentMarauderSequence);

            // 2. 动态调整血量、伤害、防御的基础倍率
            // 原来的逻辑是直接 +5000 血，现在改为：世界越强，加成越高

            // --- 通用成长 ---
            float maxMult = Math.Max(giantMult, Math.Max(hunterMult, Math.Max(moonMult, foolMult)));
            if (maxMult > 1f)
            {
                Player.moveSpeed += 0.15f * maxMult;
                Player.maxRunSpeed += 1.5f * maxMult;
                Player.jumpSpeedBoost += 1.2f * (maxMult - 1f);
            }

            // ==========================================
            // 动态数值应用 (替换原版死数值)
            // ==========================================

            // 举例：如果您是序列1 (mult约为3.0)，且世界刚开局 (worldMult=0.2)
            // 最终加成 = 3.0 * 0.2 = 0.6倍，不会太离谱
            // 如果打完灾厄 (worldMult=5.0)，最终加成 = 15.0倍，足以抗衡神吞

            // --- 巨人/战士 ---
            if (currentSequence <= 9) { Player.statDefense += (int)(8 * giantMult * worldMult); Player.GetDamage(DamageClass.Melee) += 0.12f * giantMult; Player.GetCritChance(DamageClass.Melee) += 5; Player.statLifeMax2 += (int)(100 * giantMult * worldMult); }
            if (currentSequence <= 8) { Player.GetAttackSpeed(DamageClass.Melee) += 0.15f; Player.endurance += 0.05f; Player.noKnockback = true; }
            if (currentSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.10f; Player.GetCritChance(DamageClass.Generic) += 5; Player.GetArmorPenetration(DamageClass.Generic) += 10 * giantMult; }
            if (currentSequence <= 6)
            {
                Lighting.AddLight(Player.Center, 1.5f, 1.5f, 1.5f);
                Player.statDefense += (int)(15 * worldMult);
                Player.lifeRegen += (int)(3 * giantMult);
                if (dawnArmorActive && !dawnArmorBroken)
                {
                    Player.statDefense += (int)(40 * giantMult * worldMult);
                    Player.endurance += 0.15f * giantMult;
                }
            }
            if (currentSequence <= 5)
            {
                Player.statDefense += (int)(20 * worldMult);
                Player.endurance += 0.05f;
                Player.buffImmune[BuffID.Confused] = true;
                if (isGuardianStance)
                {
                    Player.statDefense += (int)(100 * giantMult * worldMult);
                    Player.endurance += 0.3f * giantMult;
                }
            }
            if (currentSequence <= 4) { Player.statLifeMax2 += (int)(500 * giantMult * worldMult); Player.GetDamage(DamageClass.Generic) += 0.20f; Player.GetCritChance(DamageClass.Generic) += 10; Player.nightVision = true; Player.detectCreature = true; Player.buffImmune[BuffID.CursedInferno] = true; Player.buffImmune[BuffID.ShadowFlame] = true; }
            if (currentSequence <= 3) { Player.statDefense += (int)(30 * worldMult); Player.lifeRegen += 5; Player.GetAttackSpeed(DamageClass.Melee) += 0.20f; Player.blackBelt = true; }
            if (currentSequence <= 2) { Player.statLifeMax2 += (int)(2000 * worldMult); Player.statDefense += (int)(50 * worldMult); Player.endurance += 0.15f; Player.GetDamage(DamageClass.Generic) += 0.20f; }

            // --- 猎人途径 ---
            if (currentHunterSequence <= 9) { Player.GetDamage(DamageClass.Ranged) += 0.15f; Player.GetDamage(DamageClass.Melee) += 0.05f; Player.detectCreature = true; Player.dangerSense = true; }
            if (currentHunterSequence <= 8) { Player.statDefense += 10; Player.aggro += 300; Player.lifeRegen += 2; }
            if (currentHunterSequence <= 7){Player.GetDamage(DamageClass.Generic) += 0.15f * hunterMult;Player.buffImmune[BuffID.OnFire] = true; Player.buffImmune[BuffID.OnFire3] = true;Player.buffImmune[BuffID.Frostburn] = true;Player.resistCold = true;Player.lavaImmune = true;Player.fireWalk = true;}
            if (currentHunterSequence <= 6) { Player.GetCritChance(DamageClass.Generic) += 15; Player.manaCost -= 0.20f; }
            if (currentHunterSequence <= 5) { Player.GetArmorPenetration(DamageClass.Generic) += 30 * hunterMult; Player.GetCritChance(DamageClass.Generic) += 20; }
            if (currentHunterSequence <= 4) { Player.statDefense += (int)(50 * hunterMult * worldMult); Player.endurance += 0.10f; Player.maxMinions += 5; Player.noKnockback = true; }
            if (currentHunterSequence <= 3) { Player.maxMinions += 10; Player.maxTurrets += 3; Player.GetDamage(DamageClass.Summon) += 0.40f; }
            if (currentHunterSequence <= 2) { Player.statLifeMax2 += (int)(600 * worldMult); Player.statManaMax2 += 300; Player.GetDamage(DamageClass.Generic) += 0.40f; Player.buffImmune[BuffID.WindPushed] = true; }
            if (currentHunterSequence <= 1) { Player.statDefense += (int)(100 * worldMult); Player.endurance += 0.25f; Player.GetDamage(DamageClass.Generic) += 1.0f; Player.GetCritChance(DamageClass.Generic) += 40; Player.aggro += 2000; Player.buffImmune[BuffID.Weak] = true; Player.buffImmune[BuffID.BrokenArmor] = true; Player.buffImmune[BuffID.WitheredArmor] = true; Player.buffImmune[BuffID.WitheredWeapon] = true; }

            // --- 月亮途径 ---
            if (currentMoonSequence <= 9)
            {
                Player.buffImmune[BuffID.Poisoned] = true;
                Player.buffImmune[BuffID.Venom] = true;
                Player.detectCreature = true;
                Player.lifeRegen += (int)(3 * moonMult);
                Player.statLifeMax2 += (int)(30 * moonMult * worldMult);
            }
            if (currentMoonSequence <= 8) { Player.statDefense += 8; Player.GetDamage(DamageClass.Generic) += 0.15f * moonMult; Player.moveSpeed += 0.3f; Player.maxMinions += (int)(2 * moonMult); Player.dangerSense = true; }
            if (currentMoonSequence <= 7)
            {
                Player.statLifeMax2 += (int)(100 * worldMult);
                Player.lifeRegen += (int)(8 * moonMult);
                Player.moveSpeed += 0.3f;
                Player.noFallDmg = true;
                if (Main.dayTime && Player.ZoneOverworldHeight && Player.behindBackWall == false) { Player.statDefense -= 10; Player.lifeRegen -= 5; Player.GetDamage(DamageClass.Generic) -= 0.1f; }
            }
            if (currentMoonSequence <= 6) { Player.statManaMax2 += 100; Player.GetDamage(DamageClass.Magic) += 0.20f * moonMult; Player.pStone = true; Player.buffImmune[BuffID.OnFire] = true; Player.buffImmune[BuffID.Frostburn] = true; Player.buffImmune[BuffID.CursedInferno] = true; }
            if (currentMoonSequence <= 5)
            {
                Player.lifeRegen += 20;
                Player.buffImmune[BuffID.Confused] = true; Player.buffImmune[BuffID.Darkness] = true; Player.buffImmune[BuffID.Silenced] = true; Player.buffImmune[BuffID.Blackout] = true;
                if (isFullMoonActive) { Player.GetDamage(DamageClass.Magic) += 0.40f * moonMult; Player.statDefense -= 10; Lighting.AddLight(Player.Center, 0.6f, 0.7f, 0.9f); Player.manaRegen += (int)(30 * moonMult); Player.manaRegenDelayBonus += 5; }
            }
            if (currentMoonSequence <= 4) { Player.statLifeMax2 += (int)(300 * worldMult); Player.statManaMax2 += 200; Player.GetDamage(DamageClass.Magic) += 0.30f * moonMult; Player.GetDamage(DamageClass.Summon) += 0.30f * moonMult; }
            if (currentMoonSequence <= 3) { Player.maxMinions += (int)(5 * moonMult); Player.GetDamage(DamageClass.Summon) += 0.40f * moonMult; Player.GetKnockback(DamageClass.Summon) += 3f; Player.statManaMax2 += 300; Player.manaCost -= 0.25f; }
            if (currentMoonSequence <= 2)
            {
                Player.statLifeMax2 += (int)(1500 * worldMult);
                Player.lifeRegen += 60;
                Player.buffImmune[BuffID.Bleeding] = true; Player.buffImmune[BuffID.Poisoned] = true; Player.buffImmune[BuffID.Venom] = true; Player.buffImmune[BuffID.CursedInferno] = true; Player.buffImmune[BuffID.Ichor] = true; Player.buffImmune[BuffID.Frozen] = true;
                if (isCreationDomain) { Player.lifeRegen += 60; Player.statDefense += (int)(50 * worldMult); Lighting.AddLight(Player.Center, 0.1f, 0.8f, 0.2f); Player.flowerBoots = true; }
            }
            if (currentMoonSequence <= 1)
            {
                Player.statLifeMax2 += (int)(3000 * worldMult);
                Player.endurance += 0.20f;
                Player.GetDamage(DamageClass.Generic) += 0.80f;
                for (int i = 0; i < BuffID.Count; i++) { if (Main.debuff[i]) Player.buffImmune[i] = true; }
                if (isCreationDomain) { Player.GetDamage(DamageClass.Generic) += 0.50f; Lighting.AddLight(Player.Center, 1.0f, 0.4f, 0.7f); }
            }

            // --- 愚者途径 (The Fool) ---
            if (currentFoolSequence <= 9) { Player.GetDamage(DamageClass.Magic) += 0.10f * foolMult; Player.GetCritChance(DamageClass.Magic) += 5; Player.statManaMax2 += (int)(40 * foolMult); Player.dangerSense = true; if (isSpiritVisionActive) { if (!TryConsumeSpirituality(0.1f, true)) { isSpiritVisionActive = false; Main.NewText("灵性枯竭，灵视被迫中断！", 255, 50, 50); } else { Lighting.AddLight(Player.Center, 0.4f, 0.4f, 1.0f); Player.findTreasure = true; } } Player.luck += 0.5f * foolMult; }
            if (currentFoolSequence <= 8) { Player.moveSpeed += 0.3f; Player.jumpSpeedBoost += 1.5f; Player.accRunSpeed += 2.0f; Player.GetDamage(DamageClass.Generic) += 0.15f * foolMult; Player.GetAttackSpeed(DamageClass.Melee) += 0.15f; Player.GetCritChance(DamageClass.Generic) += 10; Player.blackBelt = true; Player.statManaMax2 += (int)(60 * foolMult); }
            if (currentFoolSequence <= 7)
            {
                Player.GetAttackSpeed(DamageClass.Generic) += 0.2f;
                Player.manaCost -= 0.15f;
                Player.buffImmune[BuffID.Webbed] = true;
                Player.buffImmune[BuffID.Stoned] = true;
                if (Player.wet) Player.gills = true;

                Player.ignoreWater = true;
                Player.statManaMax2 += (int)(100 * foolMult);
            }
            if (currentFoolSequence <= 6) { Player.accCritterGuide = true; Player.accStopwatch = true; Player.accOreFinder = true; Player.GetDamage(DamageClass.Generic) += 0.15f * foolMult; Player.GetCritChance(DamageClass.Generic) += 10; Player.GetDamage(DamageClass.Magic) += 0.3f * foolMult; Player.gills = true; if (isFacelessActive) { Player.aggro -= 1000; Player.shroomiteStealth = true; Player.statDefense += 10; if (!TryConsumeSpirituality(1.0f, true)) { isFacelessActive = false; Main.NewText("灵性不足，伪装失效！", 255, 50, 50); } } Player.statManaMax2 += (int)(150 * foolMult); }
            if (currentFoolSequence <= 5) { Player.detectCreature = true; Player.dangerSense = true; Player.findTreasure = true; Player.maxMinions += 3; Player.GetDamage(DamageClass.Magic) += 0.2f * foolMult; Player.statManaMax2 += (int)(200 * foolMult); }
            if (currentFoolSequence <= 4)
            {
                Player.statLifeMax2 += (int)(100 * worldMult); // 砍半
                Player.statDefense += 20;
                Player.GetDamage(DamageClass.Generic) += 0.2f * foolMult;
                Player.maxMinions += 7;
                Player.aggro -= 2000;
                Player.statManaMax2 += (int)(400 * foolMult);
            }
            if (currentFoolSequence <= 3)
            {
                Player.statLifeMax2 += (int)(100 * worldMult); // 砍半
                Player.statDefense += (int)(40 * worldMult);
                Player.GetDamage(DamageClass.Generic) += 0.4f * foolMult;
                if (isBorrowingPower)
                {
                    Player.GetDamage(DamageClass.Generic) += 0.5f;
                    Player.statDefense += 50;
                    Player.lifeRegen += 20;
                    Player.moveSpeed += 0.5f;
                    Player.endurance += 0.2f;
                    if (Main.rand.NextBool(3)) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke, 0, 0, 100, Color.Gray, 1.5f);
                }
            }
            if (currentFoolSequence <= 2)
            {
                Player.statLifeMax2 += (int)(100 * worldMult); // 砍半
                Player.statDefense += (int)(60 * worldMult);
                Player.GetDamage(DamageClass.Generic) += 0.5f * foolMult;

                if (fateDisturbanceActive)
                {
                    Player.GetCritChance(DamageClass.Generic) += 50;
                    Player.luck += 1.0f;
                    if (!TryConsumeSpirituality(2.0f, true))
                    {
                        fateDisturbanceActive = false;
                        Main.NewText("灵性不足，命运干扰停止。", 150, 150, 150);
                    }
                }
                Player.statManaMax2 += 1000;
            }
            if (currentFoolSequence <= 1)
            {
                Player.statLifeMax2 += (int)(100 * worldMult); // 砍半
                Player.statDefense += (int)(100 * worldMult);
                Player.GetDamage(DamageClass.Generic) += 1.0f * foolMult;
                Player.statManaMax2 += 1000;

                if (graftingMode == 2)
                {
                    Player.GetCritChance(DamageClass.Generic) += 100;
                    Player.GetArmorPenetration(DamageClass.Generic) += 9999;
                }
            }
            // ==========================================
            if (currentMarauderSequence <= 9) // 偷盗者
            {
                // 60 -> 30
                Player.statLifeMax2 += (int)(30 * worldMult);
                Player.moveSpeed += 0.05f;
            }

            if (currentMarauderSequence <= 8) // 诈骗师
            {
                // 100 -> 50
                Player.statLifeMax2 += (int)(50 * worldMult);
                Player.statDefense += (int)(4 * worldMult);
            }

            if (currentMarauderSequence <= 7) // 解密学者
            {
                // 150 -> 75
                Player.statLifeMax2 += (int)(75 * worldMult);
                Player.GetCritChance(DamageClass.Generic) += 5;
                Player.GetArmorPenetration(DamageClass.Generic) += 5;
            }
            if (currentMarauderSequence <= 6) // 盗火人
            {
                // 250 -> 125
                Player.statLifeMax2 += (int)(125 * worldMult);
                Player.statDefense += (int)(10 * worldMult);
                Player.GetDamage(DamageClass.Generic) += 0.05f;
            }

            if (currentMarauderSequence <= 5) // 窃梦家
            {
                // 400 -> 200
                Player.statLifeMax2 += (int)(200 * worldMult);
                Player.statManaMax2 += 50;
                Player.endurance += 0.05f;
            }

            if (currentMarauderSequence <= 4) // 寄生者 (半神)
            {
                // 800 -> 400
                Player.statLifeMax2 += (int)(400 * worldMult);
                Player.statDefense += (int)(20 * worldMult);
                Player.lifeRegen += 3;
            }

            if (currentMarauderSequence <= 3) // 欺瞒导师
            {
                // 1200 -> 600
                Player.statLifeMax2 += (int)(600 * worldMult);
                Player.GetDamage(DamageClass.Generic) += 0.10f;
                Player.GetCritChance(DamageClass.Generic) += 5;
            }

            // --- 天使 (2-1) ---
            if (currentMarauderSequence <= 2) // 命运木马 (天使)
            {
                // 2000 -> 1000
                Player.statLifeMax2 += (int)(1000 * worldMult);
                Player.statDefense += (int)(30 * worldMult);
                Player.statManaMax2 += 100;
                Player.endurance += 0.05f;
            }

            if (currentMarauderSequence <= 1) // 时之虫 (天使之王)
            {
                // 3500 -> 1750 (典型的脆皮高攻)
                Player.statLifeMax2 += (int)(1750 * worldMult);

                Player.statDefense += (int)(40 * worldMult);
                Player.GetDamage(DamageClass.Generic) += 0.15f;
                Player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
                Player.endurance += 0.05f;
                Player.manaCost -= 0.25f;
            }
        }
        private void ProcessRealmOfMysteries()
        {
            // 1. 缓存变量
            Vector2 playerCenter = Player.Center;
            float rangeSQ = realmRange * realmRange; // 平方距离
            int spiritDebuff = ModContent.BuffType<Buffs.SpiritControlDebuff>();

            // 2. A. 压制敌人
            // 使用 for 循环遍历 ActiveNPCs，性能微优于 foreach
            foreach (NPC npc in Main.ActiveNPCs)
            {
                // 使用 DistanceSQ 代替 Distance，省去开根号运算，性能提升很大
                if (!npc.friendly && !npc.dontTakeDamage && npc.Center.DistanceSQ(playerCenter) < rangeSQ)
                {
                    if (!npc.HasBuff(BuffID.Slow)) npc.AddBuff(BuffID.Slow, 60);
                    if (!npc.HasBuff(spiritDebuff)) npc.AddBuff(spiritDebuff, 60);

                    if (Main.rand.NextBool(50)) npc.AddBuff(BuffID.Confused, 120);
                }
            }

            // 3. B. 再生：吞噬掉落物
            // 使用 for 循环遍历数组，比 foreach 更快且无垃圾回收(GC)压力
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                
                // 先判断 active，这步最快，能过滤掉绝大多数空槽位
                if (!item.active || item.value <= 0) continue;

                // 距离判定优化
                float distSQ = item.Center.DistanceSQ(playerCenter);
                
                if (distSQ < rangeSQ)
                {
                    // 吸取逻辑：让物品飞向玩家
                    item.velocity = (playerCenter - item.Center).SafeNormalize(Vector2.Zero) * 15f;

                    // 接触判定 (60 * 60 = 3600)
                    if (distSQ < 3600f)
                    {
                        int value = item.value * item.stack;
                        int heal = Math.Max(1, value / 1000);
                        if (heal > 50) heal = 50;

                        Player.statLife += heal;
                        Player.HealEffect(heal);

                        // 彻底删除
                        item.TurnToAir();
                        item.active = false;

                        // 限制粒子数量
                        if (Main.rand.NextBool(3)) 
                        {
                             Dust d = Dust.NewDustPerfect(playerCenter, DustID.SpectreStaff, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.5f);
                             d.noGravity = true;
                        }
                    }
                }
            }
        }
        private void ApplyMarauderStats()
        {
            float marauderMult = GetSequenceMultiplier(currentMarauderSequence);

            // 1. 序列9 偷盗者
            if (currentMarauderSequence <= 9)
            {
                Player.pickSpeed -= 0.1f * marauderMult;
                Player.wallSpeed += 0.1f * marauderMult;
                Player.tileSpeed += 0.1f * marauderMult;
                Player.treasureMagnet = true; 
                Player.goldRing = true;
                Player.findTreasure = true;
            }

            // 2. 序列8 诈骗师
            if (currentMarauderSequence <= 8)
            {
                Player.discountAvailable = true;
                Player.moveSpeed += 0.2f;
                Player.runAcceleration += 0.1f;
            }

            // 3. 序列7 解密学者
            if (currentMarauderSequence <= 7)
            {
                Player.detectCreature = true;
                Player.dangerSense = true;
                Player.GetArmorPenetration(DamageClass.Generic) += 10;
                Player.GetCritChance(DamageClass.Generic) += 5;
            }

            // 4. 序列6 盗火人
            if (currentMarauderSequence <= 6)
            {
                Player.statDefense += 15;
                Player.moveSpeed += 0.4f;
                Player.GetDamage(DamageClass.Generic) += 0.15f;
                Player.lifeRegen += 3;
                Player.buffImmune[BuffID.Confused] = true;
                Player.buffImmune[BuffID.Darkness] = true;
                Player.buffImmune[BuffID.Blackout] = true;
                Player.buffImmune[BuffID.Obstructed] = true;
                Player.findTreasure = true;
                Lighting.AddLight(Player.Center, 0.8f, 0.6f, 0.0f);
            }

            // 5. 序列5 窃梦家
            if (currentMarauderSequence <= 5)
            {
                Player.manaMagnet = true;
                Player.lifeMagnet = true;

                // 梦魇光环
                int auraRadius = 300;
                foreach (NPC target in Main.ActiveNPCs)
                {
                    if (target.active && !target.friendly && !target.dontTakeDamage && target.Distance(Player.Center) < auraRadius)
                    {
                        target.AddBuff(BuffID.Slow, 10);
                        target.AddBuff(BuffID.Ichor, 10);
                        if (Main.rand.NextBool(30)) Dust.NewDust(target.position, target.width, target.height, DustID.DungeonSpirit, 0, 0, 150, default, 0.8f);
                    }
                }

                // 盗天机
                bool inCombat = (Player.aggro > -1000 && Player.itemAnimation > 0);
                if (inCombat && Main.GameUpdateCount % 180 == 0 && Main.rand.NextBool(3))
                {
                    int[] stolenBuffs = { BuffID.Rage, BuffID.Wrath, BuffID.Endurance, BuffID.Lifeforce, BuffID.Ironskin, BuffID.Regeneration, BuffID.MagicPower, BuffID.Titan };
                    int buff = stolenBuffs[Main.rand.Next(stolenBuffs.Length)];
                    Player.AddBuff(buff, 600);
                    CombatText.NewText(Player.getRect(), new Color(100, 149, 237), "盗天机!", false, true);
                }
            }
        }
        // ===================================================
        // 5. 视觉特效与翅膀
        // ===================================================
        public override void FrameEffects()
        {
            if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken) { int dyeId = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BrightSilverDye); Player.cHead = dyeId; Player.cBody = dyeId; Player.cLegs = dyeId; }
            if (isVampireWings) { Player.wings = 12; int blackDyeId = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BlackDye); Player.cWings = blackDyeId; }
            if (isMoonlightized) { int redDyeId = GameShaders.Armor.GetShaderIdFromItemId(ItemID.RedAcidDye); Player.cHead = redDyeId; Player.cBody = redDyeId; Player.cLegs = redDyeId; }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (isBatSwarm)
            {
                // 1. 基础透明度设为 0 (完全不可见)
                a = 0f;
                // 告诉游戏引擎玩家处于隐身状态 (移除头部箭头等UI)
                drawInfo.drawPlayer.invis = true;

                // 2. 移除所有基础身体部位
                drawInfo.drawPlayer.head = -1;
                drawInfo.drawPlayer.body = -1;
                drawInfo.drawPlayer.legs = -1;

                // 3. 【核心修复】移除所有饰品、装备、翅膀
                // 如果不加这些，玩家变成蝙蝠时，翅膀和鞋子还会飘在空中
                drawInfo.drawPlayer.handon = -1;  // 手套 (前)
                drawInfo.drawPlayer.handoff = -1; // 手套 (后)
                drawInfo.drawPlayer.back = -1;    // 背部
                drawInfo.drawPlayer.front = -1;   // 胸前
                drawInfo.drawPlayer.shoe = -1;    // 鞋子
                drawInfo.drawPlayer.waist = -1;   // 腰带
                drawInfo.drawPlayer.shield = -1;  // 盾牌
                drawInfo.drawPlayer.neck = -1;    // 脖子
                drawInfo.drawPlayer.face = -1;    // 脸部饰品
                drawInfo.drawPlayer.balloon = -1; // 气球
                drawInfo.drawPlayer.wings = -1;   // 【重要】隐藏翅膀

                // 4. 【核心修复】隐藏手持物品
                // 将绘制信息中的物品设为空，这样就不会画出剑或枪了
                drawInfo.heldItem = null;
            }
            if (isBatSwarm)
            {
                // 既然化身蝙蝠了，就不能用人类的道具了
                // 这样也能防止手里拿着东西导致隐身穿帮
                Player.noItems = true;
            }
            if (graftingMode != 0)
            {
                if (graftingMode == 1)
                {
                    r *= 0.8f;
                    g *= 0.5f;
                    b *= 1.0f;
                    if (!isSpiritForm) a = 0.8f;
                }
                else if (graftingMode == 2)
                {
                    r *= 1.0f;
                    g *= 0.5f;
                    b *= 0.5f;
                }
            }
            if (isCalamityGiant || isFireForm || isMercuryForm || isMoonlightized) { drawInfo.hideEntirePlayer = true; return; }
            if (currentSequence <= 3 && stealthTimer > 60) { drawInfo.hideEntirePlayer = true; drawInfo.shadow = 0f; a = 0f; return; }
            if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken && !Player.shroomiteStealth) { r = 2.0f; g = 2.0f; b = 2.0f; fullBright = true; if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Player.Center, DustID.Enchanted_Gold, Vector2.Zero, 0, default, 0.8f).noGravity = true; }

            if (currentMoonSequence <= 1) { Lighting.AddLight(Player.Center, 0.8f, 0.4f, 0.6f); if (Main.rand.NextBool(10)) Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.PinkTorch, 0, 0, 100, default, 1.0f).noGravity = true; }
        }

        public override void PostUpdateMiscEffects()
        {
            float moonMult = GetSequenceMultiplier(currentMoonSequence);
            float giantMult = GetSequenceMultiplier(currentSequence);
            float hunterMult = GetSequenceMultiplier(currentHunterSequence);

            // 美神被动
            if (currentMoonSequence <= 1) { foreach (NPC npc in Main.ActiveNPCs) { if (!npc.friendly && !npc.dontTakeDamage && !npc.boss && npc.Distance(Player.Center) < 800f) { npc.AddBuff(BuffID.Confused, 2); npc.AddBuff(BuffID.Lovestruck, 2); npc.damage = (int)(npc.defDamage * 0.5f); } } }

            // 猎人/巨人特效
            if (isCalamityGiant) { if (!TryConsumeSpirituality(100.0f, true)) isCalamityGiant = false; else { Player.statDefense += 300; Player.wingsLogic = 0; Player.wingTime = 9999; Player.gravity = 0f; Player.statLifeMax2 += 3000; Player.invis = true; if (Main.rand.NextBool(2)) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Electric, 0, 0, 0, default, 1.5f); } }
            if (isArmyOfOne) { if (currentHunterSequence <= 4 && TryConsumeSpirituality(5.0f, true)) { int bonusMinions = 5; if (currentHunterSequence <= 1) bonusMinions = 40; else if (currentHunterSequence <= 2) bonusMinions = 20; else if (currentHunterSequence <= 3) bonusMinions = 10; Player.maxMinions += bonusMinions; } else { isArmyOfOne = false; } }

            if (isFireForm)
            {
                if (!TryConsumeSpirituality(50.0f, true)) { isFireForm = false; Main.NewText("灵性枯竭！", 255, 50, 50); return; }
                Player.noKnockback = true; Player.wingsLogic = 0; Player.wingTime = 9999; Player.rocketTime = 9999; Player.noItems = true;
                if (currentHunterSequence <= 1) { Player.moveSpeed += 5.0f; Player.maxRunSpeed += 30f; Player.runAcceleration *= 10f; Player.jumpSpeedBoost += 30f; Player.statDefense += 200; Player.endurance += 0.4f; Vector2 tip = Player.Center + Player.velocity * 2f; for (int i = 0; i < 5; i++) { Vector2 pos = Vector2.Lerp(Player.Center, tip, i / 5f) + Main.rand.NextVector2Circular(10, 10); int d = Dust.NewDust(pos, 0, 0, DustID.Shadowflame, 0, 0, 0, default, 2.5f); Main.dust[d].noGravity = true; Main.dust[d].velocity = -Player.velocity * 0.2f; } Rectangle myRect = Player.getRect(); myRect.Inflate(40, 40); if (Player.velocity.Length() > 5f) { for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(myRect)) { if (npc.immune[Player.whoAmI] <= 0) { int dashDamage = (int)(Player.GetDamage(DamageClass.Melee).ApplyTo(3000) * hunterMult); Player.ApplyDamageToNPC(npc, dashDamage, 30f, Player.direction, true); npc.AddBuff(ModContent.BuffType<ConquerorWill>(), 600); npc.immune[Player.whoAmI] = 6; SoundEngine.PlaySound(SoundID.Item74, npc.Center); for (int k = 0; k < 20; k++) Dust.NewDust(npc.position, npc.width, npc.height, DustID.Shadowflame, 0, 0, 0, default, 3f); } } } } } else { Player.moveSpeed += 3.0f; Player.maxRunSpeed += 15f; Player.runAcceleration *= 4f; Player.jumpSpeedBoost += 20f; Player.gravity *= 0.5f; Player.statDefense += 100; for (int i = 0; i < 3; i++) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.SolarFlare, 0, 0, 0, default, 2f); Main.dust[d].noGravity = true; Main.dust[d].velocity = Player.velocity * 0.5f; } }
            }

            // 月亮特效
            if (isVampireWings) { if (!TryConsumeSpirituality(0.5f, true)) { isVampireWings = false; Main.NewText("灵性耗尽，黑暗之翼消散！", 200, 50, 50); return; } Player.wingTime = 1000; Player.wingTimeMax = 1000; Player.wingsLogic = 12; Player.noFallDmg = true; if (Main.rand.NextBool(4)) Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Shadowflame, 0, 0, 100).noGravity = true; }
            if (isMoonlightized) { if (!TryConsumeSpirituality(4f, true)) { isMoonlightized = false; Main.NewText("灵性耗尽！", 200, 50, 50); return; } Player.immune = true; Player.immuneTime = 2; Player.noItems = true; Player.noKnockback = true; Player.invis = true; Player.maxRunSpeed += 10f; Player.moveSpeed += 2.0f; if (Main.rand.NextBool(2)) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.VampireHeal, Player.velocity.X, Player.velocity.Y, 100, default, 1.5f); Main.dust[d].noGravity = true; } }
            if (isFullMoonActive) { if (!TryConsumeSpirituality(0.1f, true)) { isFullMoonActive = false; return; } Lighting.AddLight(Player.Center, 0.6f, 0.7f, 0.9f); if (Player.ownedProjectileCounts[ModContent.ProjectileType<FullMoonCircle>()] < 1) { Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<FullMoonCircle>(), 0, 0, Player.whoAmI); } if (Main.rand.NextBool(10)) { Vector2 pos = Player.Center + Main.rand.NextVector2Circular(100, 100); Dust d = Dust.NewDustPerfect(pos, DustID.BlueCrystalShard, Vector2.Zero, 150, default, 1.0f); d.noGravity = true; } }
            if (isTamingActive)
            {
                // 1. 每秒消耗 2 点灵性 (2.0f / 60f 每帧)
                if (!TryConsumeSpirituality(2.0f / 60f, true))
                {
                    isTamingActive = false;
                    Main.NewText("灵性不足，驯兽模式被迫中断。", 255, 50, 50);
                }

                // 2. 添加一个视觉提示 (绿色光环)，防止忘记关技能
                if (Main.rand.NextBool(10))
                {
                    Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Terra, 0, 0, 0, default, 1.0f);
                    d.noGravity = true;
                    d.velocity *= 0.5f;
                }
            }
            // 蝙蝠化身
            if (isBatSwarm)
            {
                if (!TryConsumeSpirituality(0.5f, true)) { isBatSwarm = false; Main.NewText("灵性耗尽，蝙蝠化身解除！", 200, 50, 50); return; }
                Player.mount.Dismount(Player); Player.noFallDmg = true; Player.noKnockback = true; Player.wingTime = 1000; Player.wingsLogic = 12; Player.noItems = true; Player.invis = true;
                int maxBats = 60; int targetBatCount = (int)(maxBats * ((float)Player.statLife / Player.statLifeMax2));
                if (targetBatCount < 1) { isBatSwarm = false; Main.NewText("重伤解除！", 255, 50, 50); return; }
                int currentBats = Player.ownedProjectileCounts[ModContent.ProjectileType<BatSwarmProjectile>()];
                if (currentBats < targetBatCount) { Vector2 spawnPos = Player.Center + Main.rand.NextVector2Circular(20, 20); Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<BatSwarmProjectile>(), (int)(300 * moonMult), 2f, Player.whoAmI); }
                else if (currentBats > targetBatCount) { int toKill = currentBats - targetBatCount; for (int i = 0; i < Main.maxProjectiles; i++) { Projectile p = Main.projectile[i]; if (p.active && p.owner == Player.whoAmI && p.type == ModContent.ProjectileType<BatSwarmProjectile>()) { p.Kill(); toKill--; if (toKill <= 0) break; } } }
            }

            // 创生领域
            if (isCreationDomain)
            {
                if (!TryConsumeSpirituality(1.0f, true)) { isCreationDomain = false; Main.NewText("灵性枯竭，领域消散", 100, 255, 100); return; }
                if (Player.ownedProjectileCounts[ModContent.ProjectileType<CreationDomainProjectile>()] < 1) { Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<CreationDomainProjectile>(), 0, 0, Player.whoAmI); }
                Lighting.AddLight(Player.Center, 0.2f, 0.8f, 0.3f);
                if (Main.GameUpdateCount % 60 == 0)
                {
                    foreach (NPC npc in Main.ActiveNPCs) { if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 1000f) { int dmg = (int)(npc.lifeMax * 0.05f); if (currentMoonSequence <= 1) dmg = (int)(npc.lifeMax * 0.1f); if (dmg > 20000) dmg = 20000; npc.SimpleStrikeNPC(dmg, 0, true, 0, DamageClass.Magic, true); } }
                    for (int i = 0; i < Main.maxPlayers; i++) { Player p = Main.player[i]; if (p.active && !p.dead && p.Distance(Player.Center) < 800f) { p.HealEffect(50); p.statLife += 50; if (p.statLife > p.statLifeMax2) p.statLife = p.statLifeMax2; } }
                }
            }
            if (currentFoolSequence <= 2 && fateDisturbanceActive)
            {
                if (Main.GameUpdateCount % 20 == 0)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 pos = Player.Center + Main.rand.NextVector2Circular(600, 600);
                        Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, Vector2.Zero, 150, default, 0.5f);
                        d.noGravity = true;
                    }
                }
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 600f)
                    {
                        npc.damage = (int)(npc.defDamage * 0.5f);
                        npc.AddBuff(BuffID.Confused, 2);
                        npc.AddBuff(BuffID.Midas, 2); // 掉落增加 (好运)
                    }
                }
            }
            // =================================================
            // 序列1：时之虫领域 (Time Clock Domain)
            // =================================================
            if (isTimeClockActive)
            {
                if (!TryConsumeSpirituality(20.0f, true))
                {
                    isTimeClockActive = false;
                    Main.NewText("灵性枯竭，时钟虚影消散。", 150, 150, 150);
                    return;
                }

                if (Player.ownedProjectileCounts[ModContent.ProjectileType<TimeClockVisual>()] < 1)
                {
                    Projectile.NewProjectile(
                        Player.GetSource_FromThis(),
                        Player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<TimeClockVisual>(),
                        0,
                        0,
                        Player.whoAmI
                    );
                }
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 1000f) // 范围加大到 1000
                    {
                        npc.velocity *= 0.1f; // 速度变为原来的 10%，几乎走不动

                        // B. 衰老伤害 (每秒 60 次判定)
                        if (Main.GameUpdateCount % 20 == 0) // 频率提高：每秒跳 3 次伤害 (原为每秒1次)
                        {
                            int decay = 0;

                            if (npc.boss)
                            {
                                decay = (int)(npc.life * 0.005f) + 2000;
                                if (decay > 10000) decay = 10000;
                            }
                            else
                            {
                                decay = (int)(npc.life * 0.10f) + 5000;
                            }
                            Player.ApplyDamageToNPC(npc, decay, 0, 0, false);
                        }
                        npc.AddBuff(BuffID.Slow, 10);
                        npc.AddBuff(BuffID.WitheredArmor, 10); // 护甲衰老
                        npc.AddBuff(BuffID.WitheredWeapon, 10); // 攻击衰老
                        npc.AddBuff(BuffID.ShadowFlame, 10);    // 视觉特效
                    }
                }
            }

            if (isFireEnchanted) { if (currentHunterSequence <= 6 && TryConsumeSpirituality(0.16f, true)) { } else isFireEnchanted = false; }
            if (isFlameCloakActive)
            {
                if (currentHunterSequence <= 7 && TryConsumeSpirituality(1.0f, true))
                {
                    Player.buffImmune[BuffID.Chilled] = true;
                    Player.buffImmune[BuffID.Frozen] = true; // 顺便免疫冰冻
                    Player.statDefense += 8;
                    if (Main.rand.NextBool(3))
                    {
                        Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Torch, 0, 0, 100, default, 1.5f);
                        d.noGravity = true;
                        d.velocity *= 0.5f;
                    }
                    if (Main.GameUpdateCount % 10 == 0)
                    {
                        float range = 150f; // 范围
                        foreach (NPC npc in Main.ActiveNPCs)
                        {
                            if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < range)
                            {
                                npc.SimpleStrikeNPC(20, 0, true, 0, DamageClass.Generic); // 20点基础伤害
                                npc.AddBuff(BuffID.OnFire3, 120); // 狱火 Debuff
                            }
                        }
                    }
                }
                else isFlameCloakActive = false;
            }
            if (fireTeleportCooldown > 0) fireTeleportCooldown--;
            if (isMercuryForm) { if (!TryConsumeSpirituality(20.0f, true)) isMercuryForm = false; else { Player.moveSpeed += 2.0f; Player.invis = true; Rectangle myRect = Player.getRect(); myRect.Inflate(10, 10); foreach (NPC npc in Main.npc) { if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(myRect)) { if (npc.immune[Player.whoAmI] == 0) { int damage = (int)((Player.GetDamage(DamageClass.Melee).ApplyTo(50) * 5f) * giantMult); Player.ApplyDamageToNPC(npc, damage, 10f, Player.direction, false); npc.immune[Player.whoAmI] = 10; npc.AddBuff(BuffID.Slow, 300); npc.AddBuff(BuffID.Frostburn, 300); } } } } }
            if (currentSequence <= 3 && !isMercuryForm) { if (Player.velocity.Length() < 0.1f) { stealthTimer++; if (stealthTimer > 60) { Player.invis = true; Player.aggro -= 1000; } } else { stealthTimer = 0; } }
            if (isGuardianStance) { Player.velocity.X = 0; Player.statDefense += 80; Player.noKnockback = true; }
            if (glacierCooldown > 0) glacierCooldown--;
        }

        // 6. 攻击
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyHitEffects(target); CheckRitualKill(target); CheckExecution(target); if (currentMarauderSequence <= 9)
            {
                if (Main.rand.NextBool(5)) // 20% 几率
                {
                    target.value *= 1.2f; // 增加掉落钱币价值
                    // 制造一点金币特效
                    Dust.NewDust(target.position, target.width, target.height, DustID.GoldCoin, 0, 0, 0, default, 0.8f);
                }
                if (currentMarauderSequence <= 6)
                {
                    float baseChance = target.boss ? 0.002f : 0.02f;
                    float multiplier = 1f + (6 - currentMarauderSequence) * 0.3f;
                    float finalChance = baseChance * multiplier;

                    // [平衡性限制] 设置硬上限，防止概率溢出太离谱
                    // 普通怪上限 10%, Boss上限 1%
                    if (!target.boss && finalChance > 0.1f) finalChance = 0.1f;
                    if (target.boss && finalChance > 0.01f) finalChance = 0.01f;

                    // 2. 触发判定
                    int stealAttempts = (currentMarauderSequence <= 2) ? 6 : (currentMarauderSequence <= 3 ? 3 : 1);

                    // 这里的变量名改成了 n
                    for (int n = 0; n < stealAttempts; n++)
                    {
                        if (Main.rand.NextFloat() < finalChance && target.life > 0)
                        {
                            var dropInfo = new Terraria.GameContent.ItemDropRules.DropAttemptInfo
                            {
                                player = Player,
                                npc = target,
                                IsExpertMode = Main.expertMode,
                                IsMasterMode = Main.masterMode,
                                IsInSimulation = false,
                                rng = Main.rand
                            };
                            Main.ItemDropSolver.TryDropping(dropInfo);

                            // 只在第一次循环显示文字
                            if (n == 0)
                            {
                                string text = "窃取!";
                                if (currentMarauderSequence <= 2) text = "命运窃取 (x6)!"; // 序列 2/1 提示
                                else if (currentMarauderSequence <= 3) text = "三重窃取!";   // 序列 3 提示

                                CombatText.NewText(target.getRect(), new Color(255, 165, 0), text, true);
                            }

                            // 窃取成功时的金光特效 (这里可能用的是 i，没关系)
                            for (int i = 0; i < 5; i++)
                                Dust.NewDust(target.position, target.width, target.height, DustID.GoldFlame, 0, 0, 0, default, 1.0f);

                            // --- 仪式逻辑 (这里面可能原本包含了一个 int k 的循环) ---
                            if (currentMarauderSequence == 5 && parasiteRitualProgress < PARASITE_RITUAL_TARGET)
                            {
                                parasiteRitualProgress++;
                                if (parasiteRitualProgress >= PARASITE_RITUAL_TARGET)
                                {
                                    Main.NewText("仪式完成：命运的馈赠已集齐... (9/9)", 220, 20, 60);
                                    SoundEngine.PlaySound(SoundID.Roar, Player.position);

                                    // 【这里就是冲突的根源】原来的代码里有 int k
                                    // 现在外层改成了 n，这里就可以安全地使用 k 了
                                    for (int k = 0; k < 20; k++)
                                    {
                                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.PurpleCrystalShard, 0, 0, 0, default, 1.5f);
                                    }
                                }
                                else
                                {
                                    Main.NewText($"从目标处获得了‘供养’... ({parasiteRitualProgress}/{PARASITE_RITUAL_TARGET})", 150, 150, 150);
                                }
                            }
                    for (int i = 0; i < 5; i++)
                                Dust.NewDust(target.position, target.width, target.height, DustID.GoldFlame, 0, 0, 0, default, 1.0f);
                        }

                        // 2. 窃取能力 (模拟：吸取生命/魔力/Buff)
                        // 每次攻击有概率回复生命或魔力，模拟“偷走了对方的力量”
                        if (Main.rand.NextBool(10))
                        {
                            int stealAmount = (int)(damageDone * 0.1f); // 偷取 10% 伤害值的生命/蓝
                            if (stealAmount < 1) stealAmount = 1;
                            if (stealAmount > 20) stealAmount = 20;

                            if (Main.rand.NextBool())
                            {
                                Player.statLife += stealAmount;
                                Player.HealEffect(stealAmount);
                            }
                            else
                            {
                                Player.statMana += stealAmount;
                                Player.ManaEffect(stealAmount);
                            }
                        }
                    }

                }
            }
            if (currentMarauderSequence <= 5)
            {
                // 10% 概率偷走对方“攻击/移动”的想法 -> 造成强力减速或混乱
                if (Main.rand.NextBool(10))
                {
                    target.AddBuff(BuffID.Confused, 180); // 混乱3秒
                    target.AddBuff(BuffID.Slow, 300);     // 减速5秒

                    // 如果是 Boss，可能免疫混乱，但可以稍微减速
                    if (target.boss)
                    {
                        // 可以在这里写特殊的Boss减速逻辑，或者直接忽略
                    }
                    else
                    {
                        // 普通怪直接呆滞一瞬间 (速度归零)
                        target.velocity *= 0.1f;
                    }

                    // 视觉反馈：梦境气泡
                    for (int i = 0; i < 5; i++)
                        Dust.NewDust(target.position, target.width, target.height, DustID.DungeonSpirit, 0, 0, 100, default, 1f);
                }
                if (Main.rand.NextBool(5))
                {
                    // 削弱敌人
                    target.AddBuff(BuffID.Weak, 300);       // 虚弱 (减攻/减速)
                    target.AddBuff(BuffID.BrokenArmor, 300);// 破甲 (减防)

                    // 回复自身 (将记忆转化为精神养分)
                    int heal = 2;
                    Player.statLife += heal;
                    Player.HealEffect(heal);

                    // 特效
                    CombatText.NewText(target.getRect(), new Color(147, 112, 219), "记忆窃取!", true);
                }
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 1. 基础检查：开启了模式 + 序列8 + 必须是鞭子
            if (isTamingActive && currentMoonSequence <= 8 && ProjectileID.Sets.IsAWhip[proj.type])
            {
                // 2. 目标检查：活的 + 敌对 + 非Boss + 非无敌
                if (target.active && !target.friendly && !target.boss && !target.dontTakeDamage)
                {
                    // 3. 血量检查：虚弱(25%血) 或 弱小生物(血上限<50)
                    bool isWeak = (target.life <= target.lifeMax * 0.25f) || (target.lifeMax < 50);

                    if (isWeak)
                    {
                        // 4. 尝试消耗灵性进行契约 (20点)
                        if (TryConsumeSpirituality(20))
                        {
                            SoundEngine.PlaySound(SoundID.Item29, target.position); // 成功音效

                            // === 核心修复：强制满血 + 视觉显示 ===
                            int healAmount = target.lifeMax - target.life; // 计算需要回多少血
                            target.life = target.lifeMax; // 1. 实际加血
                            target.HealEffect(healAmount); // 2. 【关键】显示绿色回血数字！

                            // 清除所有负面状态 (防止驯服后被之前的流血/中毒死)
                            for (int i = 0; i < target.buffType.Length; i++)
                            {
                                if (target.buffType[i] > 0 && Main.debuff[target.buffType[i]])
                                {
                                    target.DelBuff(i);
                                    i--;
                                }
                            }

                            // 添加驯服 Buff
                            target.AddBuff(ModContent.BuffType<Buffs.TamedBuff>(), 18000);

                            // 特效
                            CombatText.NewText(target.getRect(), Color.LightGreen, "驯服成功!", true);
                            for (int k = 0; k < 20; k++)
                            {
                                Dust.NewDust(target.position, target.width, target.height, DustID.HeartCrystal, 0, 0, 0, default, 1.5f);
                            }
                        }
                        else
                        {
                            Main.NewText("灵性不足以完成契约！", 255, 50, 50);
                        }
                    }
                    else
                    {
                        // 提示需虚弱 (加上概率防止刷屏)
                        if (Main.rand.NextBool(10))
                            CombatText.NewText(target.getRect(), Color.Gray, "需虚弱(25%血)!", true);
                    }
                }
            }

            base.OnHitNPCWithProj(proj, target, hit, damageDone);
        }

        private void ApplyHitEffects(NPC target) { if (currentSequence <= 4) target.AddBuff(BuffID.Ichor, 300); if (currentHunterSequence <= 7) target.AddBuff(BuffID.OnFire, 300); if (isCalamityGiant) target.AddBuff(BuffID.Electrified, 300); if (currentHunterSequence <= 1) target.AddBuff(ModContent.BuffType<ConquerorWill>(), 600); if (currentMoonSequence <= 7 && (Player.HeldItem.DamageType == DamageClass.Melee || Player.HeldItem.DamageType == DamageClass.SummonMeleeSpeed || Player.HeldItem.DamageType == DamageClass.Summon)) { target.AddBuff(BuffID.Ichor, 300); if (Main.rand.NextBool(3)) target.AddBuff(BuffID.Poisoned, 300); } }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f; base.ModifyHitNPC(target, ref modifiers); // 保持基类逻辑

            // 处理猎人途径的暴击伤害加成 (原有逻辑)
            if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f;

            // 处理序列1 嫁接模式2 (攻击嫁接)
            if (currentFoolSequence <= 1 && graftingMode == 2)
            {
                bool nerf = ModContent.GetInstance<LotMConfig>().NerfDivineAbilities;

                if (nerf)
                {
                    // 【平衡模式】
                    // 机制：附加目标最大生命值百分比的真实伤害
                    float worldMult = Systems.BalanceSystem.GetWorldTierMultiplier();

                    // 伤害上限计算：开局500 -> 终灾25000
                    int damageCap = (int)(5000 * worldMult);

                    // 基础附加：Boss 1%, 小怪 10%
                    int bonusDamage = target.lifeMax / (target.boss ? 100 : 10);

                    if (bonusDamage > damageCap) bonusDamage = damageCap;

                    modifiers.FinalDamage.Flat += bonusDamage;
                    modifiers.SetCrit(); // 必暴
                }
                else
                {
                    // 【原著模式】秒杀
                    modifiers.SetCrit();
                    modifiers.FinalDamage *= 100;
                    modifiers.ArmorPenetration += 9999;
                }
            }
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f; }
        private void CheckExecution(NPC target) { if (currentHunterSequence <= 5 && !target.boss && target.life < target.lifeMax * 0.2f) target.SimpleStrikeNPC(9999, 0); }
        private void CheckRitualKill(NPC target) { if (target.life <= 0) { if (currentSequence == 5 && demonHunterRitualProgress < DEMON_HUNTER_RITUAL_TARGET) { if (target.type == NPCID.RedDevil) { demonHunterRitualProgress++; } } }
            if (currentMarauderSequence == 4 && mentorRitualProgress < MENTOR_RITUAL_TARGET)
            {
                if (target.HasBuff(BuffID.Confused))
                {
                    mentorRitualProgress++;
                    if (mentorRitualProgress >= MENTOR_RITUAL_TARGET)
                    {
                        Main.NewText("仪式完成：秩序已瓦解，九个被误导的冤魂正在哀嚎... (9/9)", 0, 255, 127); // 碧绿色提示
                        SoundEngine.PlaySound(SoundID.ZombieMoan, Player.position);
                    }
                    else
                    {
                        // 只有当是你造成的混乱致死时才提示 (稍微减少刷屏)
                        Main.NewText($"冤魂 +1 ({mentorRitualProgress}/{MENTOR_RITUAL_TARGET})", 200, 200, 200);
                    }
                }
            }
        }
        public override void PostHurt(Player.HurtInfo info) { if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken) { dawnArmorCurrentHP -= info.Damage; if (dawnArmorCurrentHP <= 0) { dawnArmorCurrentHP = 0; dawnArmorActive = false; dawnArmorBroken = true; dawnArmorCooldownTimer = DAWN_ARMOR_COOLDOWN_MAX; Main.NewText("铠甲已重铸", 100, 255, 100); } }
            if (currentSequence == 6 && guardianRitualProgress < GUARDIAN_RITUAL_TARGET)
            {
                bool npcNearby = false;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if ((npc.townNPC || npc.type == NPCID.TravellingMerchant || npc.type == NPCID.SkeletonMerchant) && npc.Distance(Player.Center) < 800f)
                    {
                        npcNearby = true;
                        break;
                    }
                }

                if (npcNearby)
                {
                    guardianRitualProgress += info.Damage;
                    if (guardianRitualProgress >= GUARDIAN_RITUAL_TARGET)
                    {
                        guardianRitualProgress = GUARDIAN_RITUAL_TARGET;
                        Main.NewText("仪式完成：你已证明了守护的决心！(1000/1000)", 255, 215, 0); // 金色提示
                        SoundEngine.PlaySound(SoundID.Item37, Player.position); // 播放一个提示音效
                    }
                }
            }
        }
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // =================================================
            // 【新增】序列1：概念嫁接 (防御模式)
            // =================================================
            // 如果玩家即将死亡，且是诡秘侍者，尝试消耗灵性嫁接死亡概念
            if (currentFoolSequence <= 1)
            {
                // 需要 5000 灵性来嫁接一次死亡
                if (TryConsumeSpirituality(50000))
                {
                    SoundEngine.PlaySound(SoundID.Item4, Player.position);

                    // 恢复满血
                    Player.statLife = Player.statLifeMax2;
                    Player.HealEffect(Player.statLifeMax2);

                    // 短暂无敌
                    Player.immune = true;
                    Player.immuneTime = 60;

                    Main.NewText("嫁接：死亡的概念被移除了。", 200, 200, 255);

                    // 特效：扭曲的虚空
                    for (int i = 0; i < 50; i++)
                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.Vortex, 0, 0, 0, default, 2f);

                    return false; // 取消死亡
                }
            }
            // =================================================
            // 1. 荣耀战神复活 (巨人途径 序列2)
            // =================================================
            if (currentSequence <= 2 && twilightResurrectionCooldown <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item119, Player.position);
                Player.statLife = Player.statLifeMax2;
                Player.HealEffect(Player.statLifeMax2);
                Player.immune = true;
                Player.immuneTime = 180;
                twilightResurrectionCooldown = TWILIGHT_RESURRECTION_MAX;
                return false; // 取消死亡
            }

            // =================================================
            // 2. 伤害转移 (愚者途径 序列7 魔术师)
            // =================================================
            // 只有当不是半神(序列4以上)时，主要依赖这个。
            // 到了半神主要靠灵之虫，但这个作为最后手段也可以保留。
            if (currentFoolSequence <= 7 && damageTransferCooldown <= 0 && TryConsumeSpirituality(50, true))
            {
                // 如果是半神且有足够的灵之虫，优先触发灵之虫复活，跳过伤害转移
                if (currentFoolSequence <= 4 && spiritWorms >= 10)
                {
                    // 跳过此逻辑，进入下面的灵之虫判断
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item8, Player.position);
                    Player.statLife = 50;
                    Player.immune = true;
                    Player.immuneTime = 120;

                    // 序列6冷却减半
                    damageTransferCooldown = (currentFoolSequence <= 6) ? 5400 : 10800;

                    CombatText.NewText(Player.getRect(), Color.Red, "伤害转移!", true);
                    for (int i = 0; i < 30; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Blood, 0, 0, 0, default, 1.5f);
                    return false; // 取消死亡
                }
            }

            // =================================================
            // 3. 灵之虫复活 (愚者途径 序列4 - 序列2)
            // =================================================
            if (currentFoolSequence <= 4)
            {
                // --- 动态计算消耗 ---
                int wormsNeeded = 10; // 序列4基础消耗
                if (currentFoolSequence <= 3) wormsNeeded = 100;
                if (currentFoolSequence <= 2) wormsNeeded = 300;

                // 检查虫子够不够
                if (spiritWorms >= wormsNeeded)
                {
                    spiritWorms -= wormsNeeded;
                    SoundEngine.PlaySound(SoundID.Item29, Player.position);

                    // --- 动态计算回血 ---
                    int heal = Player.statLifeMax2; // 默认满血 (序列3, 2)
                    if (currentFoolSequence == 4) heal = Player.statLifeMax2 / 2; // 序列4回一半

                    Player.statLife = heal;
                    Player.HealEffect(heal);

                    Player.immune = true;
                    Player.immuneTime = (currentFoolSequence <= 2) ? 300 : 180; // 序列2无敌时间更长

                    // --- 序列2特权：复活奇迹 (清屏伤害) ---
                    if (currentFoolSequence <= 2)
                    {
                        Main.NewText("奇迹降临：死而复生！", 255, 215, 0);
                        // 对全屏敌人造成伤害
                        foreach (NPC npc in Main.ActiveNPCs)
                        {
                            if (!npc.friendly && !npc.dontTakeDamage)
                            {
                                int dmg = 5000;
                                Player.ApplyDamageToNPC(npc, dmg, 0, 0, false);
                                // 生成特效
                                for (int i = 0; i < 5; i++) Dust.NewDust(npc.position, npc.width, npc.height, DustID.Enchanted_Gold, 0, 0, 0, default, 1.5f);
                            }
                        }
                    }
                    else
                    {
                        // 普通复活提示
                        Main.NewText($"灵之虫替你承受了死亡... (剩余: {spiritWorms})", 200, 200, 200);
                    }

                    // 视觉特效：虫子重组
                    for (int i = 0; i < 40; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Worm, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 0, default, 1.5f);

                    return false; // 取消死亡
                }
            }
            // =================================================
            // 错误途径序列4：半虫化 (不死之身)
            // =================================================
            if (currentMarauderSequence <= 4 && wormificationCooldown <= 0)
            {
                // 消耗大量灵性重组身体
                if (TryConsumeSpirituality(200, true)) // 紧急消耗，允许透支一点
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath13, Player.position); // 虫子恶心的声音

                    // 恢复部分生命
                    int heal = Player.statLifeMax2 / 2; // 恢复一半血
                    Player.statLife = heal;
                    Player.HealEffect(heal);

                    // 给予无敌时间
                    Player.immune = true;
                    Player.immuneTime = 180; // 3秒无敌

                    // 视觉特效：身体分散成虫子又聚合
                    for (int i = 0; i < 30; i++)
                    {
                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.Worm, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 0, default, 1.2f);
                    }

                    Main.NewText("身体瞬间分散成无数虫豸，躲过了致命一击！", 175, 238, 238);

                    wormificationCooldown = WORMIFICATION_COOLDOWN_MAX; // 进入长冷却
                    return false; // 拒绝死亡
                }
            }
            // =================================================
            // 错误途径序列2：命运木马 (分身替死)
            // =================================================
            if (currentMarauderSequence <= 2 && !isTrojanResurrection)
            {
                // 消耗大量灵性
                if (TryConsumeSpirituality(300, true))
                {
                    isTrojanResurrection = true; // 标记已触发，防止无限触发（需在重置效果或冷却中重置）
                    twilightResurrectionCooldown = 3600; // 复用通用的复活冷却变量，或者新建一个

                    Player.statLife = Player.statLifeMax2; // 满血复活
                    Player.HealEffect(Player.statLifeMax2);
                    Player.immune = true;
                    Player.immuneTime = 120;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, Player.position); // 时空扭曲声
                    Main.NewText("命运的浪花已被预见，死亡的是你的‘过去’...", 200, 200, 255);

                    // 特效：生成一个假身破碎
                    for (int i = 0; i < 30; i++)
                    {
                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.SilverCoin, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 0, default, 1.5f);
                    }

                    return false; // 拒绝死亡
                }
            }

            return true; // 允许死亡 (如果上面都没触发)
        }

        // 【美神被动】
        public override bool FreeDodge(Player.HurtInfo info)
        {
            // 【新增】序列1：诡秘侍者 (灵体状态下免疫大部分伤害)
            if (isSpiritForm)
            {
                // 产生一点幽灵特效
                if (Main.rand.NextBool(3))
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.SpectreStaff, 0, 0, 150, default, 1.5f);
                return true; // 直接免疫此次伤害
            }
            // 1. 序列1 美神 (月亮途径) - 25% 几率
            if (currentMoonSequence <= 1 && Main.rand.NextFloat() < 0.25f)
            {
                Player.SetImmuneTimeForAllTypes(60);
                CombatText.NewText(Player.getRect(), Color.Pink, "Miss", true);
                return true;
            }

            // 2. 序列7 魔术师 & 序列6 无面人 (愚者途径)
            // 序列6几率提升至 40%，序列7为 25%
            float paperChance = (currentFoolSequence <= 6) ? 0.4f : 0.25f;

            // ==========================================================
            // [重做] 序列7 魔术师 & 序列6 无面人 (纸人替身)
            // 机制：不再是概率触发，而是消耗背包里的“纸人替身”道具来100%回避
            // ==========================================================
            if (currentFoolSequence <= 7)
            {
                bool hasPaper = false;
                // 定义纸人道具的类型 ID
                int paperItemType = ModContent.ItemType<Content.Items.Consumables.PaperFigurine>();

                // 检查背包里是否有纸人 (全背包搜索，包含虚空袋)
                if (Player.CountItem(paperItemType) > 0)
                {
                    Player.ConsumeItem(paperItemType);
                    Player.SetImmuneTimeForAllTypes(120); // 2秒无敌

                    // 特效
                    SoundEngine.PlaySound(SoundID.Item65, Player.position);
                    for (int i = 0; i < 20; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Confetti, 0, 0, 0, default, 1.5f);

                    // 随机位移
                    for (int i = 0; i < 10; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke, 0, 0, 100, Color.Gray, 2f);
                    Vector2 randomPos = Player.position + Main.rand.NextVector2Circular(200, 200);
                    if (!Collision.SolidCollision(randomPos, Player.width, Player.height)) Player.position = randomPos;

                    CombatText.NewText(Player.getRect(), Color.White, "纸人替身!", true);
                    return true; // 闪避成功
                }
            }

            // ==========================================================
            // 2. 反占卜 (序列6 无面人) - 纯被动流
            // 机制：没有纸人时，有概率自动触发（概率较低），触发时清除 Debuff
            // ==========================================================
            // 设定概率为 15% (0.15f)
            if (currentFoolSequence <= 6 && Main.rand.NextFloat() < 0.15f)
            {
                Player.SetImmuneTimeForAllTypes(60); // 1秒无敌

                // 反占卜核心：清除负面状态
                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    if (Player.buffType[i] > 0 && Main.debuff[Player.buffType[i]])
                    {
                        Player.DelBuff(i);
                        i--;
                    }
                }

                // 视觉特效：神秘的灰色符文/烟雾
                for (int i = 0; i < 15; i++)
                {
                    Dust d = Dust.NewDustPerfect(Player.Center, DustID.DungeonSpirit, Main.rand.NextVector2Circular(3f, 3f), 150, default, 1.2f);
                    d.noGravity = true;
                }

                CombatText.NewText(Player.getRect(), Color.Gray, "反占卜!", true);
                return true; // 闪避成功
            }

            // 3. 直觉闪避 (序列8 小丑) - 低保被动
            // 如果上面两个都没触发，最后判定这个 10%
            else if (currentFoolSequence <= 8 && Main.rand.NextFloat() < 0.1f)
            {
                Player.SetImmuneTimeForAllTypes(60);
                for (int i = 0; i < 10; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Confetti, 0, 0, 0, default, 1.2f);
                CombatText.NewText(Player.getRect(), Color.Orange, "直觉闪避!", true);
                return true;
            }

            // 4. 特殊变身形态 (默认拥有闪避/虚化效果)
            if (isCalamityGiant || isMercuryForm || isMoonlightized || isFacelessActive)
                return true;

            return base.FreeDodge(info);
        }

        // 7. 按键
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            float moonMult = GetSequenceMultiplier(currentMoonSequence);
            float hunterMult = GetSequenceMultiplier(currentHunterSequence);
            float giantMult = GetSequenceMultiplier(currentSequence);
            float foolMult = GetSequenceMultiplier(currentFoolSequence);

            if (LotMKeybinds.Moon_Wings.JustPressed && currentMoonSequence <= 7) { isVampireWings = !isVampireWings; if (isVampireWings) { isBatSwarm = false; SoundEngine.PlaySound(SoundID.Item103, Player.position); Main.NewText("黑暗之翼：展开", 180, 0, 0); } else Main.NewText("黑暗之翼：收起", 200, 200, 200); }
            if (LotMKeybinds.Moon_BatSwarm.JustPressed && currentMoonSequence <= 4) { isBatSwarm = !isBatSwarm; if (isBatSwarm) { isVampireWings = false; isMoonlightized = false; SoundEngine.PlaySound(SoundID.Item103, Player.position); Main.NewText("化身为蝙蝠群...", 100, 0, 200); } else Main.NewText("解除化身", 200, 200, 200); }

            // J键
            if (LotMKeybinds.Moon_PaperFigurine.JustPressed)
            {
                if (currentMoonSequence <= 2)
                {
                    if (paperFigurineCooldown <= 0 && TryConsumeSpirituality(5000)) { SoundEngine.PlaySound(SoundID.Item119, Player.position); Main.NewText("万物滋长：生命归顺！", 100, 255, 100); foreach (NPC npc in Main.ActiveNPCs) { if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 1500f) { if (npc.boss) { int dmg = (int)(5000 * moonMult); npc.SimpleStrikeNPC(dmg, 0, true, 0, DamageClass.Magic); CombatText.NewText(npc.getRect(), Color.Red, "生命剥夺!", true); } else { npc.AddBuff(ModContent.BuffType<TamedBuff>(), 36000); CombatText.NewText(npc.getRect(), Color.LightGreen, "新生!", true); for (int i = 0; i < 20; i++) Dust.NewDust(npc.position, npc.width, npc.height, DustID.Terra, 0, 0, 0, default, 1.5f); } } } for (int i = 0; i < Main.maxPlayers; i++) { Player p = Main.player[i]; if (p.active && !p.dead && p.whoAmI != Player.whoAmI && p.hostile && Player.hostile && p.Distance(Player.Center) < 1000f) { p.AddBuff(BuffID.Confused, 300); p.AddBuff(BuffID.Silenced, 300); } } paperFigurineCooldown = 18000; } else if (paperFigurineCooldown > 0) Main.NewText($"转化冷却: {paperFigurineCooldown / 60}s", 150, 150, 150);
                }
                else if (currentMoonSequence <= 4) { if (paperFigurineCooldown <= 0 && TryConsumeSpirituality(100)) { Player.velocity = -Player.velocity * 2f; Player.immune = true; Player.immuneTime = 60; SoundEngine.PlaySound(SoundID.Item6, Player.position); paperFigurineCooldown = 900; } }
            }

            if (LotMKeybinds.Moon_Gaze.JustPressed && currentMoonSequence <= 4) { if (darknessGazeCooldown <= 0 && TryConsumeSpirituality(200)) { bool hit = false; for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && npc.getRect().Contains(Main.MouseWorld.ToPoint())) { int dmg = (int)(5000 * moonMult); if (currentMoonSequence <= 1 && !npc.boss) dmg = 999999; npc.SimpleStrikeNPC(dmg, 0, true, 0, DamageClass.Magic); npc.AddBuff(BuffID.Darkness, 600); npc.AddBuff(BuffID.ShadowFlame, 600); hit = true; } } if (hit) { SoundEngine.PlaySound(SoundID.Item104, Main.MouseWorld); darknessGazeCooldown = 1200; } } else if (darknessGazeCooldown > 0) Main.NewText($"凝视冷却: {darknessGazeCooldown / 60}s", 150, 150, 150); }
            if (LotMKeybinds.Moon_Shackles.JustPressed && currentMoonSequence <= 7) { if (currentMoonSequence <= 5 && isFullMoonActive) { if (fireTeleportCooldown <= 0 && TryConsumeSpirituality(50)) { Vector2 targetPos = Main.MouseWorld; if (Player.Distance(targetPos) < 800f && Collision.CanHit(Player.position, Player.width, Player.height, targetPos, Player.width, Player.height)) { SoundEngine.PlaySound(SoundID.Item8, Player.position); Player.Teleport(targetPos, 1); fireTeleportCooldown = 30; } } } else if (abyssShackleCooldown <= 0 && TryConsumeSpirituality(30)) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 12f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<AbyssShackleProjectile>(), 20, 0f, Player.whoAmI); SoundEngine.PlaySound(SoundID.Item8, Player.position); abyssShackleCooldown = 180; } }
            if (LotMKeybinds.Moon_Grenade.JustPressed && currentMoonSequence <= 6)
            {
                if (currentMoonSequence <= 2) { if (purifyCooldown <= 0 && TryConsumeSpirituality(1000)) { int radius = 60; int centerX = (int)(Player.Center.X / 16f); int centerY = (int)(Player.Center.Y / 16f); for (int x = centerX - radius; x <= centerX + radius; x++) for (int y = centerY - radius; y <= centerY + radius; y++) WorldGen.Convert(x, y, 0, 0, false, false); SoundEngine.PlaySound(SoundID.Item29, Player.position); Main.NewText("大地重获新生。", 100, 255, 100); purifyCooldown = 600; } }
                else if (TryConsumeSpirituality(20)) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 14f; int dmg = (int)(60 * moonMult); Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<AlchemicalGrenade>(), dmg, 5f, Player.whoAmI); SoundEngine.PlaySound(SoundID.Item1, Player.position); }
            }
            if (LotMKeybinds.Moon_Elixir.JustPressed && currentMoonSequence <= 6)
            {
                if (currentMoonSequence <= 2) { if (elixirCooldown <= 0 && TryConsumeSpirituality(2000)) { int heal = Player.statLifeMax2 - Player.statLife; Player.statLife = Player.statLifeMax2; Player.HealEffect(heal); for (int i = 0; i < Player.MaxBuffs; i++) if (Main.debuff[Player.buffType[i]]) Player.DelBuff(i); SoundEngine.PlaySound(SoundID.Item4, Player.position); Main.NewText("生命奇迹：重获新生！", 0, 255, 255); elixirCooldown = 3600; } }
                else if (elixirCooldown <= 0 && TryConsumeSpirituality(200)) { int heal = (int)(1000 * moonMult); Player.statLife += heal; if (Player.statLife > Player.statLifeMax2) Player.statLife = Player.statLifeMax2; Player.HealEffect(heal); SoundEngine.PlaySound(SoundID.Item3, Player.position); elixirCooldown = 3600; Main.NewText("服用生命灵液！", 50, 255, 50); }
            }
            if (LotMKeybinds.Moon_Moonlight.JustPressed && currentMoonSequence <= 5) { isMoonlightized = !isMoonlightized; if (isMoonlightized) { isBatSwarm = false; SoundEngine.PlaySound(SoundID.Item8, Player.position); Main.NewText("身体化为绯红月光...", 255, 100, 100); } else Main.NewText("解除月光化", 200, 200, 200); }
            if (LotMKeybinds.Moon_FullMoon.JustPressed && currentMoonSequence <= 5)
            {
                if (currentMoonSequence <= 2) { isCreationDomain = !isCreationDomain; if (isCreationDomain) { isFullMoonActive = false; SoundEngine.PlaySound(SoundID.Item29, Player.position); Main.NewText("创生领域：生命主宰", 0, 255, 0); } else Main.NewText("领域收起", 200, 200, 200); }
                else { isFullMoonActive = !isFullMoonActive; if (isFullMoonActive) { SoundEngine.PlaySound(SoundID.Item29, Player.position); Main.NewText("满月降临", 255, 50, 50); } else Main.NewText("满月隐去", 200, 200, 200); }
            }
            if (LotMKeybinds.Moon_SummonGate.JustPressed && currentMoonSequence <= 3) { if (summonGateCooldown <= 0 && TryConsumeSpirituality(300)) { Vector2 spawnPos = Player.Center + new Vector2(Player.direction * 50, -50); Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<SummoningDoorProjectile>(), 0, 0f, Player.whoAmI); summonGateCooldown = 3600; Main.NewText("召唤之门已打开！", 150, 100, 255); } else if (summonGateCooldown > 0) Main.NewText($"召唤冷却: {summonGateCooldown / 60}s", 150, 150, 150); }
            if (LotMKeybinds.Moon_Tame.JustPressed && currentMoonSequence <= 8)
            {
                isTamingActive = !isTamingActive;
                if (isTamingActive)
                {
                    SoundEngine.PlaySound(SoundID.Item4, Player.position);
                    Main.NewText("驯兽模式：开启 (持续消耗灵性，使用鞭子驯服)", 100, 255, 100);
                }
                else
                {
                    Main.NewText("驯兽模式：关闭", 200, 200, 200);
                }
            }
            // 状态自动解除 (如果死掉或序列不对)
            if (currentMoonSequence > 8) isTamingActive = false;

            if (LotMKeybinds.RP_Transformation.JustPressed) { if (currentHunterSequence <= 2) { isCalamityGiant = !isCalamityGiant; if (isCalamityGiant) Main.NewText("灾祸巨人形态", 0, 255, 255); } else if (currentHunterSequence <= 4) { isFireForm = !isFireForm; if (isFireForm) Main.NewText("火焰形态", 255, 100, 0); } }
            if (LotMKeybinds.RP_Flash.JustPressed && currentHunterSequence <= 6) { if (fireTeleportCooldown <= 0 && TryConsumeSpirituality(100)) { Vector2 targetPos = Main.MouseWorld; if (Player.Distance(targetPos) < 600f && Collision.CanHit(Player.position, Player.width, Player.height, targetPos, Player.width, Player.height)) { SoundEngine.PlaySound(SoundID.Item14, Player.position); for (int i = 0; i < 20; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, 0, 0, 0, default, 2f); Player.Teleport(targetPos, 1); fireTeleportCooldown = 60; } } }
            if (LotMKeybinds.RP_Bomb.JustPressed && currentHunterSequence <= 7) { if (TryConsumeSpirituality(50)) { int dmg = (int)(100 * hunterMult); Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 10f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<PyromaniacBomb>(), dmg, 5f, Player.whoAmI); } }
            if (LotMKeybinds.RP_Cloak.JustPressed && currentHunterSequence <= 7) { isFlameCloakActive = !isFlameCloakActive; if (isFlameCloakActive) Main.NewText("火焰披风开启", 255, 100, 0); }
            if (LotMKeybinds.RP_Slash.JustPressed && currentHunterSequence <= 5) { if (!isFireForm && TryConsumeSpirituality(100)) { int dmg = (int)(200 * hunterMult); Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<ReaperSlashProjectile>(), dmg, 10f, Player.whoAmI); } }
            if (LotMKeybinds.RP_Enchant.JustPressed && currentHunterSequence <= 6) { isFireEnchanted = !isFireEnchanted; Main.NewText("武器附魔切换", 255, 100, 0); }
            if (currentHunterSequence <= 7) { if (LotMKeybinds.RP_Skill.Current) { if (TryConsumeSpirituality(0.5f)) { isChargingFireball = true; fireballChargeTimer++; } } if (LotMKeybinds.RP_Skill.JustReleased && isChargingFireball) { int dmg = (int)(100 * hunterMult); Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 12f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<PyromaniacFireball>(), dmg, 4f, Player.whoAmI); isChargingFireball = false; fireballChargeTimer = 0; } }
            if (LotMKeybinds.RP_Army.JustPressed && currentHunterSequence <= 4) { if (!isFireForm) { isArmyOfOne = !isArmyOfOne; Main.NewText("集众切换", 255, 100, 0); } }
            if (LotMKeybinds.RP_Weather.JustPressed && currentHunterSequence <= 2) { if (TryConsumeSpirituality(200, true)) { int dmg = (int)(500 * hunterMult); Projectile.NewProjectile(Player.GetSource_FromThis(), Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<WeatherStrikeLightning>(), dmg, 5f, Player.whoAmI); } }
            if (LotMKeybinds.RP_Glacier.JustPressed && currentHunterSequence <= 2) { if (glacierCooldown <= 0 && TryConsumeSpirituality(1000)) { Main.NewText("冰河世纪！", 0, 255, 255); glacierCooldown = 1800; foreach (NPC npc in Main.ActiveNPCs) { if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 2000f) { npc.AddBuff(BuffID.Frozen, 300); npc.AddBuff(BuffID.Frostburn2, 600); npc.AddBuff(BuffID.Slow, 600); if (!npc.boss) npc.velocity = Vector2.Zero; } } } }
            if (LotMKeybinds.Giant_Mercury.JustPressed && currentSequence <= 3) { if (!isGuardianStance) { if (!isMercuryForm) { if (TryConsumeSpirituality(500)) isMercuryForm = true; } else isMercuryForm = false; } }
            if (LotMKeybinds.Giant_Armor.JustPressed && currentSequence <= 6) { if (!isMercuryForm && !dawnArmorBroken) dawnArmorActive = !dawnArmorActive; }
            if (currentSequence <= 5 && LotMKeybinds.Giant_Guardian.Current && !isMercuryForm) { if (TryConsumeSpirituality(10.0f)) isGuardianStance = true; else isGuardianStance = false; } else isGuardianStance = false;

            bool vKeyJustPressed = LotMKeybinds.Fool_SpiritForm.JustPressed || LotMKeybinds.Fool_Miracle.JustPressed || LotMKeybinds.Fool_Faceless.JustPressed;
            bool vKeyCurrent = LotMKeybinds.Fool_SpiritForm.Current || LotMKeybinds.Fool_Miracle.Current || LotMKeybinds.Fool_Faceless.Current;
            bool shiftPressed = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift);

            // 【情况 A】序列 1 诡秘侍者 (Shift+V 愿望, V 灵体化)
            if (currentFoolSequence <= 1)
            {
                // 1. 愿望逻辑 (Shift + V)
                if ((shiftPressed && vKeyCurrent) || (wishCastTimer > 0 && vKeyCurrent))
                {
                    if (vKeyJustPressed && shiftPressed)
                    {
                        selectedWish++; if (selectedWish > 3) selectedWish = 0;
                        string n = selectedWish == 0 ? "生命复苏" : selectedWish == 1 ? "毁灭天灾" : selectedWish == 2 ? "空间传送" : "昼夜更替";
                        Main.NewText($"[奇迹愿望]: {n} (保持按住以实现)", 255, 215, 0);
                        wishCastTimer = 0;
                    }
                    wishCastTimer++;
                    if (wishCastTimer % 10 == 0) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Enchanted_Gold, 0, -2);

                    if (wishCastTimer >= 60)
                    {
                        CastMiracleWish();
                        wishCastTimer = 0;
                    }
                }
                // 2. 灵体化逻辑 (单按 V)
                else if (vKeyJustPressed && !shiftPressed)
                {
                    isSpiritForm = !isSpiritForm;
                    if (isSpiritForm)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, Player.position);
                        Main.NewText("灵体化：开启 (物理免疫/穿墙/高耗蓝)", 200, 200, 255);
                    }
                    else Main.NewText("回归血肉之躯", 150, 150, 150);

                    wishCastTimer = 0;
                }
                else if (!vKeyCurrent) wishCastTimer = 0;
            }
            // 【情况 B】序列 2 奇迹师 (直接 V 愿望)
            else if (currentFoolSequence <= 2)
            {
                if (vKeyJustPressed)
                {
                    selectedWish++; if (selectedWish > 3) selectedWish = 0;
                    string n = selectedWish == 0 ? "生命复苏" : selectedWish == 1 ? "毁灭天灾" : selectedWish == 2 ? "空间传送" : "昼夜更替";
                    Main.NewText($"[奇迹愿望]: {n} (长按V实现)", 255, 215, 0);
                    wishCastTimer = 0;
                }
                if (vKeyCurrent)
                {
                    wishCastTimer++;
                    if (wishCastTimer >= 60) { CastMiracleWish(); wishCastTimer = 0; }
                }
                else wishCastTimer = 0;
            }
            // 【情况 C】序列 6 无面人 (V 伪装)
            else if (currentFoolSequence <= 6)
            {
                if (vKeyJustPressed)
                {
                    isFacelessActive = !isFacelessActive;
                    if (isFacelessActive) { SoundEngine.PlaySound(SoundID.Item8, Player.position); Main.NewText("无面伪装：开启", 200, 200, 200); }
                    else Main.NewText("无面伪装：关闭", 150, 150, 150);
                }
            }

            // -----------------------------------------------------------
            // 2. 辅助技能：嫁接 / 干扰 / 历史 (G键 & Y键)
            // -----------------------------------------------------------
            bool gKeyJustPressed = LotMKeybinds.Fool_Grafting.JustPressed || LotMKeybinds.Fool_Distort.JustPressed;

            // --- G键逻辑 ---
            if (currentFoolSequence <= 1) // 序列1
            {
                // Shift + G : 命运干扰光环
                if (shiftPressed && gKeyJustPressed)
                {
                    fateDisturbanceActive = !fateDisturbanceActive;
                    Main.NewText(fateDisturbanceActive ? "命运干扰: 开启" : "命运干扰: 关闭", 150, 100, 255);
                }
                // 单按 G : 嫁接切换
                else if (gKeyJustPressed && !shiftPressed)
                {
                    graftingMode++; if (graftingMode > 2) graftingMode = 0;
                    string m = graftingMode == 0 ? "关闭" : (graftingMode == 1 ? "空间嫁接(反弹)" : "概念嫁接(必杀)");
                    Main.NewText($"嫁接模式: {m}", 100, 100, 255);
                    SoundEngine.PlaySound(SoundID.Item4, Player.position);
                }
            }
            else if (currentFoolSequence <= 2) // 序列2
            {
                if (gKeyJustPressed)
                {
                    fateDisturbanceActive = !fateDisturbanceActive;
                    Main.NewText(fateDisturbanceActive ? "命运干扰: 开启" : "命运干扰: 关闭", 150, 100, 255);
                }
            }
            else if (currentFoolSequence <= 6) // 序列6
            {
                if (gKeyJustPressed && distortCooldown <= 0 && TryConsumeSpirituality(30))
                {
                    SoundEngine.PlaySound(SoundID.Item18, Player.position);
                    distortCooldown = 600;
                    foreach (NPC npc in Main.ActiveNPCs) if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 600f) { npc.AddBuff(BuffID.Confused, 300); npc.AddBuff(BuffID.Midas, 600); npc.damage = (int)(npc.damage * 0.8f); }
                    Main.NewText("命运已被短暂干扰。", 255, 215, 0);
                }
            }

            // --- Y键逻辑 (历史投影) ---
            if (currentFoolSequence <= 3)
            {
                // Shift + Y : 历史场景 (序列2特权)
                if (currentFoolSequence <= 2 && shiftPressed && LotMKeybinds.Fool_History.JustPressed)
                {
                    bool exists = false;
                    for (int i = 0; i < Main.maxProjectiles; i++) { if (Main.projectile[i].active && Main.projectile[i].owner == Player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<Projectiles.HistoricalSceneProjectile>()) { Main.projectile[i].Kill(); exists = true; Main.NewText("场景消散。", 150, 150, 150); break; } }
                    if (!exists && TryConsumeSpirituality(1000))
                    {
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.HistoricalSceneProjectile>(), 0, 0, Player.whoAmI);
                        SoundEngine.PlaySound(SoundID.Item4, Player.position);
                        Main.NewText("历史场景降临...", 200, 200, 255);
                    }
                }
                // 单按 Y : Boss 投影
                else if (LotMKeybinds.Fool_History.JustPressed && !shiftPressed)
                {
                    int currentProjections = 0;
                    for (int i = 0; i < Main.maxProjectiles; i++) { if (Main.projectile[i].active && Main.projectile[i].owner == Player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<Projectiles.HistoricalBossProjectile>()) currentProjections++; }

                    int maxProj = 1;
                    if (currentProjections < maxProj)
                    {
                        if (TryConsumeSpirituality(500))
                        {
                            System.Collections.Generic.List<int> bossIDs = new System.Collections.Generic.List<int>();
                            System.Collections.Generic.List<int> bossPowers = new System.Collections.Generic.List<int>();
                            void Add(int id, int p) { bossIDs.Add(id); bossPowers.Add(p); }

                            if (NPC.downedSlimeKing) Add(NPCID.KingSlime, 40);
                            if (NPC.downedBoss1) Add(NPCID.EyeofCthulhu, 50);
                            if (NPC.downedBoss2) Add(NPCID.EaterofWorldsHead, 60);
                            if (NPC.downedBoss3) Add(NPCID.SkeletronHead, 80);
                            if (Main.hardMode) Add(NPCID.WallofFlesh, 100);
                            if (NPC.downedMechBossAny) Add(NPCID.TheDestroyer, 140);
                            if (NPC.downedPlantBoss) Add(NPCID.Plantera, 180);
                            if (NPC.downedMoonlord) Add(NPCID.MoonLordHead, 350);
                            if (bossIDs.Count == 0) Add(NPCID.BlueSlime, 20);

                            int idx = Main.rand.Next(bossIDs.Count);
                            int dmg = (int)(bossPowers[idx] * 5 * foolMult);
                            if (currentFoolSequence <= 2) dmg *= 2;

                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.HistoricalBossProjectile>(), dmg, 4f, Player.whoAmI, bossIDs[idx]);
                            Main.NewText("历史投影降临!", 200, 200, 200);
                            SoundEngine.PlaySound(SoundID.Item113, Player.position);
                        }
                    }
                    else Main.NewText("投影数量已达上限。", 150, 150, 150);
                }
            }

            // U: 昨日重现 (序列3)
            if (LotMKeybinds.Fool_Borrow.JustPressed && currentFoolSequence <= 3)
            {
                if (!Player.HasBuff(ModContent.BuffType<Buffs.YesterdayBuff>()))
                {
                    if (borrowUsesDaily < 10 && TryConsumeSpirituality(100))
                    {
                        borrowUsesDaily++; Player.AddBuff(ModContent.BuffType<Buffs.YesterdayBuff>(), 18000);
                        Player.statLife = Player.statLifeMax2; Player.HealEffect(Player.statLifeMax2);
                        Main.NewText($"昨日重现! (剩余{10 - borrowUsesDaily}次)", 0, 255, 255);
                    }
                    else Main.NewText("次数耗尽。", 150, 150, 150);
                }
                else Main.NewText("力量正在涌动...", 150, 150, 150);
            }

            // T: 秘偶互换 (序列4)
            if (LotMKeybinds.Fool_Swap.JustPressed && currentFoolSequence <= 4)
            {
                if (swapCooldown <= 0 && TryConsumeSpirituality(50))
                {
                    Projectile closest = null; float minDst = 9999f;

                    // 遍历所有弹幕
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile p = Main.projectile[i];
                        // 寻找自己的 秘偶弹幕 或 历史投影
                        if (p.active && p.owner == Player.whoAmI &&
                           (p.type == ModContent.ProjectileType<MarionetteMinion>() ||
                            p.type == ModContent.ProjectileType<Projectiles.HistoricalBossProjectile>()))
                        {
                            float d = Vector2.Distance(Player.Center, p.Center);
                            if (d < minDst) { minDst = d; closest = p; }
                        }
                    }

                    if (closest != null)
                    {
                        Vector2 tmp = Player.Center;
                        Player.Teleport(closest.Center, 1);
                        closest.Center = tmp;

                        swapCooldown = 60;
                        SoundEngine.PlaySound(SoundID.Item6, Player.position);
                        Main.NewText("位置互换", 200, 200, 255);
                    }
                    else Main.NewText("无秘偶可换。", 150, 150, 150);
                }
            }

            // R: 控灵 (序列4)
            if (LotMKeybinds.Fool_Control.JustPressed && currentFoolSequence <= 4)
            {
                if (spiritControlCooldown <= 0 && TryConsumeSpirituality(100))
                {
                    spiritControlCooldown = 1200; SoundEngine.PlaySound(SoundID.Item103, Player.position);
                    foreach (NPC n in Main.ActiveNPCs) if (!n.friendly && !n.dontTakeDamage && n.Distance(Player.Center) < 1000f) { n.AddBuff(BuffID.Stoned, 180); n.AddBuff(BuffID.Confused, 300); }
                    Main.NewText("灵体震慑!", 148, 0, 211);
                }
                else Main.NewText("冷却中...", 150, 150, 150);
            }

            // Z: 灵体之线 (序列5)
            if (LotMKeybinds.Fool_Threads.Current && currentFoolSequence <= 5)
            {
                if (spiritThreadTargetIndex == -1)
                {
                    float minD = 400f; int idx = -1;
                    foreach (NPC n in Main.ActiveNPCs)
                    {
                        // 修正：townNPC判定需在序列2以上才生效
                        bool valid = n.CanBeChasedBy() || (n.townNPC && currentFoolSequence <= 2);
                        if (valid && n.Distance(Main.MouseWorld) < minD) { minD = n.Distance(Main.MouseWorld); idx = n.whoAmI; }
                    }
                    if (idx != -1) { spiritThreadTargetIndex = idx; Main.NewText("抓住灵体之线...", 180, 80, 255); }
                }
                if (spiritThreadTargetIndex != -1 && TryConsumeSpirituality(1.0f))
                {
                    NPC t = Main.npc[spiritThreadTargetIndex];
                    if (!t.active || t.Distance(Player.Center) > 1000f) { spiritThreadTargetIndex = -1; spiritThreadTimer = 0; }
                    else
                    {
                        t.AddBuff(ModContent.BuffType<Buffs.SpiritControlDebuff>(), 2);
                        if (Main.rand.NextBool(2)) Dust.NewDust(Vector2.Lerp(Player.Center, t.Center, Main.rand.NextFloat()), 0, 0, DustID.PurpleCrystalShard);
                        spiritThreadTimer += (currentFoolSequence <= 4 ? 5 : 1);
                        if (spiritThreadTimer >= CONTROL_TIME_REQUIRED)
                        {
                            if (t.townNPC)
                            {
                                // 【核心修复】检查是否已经有Buff，如果没有，则增加进度
                                // 这样防止对着同一个NPC重复刷进度
                                if (!t.HasBuff(ModContent.BuffType<Buffs.MarionetteTownNPCBuff>()))
                                {
                                    if (currentFoolSequence == 2 && attendantRitualProgress < ATTENDANT_RITUAL_TARGET)
                                    {
                                        attendantRitualProgress++;
                                        if (attendantRitualProgress >= ATTENDANT_RITUAL_TARGET)
                                        {
                                            attendantRitualComplete = true;

                                            Main.NewText("仪式完成：诡秘的侍者正在注视着你... (10/10)", 220, 20, 60);
                                            SoundEngine.PlaySound(SoundID.Roar, Player.position);
                                        }
                                        else
                                        {
                                            Main.NewText($"仪式进度: {attendantRitualProgress}/{ATTENDANT_RITUAL_TARGET}", 150, 100, 255);
                                        }
                                    }
                                }

                                t.AddBuff(ModContent.BuffType<Buffs.MarionetteTownNPCBuff>(), 36000);
                                Main.NewText($"{t.FullName} 已转化为秘偶!", 148, 0, 211);
                                t.life = t.lifeMax;
                            }
                            else
                            {
                                // === 【还原】直接转化逻辑 ===
                                int dmg = 999999;
                                if (t.boss || t.realLife != -1 || Terraria.ID.NPCID.Sets.ShouldBeCountedAsBoss[t.type])
                                {
                                    dmg = 2000; // 对 Boss 或其肢体只造成 2000 伤害
                                }

                                Player.ApplyDamageToNPC(t, dmg, 0, 0, false);

                                if (!t.boss)
                                {
                                    // 检查数量限制
                                    int currentCount = Player.ownedProjectileCounts[ModContent.ProjectileType<MarionetteMinion>()];
                                    int maxCount = (currentFoolSequence <= 4) ? 10 : 3;

                                    if (currentCount < maxCount)
                                    {
                                        // 直接在怪物位置生成秘偶弹幕
                                        Projectile.NewProjectile(Player.GetSource_FromThis(), t.Center, Vector2.Zero, ModContent.ProjectileType<MarionetteMinion>(), (int)(100 * foolMult), 2f, Player.whoAmI);
                                        Main.NewText("转化成功!", 200, 100, 255);
                                    }
                                    else
                                    {
                                        Main.NewText("秘偶数量已达上限，直接处决。", 150, 150, 150);
                                    }
                                }
                                else
                                {
                                    Main.NewText("目标位格过高，只能造成伤害!", 255, 100, 100);
                                }
                            }
                            spiritThreadTargetIndex = -1; spiritThreadTimer = 0;
                        }
                    }
                }
            }
            else { spiritThreadTargetIndex = -1; spiritThreadTimer = 0; }

            // C: 灵视 (序列9)
            if (LotMKeybinds.Fool_SpiritVision.JustPressed && currentFoolSequence <= 9)
            {
                isSpiritVisionActive = !isSpiritVisionActive;
                Main.NewText(isSpiritVisionActive ? "灵视开启" : "灵视关闭", 200, 200, 255);
            }

            // F: 火焰跳跃 (序列7)
            if (LotMKeybinds.Fool_FlameJump.JustPressed && currentFoolSequence <= 7)
            {
                if (flameJumpCooldown <= 0 && TryConsumeSpirituality(100))
                {
                    Player.Teleport(Main.MouseWorld, 1);
                    SoundEngine.PlaySound(SoundID.Item45, Player.position);
                    flameJumpCooldown = 60;
                }
                else if (flameJumpCooldown <= 0)
                {
                    Main.NewText("灵性不足 (需100点)", 255, 50, 50);
                }
            }

            // J: 占卜 (序列9)
            if (LotMKeybinds.Fool_Divination.JustPressed && currentFoolSequence <= 9)
            {
                if (divinationCooldown <= 0 && TryConsumeSpirituality(10))
                {
                    divinationCooldown = 3600; // 冷却60秒

                    int resultIndex = Main.rand.Next(6);
                    int buffDuration = 3600; // 持续60秒

                    string resultText = "";
                    Color textColor = Color.White;

                    switch (resultIndex)
                    {
                        case 0: // 厄运
                            resultText = "厄运：危险正在靠近...";
                            // 【修复】BuffID.DangerSense -> BuffID.Dangersense (注意小写s)
                            Player.AddBuff(BuffID.Dangersense, buffDuration);
                            Player.AddBuff(BuffID.Battle, buffDuration);
                            textColor = Color.Red;
                            break;

                        case 1: // 财富
                            resultText = "财富：金光闪烁的前路。";
                            Player.AddBuff(BuffID.Spelunker, buffDuration);
                            Player.AddBuff(BuffID.Midas, buffDuration);
                            textColor = Color.Gold;
                            break;

                        case 2: // 启示
                            resultText = "启示：迷雾消散。";
                            Player.AddBuff(BuffID.Hunter, buffDuration);
                            Player.AddBuff(BuffID.NightOwl, buffDuration);
                            textColor = Color.Cyan;
                            break;

                        case 3: // 战斗
                            resultText = "战斗：你的血液开始沸腾！";
                            Player.AddBuff(BuffID.Wrath, buffDuration);
                            textColor = Color.OrangeRed;
                            break;

                        case 4: // 生存
                            resultText = "生存：活下去的希望。";
                            Player.AddBuff(BuffID.Ironskin, buffDuration);
                            Player.AddBuff(BuffID.Regeneration, buffDuration);
                            textColor = Color.LightGreen;
                            break;

                        case 5: // 眷顾
                            resultText = "眷顾：女神在注视着你。";
                            Player.AddBuff(BuffID.Lucky, buffDuration);
                            spiritualityCurrent += 30;
                            if (spiritualityCurrent > spiritualityMax) spiritualityCurrent = spiritualityMax;
                            textColor = Color.Pink;
                            break;
                    }

                    Main.NewText($"占卜结果: {resultText}", textColor);
                }
                else if (divinationCooldown > 0)
                {
                    Main.NewText($"灵性直觉正在平复... ({divinationCooldown / 60}s)", 150, 150, 150);
                }
                else
                {
                    Main.NewText("灵性不足。", 255, 50, 50);
                }
            }
            // ==================================================================
            //   错误途径 (Marauder) 完整按键逻辑整合
            //   包含：序列9-1 所有主动技能 (处理了 Shift 组合键优先级)
            // ==================================================================

            // 获取按键状态
            bool isShiftDown = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift);

            // ------------------------------------------------------------------
            // 1. P 键逻辑 (寄生 / 领域)
            // ------------------------------------------------------------------
            if (LotMKeybinds.Marauder_Parasite.JustPressed)
            {
                // --- [Shift + P] : 领域类技能 (高序列优先) ---
                if (isShiftDown)
                {
                    // 【序列1】 时之虫领域 (Time Clock Domain)
                    if (currentMarauderSequence <= 1)
                    {
                        isTimeClockActive = !isTimeClockActive;
                        isDeceitDomainActive = false; // 开启高级领域时，自动关闭低级领域

                        if (isTimeClockActive)
                        {
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item113, Player.position); // 钟声
                            Main.NewText("时间权柄：古老壁钟已降临 (周围敌人极度减速并衰老)。", 0, 255, 255);
                        }
                        else
                        {
                            Main.NewText("壁钟虚影消散。", 150, 150, 150);
                        }
                    }
                    // 【序列3】 欺瞒领域 (Deceit Domain)
                    else if (currentMarauderSequence <= 3)
                    {
                        isDeceitDomainActive = !isDeceitDomainActive;
                        if (isDeceitDomainActive)
                        {
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.position);
                            Main.NewText("欺瞒领域：现实的规则已被扭曲 (敌人混乱/弹幕偏转)。", 0, 255, 127);
                        }
                        else
                        {
                            Main.NewText("欺瞒领域：关闭。", 150, 150, 150);
                        }
                    }
                }
                // --- [单按 P] : 寄生技能 (序列4) ---
                else if (currentMarauderSequence <= 4)
                {
                    if (isParasitizing)
                    {
                        // 主动解除寄生
                        isParasitizing = false;
                        Player.velocity = new Vector2(0, -10); // 弹射出来
                        Player.immune = true;
                        Player.immuneTime = 60;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath13, Player.position);
                        Main.NewText("解除寄生状态。", 200, 200, 200);
                    }
                    else
                    {
                        // 尝试寄生鼠标指向的目标
                        int targetIndex = -1;
                        float maxDist = 300f;
                        bool foundPlayer = false; // 标记是否找到了玩家

                        // 1. 优先寻找 NPC
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC n = Main.npc[i];
                            // 必须活着 + 鼠标指向 + 距离内
                            if (n.active && n.getRect().Contains(Main.MouseWorld.ToPoint()) && Player.Distance(n.Center) < maxDist)
                            {
                                targetIndex = i;
                                parasiteIsPlayer = false; // 是NPC
                                break;
                            }
                        }

                        // 2. 如果没找到 NPC，寻找 玩家 (队友)
                        if (targetIndex == -1)
                        {
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                Player p = Main.player[i];
                                // 必须活着 + 不是自己 + 鼠标指向 + 距离内
                                if (p.active && !p.dead && p.whoAmI != Player.whoAmI &&
                                    p.getRect().Contains(Main.MouseWorld.ToPoint()) && Player.Distance(p.Center) < maxDist)
                                {
                                    targetIndex = i;
                                    parasiteIsPlayer = true; // 是玩家
                                    foundPlayer = true;
                                    break;
                                }
                            }
                        }

                        // 3. 执行寄生
                        if (targetIndex != -1)
                        {
                            if (TryConsumeSpirituality(50))
                            {
                                isParasitizing = true;
                                parasiteTargetIndex = targetIndex;
                                Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath13, Player.position);

                                if (foundPlayer)
                                {
                                    // 寄生玩家逻辑
                                    parasiteIsPlayer = true;
                                    parasiteIsTownNPC = false;
                                    Main.NewText($"已寄生于队友 {Main.player[targetIndex].name} 体内 (生命/灵性共享)", 100, 255, 255);
                                }
                                else
                                {
                                    // 寄生 NPC 逻辑
                                    parasiteIsPlayer = false;
                                    parasiteIsTownNPC = Main.npc[targetIndex].townNPC;
                                    if (parasiteIsTownNPC)
                                        Main.NewText($"已寄生于 {Main.npc[targetIndex].FullName} 体内 (浅层寄生/恢复伤势)", 100, 255, 100);
                                    else
                                        Main.NewText($"已深度寄生目标！(控制/生命窃取)", 255, 100, 100);
                                }
                            }
                            else Main.NewText("灵性不足，无法寄生！", 255, 50, 50);
                        }
                        else Main.NewText("未找到可寄生的目标 (NPC或队友)", 150, 150, 150);
                    }
                }
            }

            // ------------------------------------------------------------------
            // 2. O 键逻辑 (窃取 / 偷盗)
            // ------------------------------------------------------------------
            if (LotMKeybinds.MarauderSteal.JustPressed)
            {
                // --- [Shift + O] : 强力窃取技能 (高序列优先) ---
                if (isShiftDown)
                {
                    // 【序列1】 窃取时间 (Time Theft)
                    if (currentMarauderSequence <= 1)
                    {
                        // 消耗保持 500，冷却时间从 10秒(600) 降低到 8秒(480)
                        if (timeTheftCooldown <= 0 && TryConsumeSpirituality(500))
                        {
                            int targetIdx = -1;
                            // 稍微扩大一点鼠标判定范围，防止点不到怪
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                if (Main.npc[i].active && !Main.npc[i].friendly && Main.npc[i].getRect().Intersects(Utils.CenteredRectangle(Main.MouseWorld, new Vector2(50, 50))))
                                {
                                    targetIdx = i;
                                    break;
                                }
                            }

                            if (targetIdx != -1)
                            {
                                NPC target = Main.npc[targetIdx];

                                // === 伤害计算公式修改 ===
                                // 1. 基础伤害：固定 10000 (保证下限)
                                int baseDamage = 10000;

                                // 2. 寿命剥夺：目标当前生命的 15% (原为10%)
                                int lifeStealDamage = (int)(target.life * 0.15f);

                                int totalDamage = baseDamage + lifeStealDamage;

                                // 3. 伤害上限修正
                                if (target.boss)
                                {
                                    // Boss 伤害上限提升至 100,000 (原为 5,000)
                                    // 这样面对几十万血的 Boss 也能一刀切掉一大块
                                    if (totalDamage > 100000) totalDamage = 100000;
                                }
                                else
                                {
                                    // 对小怪：直接造成 5倍当前生命伤害 (确切的秒杀)
                                    totalDamage = target.life * 5;
                                    if (totalDamage < 20000) totalDamage = 20000;
                                }

                                // 造成伤害 (暴击)
                                Player.ApplyDamageToNPC(target, totalDamage, 0, 0, true);

                                // 自身获得强力 Buff
                                Player.AddBuff(BuffID.Panic, 600);    // 加移速
                                Player.AddBuff(BuffID.Swiftness, 600);// 加移速
                                Player.AddBuff(BuffID.ShadowDodge, 300); // 【新增】窃取了时间，获得一次神圣闪避

                                // 视觉特效
                                CombatText.NewText(target.getRect(), Color.Cyan, $"时间剥夺! -{totalDamage}", true);
                                SoundEngine.PlaySound(SoundID.Item14, target.Center); // 爆炸音效
                                for (int k = 0; k < 40; k++)
                                {
                                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Vortex, Main.rand.NextVector2Circular(10, 10), 0, default, 2.5f);
                                    d.noGravity = true;
                                }

                                timeTheftCooldown = 480; // 冷却降低为 8秒
                            }
                        }
                        else if (timeTheftCooldown > 0) Main.NewText($"窃取时间冷却中: {timeTheftCooldown / 60}s", 150, 150, 150);
                    }
                    // 【序列2】 命运窃取 (Fate Theft)
                    else if (currentMarauderSequence <= 2)
                    {
                        if (fateTheftCooldown <= 0 && TryConsumeSpirituality(200))
                        {
                            int targetIdx = -1;
                            for (int i = 0; i < Main.maxNPCs; i++) { if (Main.npc[i].active && !Main.npc[i].friendly && Main.npc[i].getRect().Contains(Main.MouseWorld.ToPoint())) { targetIdx = i; break; } }

                            if (targetIdx != -1)
                            {
                                NPC target = Main.npc[targetIdx];
                                if (!target.boss)
                                {
                                    float myRatio = (float)Player.statLife / Player.statLifeMax2;
                                    float targetRatio = (float)target.life / target.lifeMax;
                                    if (myRatio < targetRatio)
                                    {
                                        int newLife = (int)(targetRatio * Player.statLifeMax2);
                                        int targetNewLife = (int)(myRatio * target.lifeMax);
                                        Player.statLife = newLife; Player.HealEffect(newLife - (int)(myRatio * Player.statLifeMax2));
                                        target.life = targetNewLife;
                                        CombatText.NewText(target.getRect(), Color.Purple, "命运互换!", true);
                                    }
                                    else Main.NewText("你的命运优于目标。", 150, 150, 150);
                                }
                                else
                                {
                                    Player.ApplyDamageToNPC(target, 5000, 0, 0, false);
                                    Player.statLife += 500; Player.HealEffect(500);
                                    CombatText.NewText(target.getRect(), Color.Purple, "窃取未来!", true);
                                }
                                fateTheftCooldown = 1200;
                            }
                        }
                        else if (fateTheftCooldown > 0) Main.NewText($"命运窃取冷却中: {fateTheftCooldown / 60}s", 150, 150, 150);
                    }
                }
        // --- [单按 O] : 基础窃取模式 (序列9) ---
                else if (currentMarauderSequence <= 9)
                {
                    stealMode = !stealMode;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
                    if (stealMode) Main.NewText("窃取模式：开启 (商店免费，小心被发现...)", 255, 100, 100);
                    else Main.NewText("窃取模式：关闭", 100, 255, 100);
                }
            }
            if (LotMKeybinds.Marauder_ConceptSteal.JustPressed)
            {
                // 只有序列 4 (寄生者) 及以下可以使用
                if (currentMarauderSequence <= 4)
                {
                    if (conceptStealCooldown <= 0 && TryConsumeSpirituality(80))
                    {
                        int targetIdx = -1;
                        // 寻找鼠标指向的 NPC
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (Main.npc[i].active && !Main.npc[i].friendly && Main.npc[i].getRect().Contains(Main.MouseWorld.ToPoint()))
                            {
                                targetIdx = i;
                                break;
                            }
                        }

                        if (targetIdx != -1)
                        {
                            NPC n = Main.npc[targetIdx];
                            if (!n.boss)
                            {
                                // 窃取位置 (互换)
                                Vector2 pPos = Player.Center;
                                Player.Teleport(n.Center, 1);
                                n.Center = pPos;
                                CombatText.NewText(Player.getRect(), Color.Orange, "位置窃取!", true);
                                conceptStealCooldown = 180;
                            }
                            else
                            {
                                Main.NewText("位格过高无法窃取位置！", 255, 50, 50);
                            }
                        }
                        else
                        {
                            // 窃取距离 (传送)
                            Player.Teleport(Main.MouseWorld, 1);
                            Main.NewText("窃取距离。", 200, 200, 255);
                            conceptStealCooldown = 120;
                        }
                    }
                    else if (conceptStealCooldown > 0)
                    {
                        Main.NewText($"概念窃取冷却: {conceptStealCooldown / 60}s", 150, 150, 150);
                    }
                }
                else
                {
                    // (可选) 提示序列不够
                    // Main.NewText("序列不足 (需序列4)", 150, 150, 150);
                }
            }
        }

        // 辅助方法：执行奇迹愿望
        private void CastMiracleWish()
        {
            if (miracleCooldown <= 0 && TryConsumeSpirituality(2000))
            {
                SoundEngine.PlaySound(SoundID.Item29, Player.position);
                miracleCooldown = 3600;
                switch (selectedWish)
                {
                    case 0: // 生命
                        Player.statLife = Player.statLifeMax2; Player.HealEffect(Player.statLifeMax2);
                        for (int i = 0; i < Player.MaxBuffs; i++) if (Main.debuff[Player.buffType[i]]) Player.DelBuff(i);
                        Main.NewText("奇迹：生命复苏！", 0, 255, 0);
                        break;
                    case 1: // 毁灭
                        foreach (NPC n in Main.ActiveNPCs) if (!n.friendly && !n.dontTakeDamage && n.Distance(Player.Center) < 1500f)
                            {
                                Player.ApplyDamageToNPC(n, 10000, 0, 0, false);
                                Projectile.NewProjectile(Player.GetSource_FromThis(), n.Center, Vector2.Zero, ProjectileID.Electrosphere, 1000, 0, Player.whoAmI);
                            }
                        Main.NewText("奇迹：毁灭天灾！", 255, 0, 0);
                        break;
                    case 2: // 空间传送
                        waitingForTeleport = true; // 开启等待状态
                        Main.NewText("奇迹：空间折叠已展开，请打开地图(M)或在屏幕上点击任意位置进行传送。", 0, 255, 255);
                        break;
                    case 3: // 昼夜
                        Main.time = 0; Main.dayTime = !Main.dayTime;
                        Main.NewText("奇迹：昼夜更替。", 200, 200, 200);
                        break;
                }
            }
            else if (miracleCooldown > 0) Main.NewText($"奇迹冷却中: {miracleCooldown / 60}s", 150, 150, 150);
            else Main.NewText("灵性不足！", 255, 50, 50);
        }

        // 辅助方法：打印命运状态
        private void PrintFateState()
        {
            if (fateDisturbanceActive) Main.NewText("命运干扰: 开启", 200, 100, 255);
            else Main.NewText("命运干扰: 关闭", 150, 150, 150);
        }

        // 8. 辅助
        public float GetSequenceMultiplier(int seq) { if (seq > 9) return 1f; return 1f + (9 - seq) * 0.3f; }
        public bool TryConsumeSpirituality(float amount, bool isMaintenance = false) { if (isCalamityGiant && !isMaintenance) return true; if (spiritualityCurrent >= amount) { spiritualityCurrent -= amount; return true; } return false; }
        public override void ModifyScreenPosition() { if (shakeTime > 0) { Main.screenPosition += Main.rand.NextVector2Circular(shakePower, shakePower); shakeTime--; } base.ModifyScreenPosition(); }
        public override bool CanUseItem(Item item) { if (isFireForm || isGuardianStance || isMoonlightized || isBatSwarm) return false; return base.CanUseItem(item); }
        private void CalculateMaxSpirituality()
        {
            // 基础灵性
            int max = 100;

            // --- 1. 愚者途径 (Fool) ---
            if (currentFoolSequence <= 9) max = Math.Max(max, 200);
            if (currentFoolSequence <= 8) max = Math.Max(max, 300);
            if (currentFoolSequence <= 7) max = Math.Max(max, 500);
            if (currentFoolSequence <= 6) max = Math.Max(max, 1000);
            if (currentFoolSequence <= 4) max = Math.Max(max, 5000);
            if (currentFoolSequence <= 3) max = Math.Max(max, 20000);
            if (currentFoolSequence <= 2) max = Math.Max(max, 100000);
            if (currentFoolSequence <= 1) max = Math.Max(max, 200000);
            // 未来更高序列可继续添加...

            // --- 2. 月亮途径 (Moon) ---
            if (currentMoonSequence <= 9) max = Math.Max(max, 150); // 药师灵性稍低
            if (currentMoonSequence <= 8) max = Math.Max(max, 250);
            if (currentMoonSequence <= 7) max = Math.Max(max, 600); // 吸血鬼质变
            if (currentMoonSequence <= 6) max = Math.Max(max, 1000);
            if (currentMoonSequence <= 5) max = Math.Max(max, 2000);
            if (currentMoonSequence <= 4) max = Math.Max(max, 5000);
            if (currentMoonSequence <= 3) max = Math.Max(max, 10000);
            if (currentMoonSequence <= 2) max = Math.Max(max, 50000);
            if (currentMoonSequence <= 1) max = Math.Max(max, 100000);

            // --- 3. 猎人途径 (Hunter) ---
            // 猎人主要靠体术，灵性成长较慢，直到高序列
            if (currentHunterSequence <= 9) max = Math.Max(max, 120);
            if (currentHunterSequence <= 8) max = Math.Max(max, 150);
            if (currentHunterSequence <= 7) max = Math.Max(max, 300); // 纵火家
            if (currentHunterSequence <= 6) max = Math.Max(max, 500);
            if (currentHunterSequence <= 5) max = Math.Max(max, 800);
            if (currentHunterSequence <= 4) max = Math.Max(max, 1500);
            if (currentHunterSequence <= 3) max = Math.Max(max, 3000);
            if (currentHunterSequence <= 2) max = Math.Max(max, 10000); // 天气术士
            if (currentHunterSequence <= 1) max = Math.Max(max, 100000); // 征服者

            // --- 4. 巨人途径 (Giant) ---
            // 战士途径灵性最低，主要靠体力
            if (currentSequence <= 9) max = Math.Max(max, 110);
            if (currentSequence <= 8) max = Math.Max(max, 130);
            if (currentSequence <= 7) max = Math.Max(max, 200);
            if (currentSequence <= 6) max = Math.Max(max, 400); // 黎明骑士
            if (currentSequence <= 5) max = Math.Max(max, 700);
            if (currentSequence <= 4) max = Math.Max(max, 1200);
            if (currentSequence <= 3) max = Math.Max(max, 2500); // 银骑士
            if (currentSequence <= 2) max = Math.Max(max, 50000); // 荣耀战神 (神性质变)

            // [新增] 错误途径 (灵性专长，成长较高)
            if (currentMarauderSequence <= 9) max = Math.Max(max, 100);
            if (currentMarauderSequence <= 8) max = Math.Max(max, 200);
            if (currentMarauderSequence <= 7) max = Math.Max(max, 500);
            if (currentMarauderSequence <= 6) max = Math.Max(max, 1000);
            if (currentMarauderSequence <= 5) max = Math.Max(max, 2000);
            if (currentMarauderSequence <= 4) max = Math.Max(max, 5000);
            if (currentMarauderSequence <= 3) max = Math.Max(max, 10000);
            if (currentMarauderSequence <= 2) max = Math.Max(max, 50000);
            if (currentMarauderSequence <= 1) max = Math.Max(max, 100000);

            spiritualityMax = max;
        }
        private void HandleSpiritualityRegen()
        {
            spiritualityRegenTimer++;
            if (spiritualityRegenTimer >= 60) // 每秒触发一次
            {
                spiritualityRegenTimer = 0;

                // 基础回复：2 + 1% 最大灵性
                float regen = 2f + (spiritualityMax * 0.01f);

                // [新增] 错误途径特权：更快的灵性回复 (每提升1个序列，回复速度+5%)
                if (currentMarauderSequence <= 9)
                {
                    float marauderBonus = 1f + (9 - currentMarauderSequence) * 0.05f;
                    regen *= marauderBonus;
                }

                // 月亮途径高序列回复加成
                if (currentMoonSequence <= 2) regen += (spiritualityMax * 0.04f);

                spiritualityCurrent += regen;

                // 确保不超过上限
                if (spiritualityCurrent > spiritualityMax) spiritualityCurrent = spiritualityMax;
            }
        }
        private void HandleDawnArmorLogic() { if (dawnArmorBroken) { dawnArmorCooldownTimer--; if (dawnArmorCooldownTimer <= 0) { dawnArmorBroken = false; dawnArmorCurrentHP = MaxDawnArmorHP; Main.NewText("铠甲已重铸", 100, 255, 100); } } else if (!dawnArmorActive && dawnArmorCurrentHP < MaxDawnArmorHP && Main.GameUpdateCount % 2 == 0) dawnArmorCurrentHP++; }
        private void SpawnVisualDust() { for (int i = 0; i < 40; i++) Dust.NewDustPerfect(Player.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(5f, 5f), 100, default, 2.0f).noGravity = true; }
        private void CheckConquerorRitual() { if (currentHunterSequence == 2 && !conquerorRitualComplete && ConquerorSpawnSystem.StopSpawning) { bool enemyExists = false; for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.townNPC && npc.lifeMax > 5 && !npc.dontTakeDamage) { enemyExists = true; break; } } if (!enemyExists) { conquerorRitualComplete = true; Main.NewText("这片大陆已无敌手... 征服的意志已达成！", 255, 0, 0); SoundEngine.PlaySound(SoundID.Roar, Player.position); } } }

        public override void PostUpdate()
        {
            // 1. 调用寄生逻辑修复 (防脱战/防卡死)
            UpdateParasiteLogic();

            base.PostUpdate();
        }

        private void UpdateParasiteLogic()
        {
            // 如果没有处于寄生状态，直接返回
            if (!isParasitizing || parasiteTargetIndex == -1)
                return;

            // =================================================
            // 分支 A: 寄生玩家 (队友)
            // =================================================
            if (parasiteIsPlayer)
            {
                // 获取目标玩家
                if (parasiteTargetIndex >= Main.maxPlayers) return; // 防止索引越界
                Player targetPlayer = Main.player[parasiteTargetIndex];

                // 1. 安全检查：队友下线或死亡
                if (!targetPlayer.active || targetPlayer.dead)
                {
                    EndParasiteState();
                    Main.NewText("宿主已死亡或断开连接。", 255, 50, 50);
                    return;
                }

                // 2. 锁定位置
                Player.Center = targetPlayer.Center;
                Player.velocity = targetPlayer.velocity;
                Player.gfxOffY = 0;
                Player.direction = targetPlayer.direction; // 朝向跟随

                // 3. 自身状态 (无敌/隐身/不可操作)
                Player.immune = true;
                Player.immuneTime = 2;
                Player.invis = true;
                Player.controlLeft = false; Player.controlRight = false;
                Player.controlUp = false; Player.controlDown = false;
                Player.controlJump = false; Player.controlUseItem = false;

                // 4. 消耗与增益效果
                if (!TryConsumeSpirituality(0.5f / 60f, true))
                {
                    isParasitizing = false;
                    Main.NewText("灵性耗尽，寄生中断！", 255, 50, 50);
                    return;
                }

                // 给自己回血
                Player.lifeRegen += 5;
                // 给宿主(队友)回血
                targetPlayer.lifeRegen += 5;

                // 视觉特效 (每秒冒一次绿光)
                if (Main.GameUpdateCount % 60 == 0)
                {
                    CombatText.NewText(targetPlayer.getRect(), Color.LightGreen, "寄生治疗", false, true);
                }

                return; // 结束方法，不执行下面的 NPC 逻辑
            }

            // =================================================
            // 分支 B: 寄生 NPC
            // =================================================
            if (parasiteTargetIndex >= Main.maxNPCs) return;
            NPC target = Main.npc[parasiteTargetIndex];

            // 1. 安全检查
            if (!target.active || target.life <= 0)
            {
                EndParasiteState();
                return;
            }

            // 2. 防脱战 (Boss)
            if (target.boss || target.type == NPCID.EaterofWorldsHead)
            {
                target.target = Player.whoAmI;
                target.timeLeft = 1000;
            }

            // 3. 锁定位置
            Player.Center = target.Center;
            Player.velocity = target.velocity;
            Player.gfxOffY = 0;

            // 4. 自身状态
            Player.immune = true;
            Player.immuneTime = 2;
            Player.invis = true;
            Player.controlLeft = false; Player.controlRight = false;
            Player.controlUp = false; Player.controlDown = false;
            Player.controlJump = false; Player.controlUseItem = false;

            // 5. 消耗与效果
            float cost = parasiteIsTownNPC ? 0.5f : 5.0f;
            if (!TryConsumeSpirituality(cost / 60f, true))
            {
                isParasitizing = false;
                Main.NewText("灵性耗尽，寄生中断！", 255, 50, 50);
                return;
            }

            if (parasiteIsTownNPC)
            {
                // 浅层寄生逻辑
                Player.lifeRegen += 10;

                // 序列3 仪式
                if (currentMarauderSequence == 3)
                {
                    trojanRitualTimer++;
                    if (trojanRitualTimer % 1800 == 0) Main.NewText($"正在编织命运... ({trojanRitualTimer / 60}s / 300s)", 150, 150, 255);
                    if (trojanRitualTimer == TROJAN_RITUAL_TARGET) { Main.NewText("仪式完成！", 0, 255, 255); Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.position); }
                }
            }
            else
            {
                // 深层寄生逻辑 (伤害)
                int frequency = 60;
                if (currentMarauderSequence <= 3) frequency = 45;
                if (currentMarauderSequence <= 2) frequency = 30;
                if (currentMarauderSequence <= 1) frequency = 15;

                if (Main.GameUpdateCount % frequency == 0)
                {
                    int baseDmg = 150;
                    float seqMult = 1f;
                    if (currentMarauderSequence <= 3) seqMult = 3f;
                    if (currentMarauderSequence <= 2) seqMult = 6f;
                    if (currentMarauderSequence <= 1) seqMult = 12f;

                    int finalDamage = (int)(Player.GetDamage(DamageClass.Generic).ApplyTo(baseDmg) * seqMult);

                    if (target.boss)
                    {
                        float percent = 0.001f;
                        if (currentMarauderSequence <= 2) percent = 0.002f;
                        if (currentMarauderSequence <= 1) percent = 0.005f;
                        int bonus = (int)(target.lifeMax * percent);
                        int cap = 2000 * (5 - currentMarauderSequence);
                        if (bonus > cap) bonus = cap;
                        finalDamage += bonus;
                    }

                    bool crit = currentMarauderSequence <= 2;
                    Player.ApplyDamageToNPC(target, finalDamage, 0, 0, crit);

                    int heal = 5 + (4 - currentMarauderSequence) * 5;
                    if (currentMarauderSequence <= 1) heal += finalDamage / 1000;
                    Player.statLife += heal;
                    Player.HealEffect(heal);

                    target.AddBuff(BuffID.Confused, 120);
                    target.AddBuff(BuffID.Slow, 120);
                    if (currentMarauderSequence <= 3) { target.AddBuff(BuffID.ShadowFlame, 120); target.AddBuff(BuffID.Venom, 120); }

                    CombatText.NewText(target.getRect(), Color.MediumPurple, $"-{finalDamage}", false, true);
                }
            }
        }

        // 解除寄生的辅助方法
        public void EndParasiteState()
        {
            isParasitizing = false;
            parasiteTargetIndex = -1;
            Player.invis = false;
            Player.immune = false;
            // 稍微向下弹开一点，防止解除瞬间卡在 Boss 身体里
            Player.velocity = new Vector2(0, -5f);
            Main.NewText("你解除了寄生状态。", 200, 200, 255);
        }
        public class ApothecaryCrafting : Terraria.ModLoader.GlobalItem { public override void OnCreated(Terraria.Item item, ItemCreationContext context) { if (context is RecipeItemCreationContext) { Player p = Main.LocalPlayer; if (p != null && p.active && p.GetModPlayer<LotMPlayer>().currentMoonSequence <= 9) { bool isP = item.consumable && (item.buffType > 0 || item.healLife > 0 || item.healMana > 0); if (isP) p.QuickSpawnItem(item.GetSource_FromThis(), item.type, item.stack); } } } }
    }
}