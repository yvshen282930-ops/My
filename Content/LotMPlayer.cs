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
using zhashi;
using zhashi.Content.Projectiles;
using zhashi.Content.Items.Weapons;
using zhashi.Content.Buffs;

namespace zhashi.Content
{
    public class LotMPlayer : ModPlayer
    {
        // ===================================================
        // 1. 核心变量定义
        // ===================================================
        public int currentSequence = 10;       // 巨人/战士途径 (9-2)
        public int currentHunterSequence = 10; // 猎人途径 (9-1)
        public int currentMoonSequence = 10;   // 月亮途径 (9-1)
        public int currentFoolSequence = 10;   // 愚者途径 (9-?)


        public bool IsBeyonder => currentSequence < 10 || currentHunterSequence < 10 || currentMoonSequence < 10 || currentFoolSequence< 10;

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

        // --- 月亮途径技能状态 ---
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
        }

        // ===================================================
        // 3. 属性重置与核心逻辑
        // ===================================================
        public override void PreUpdate()
        {
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
            // 灵肉转化：每帧消耗灵性
            if (isSpiritForm)
            {
                if (!TryConsumeSpirituality(150.0f, true))
                {
                    isSpiritForm = false;
                    Main.NewText("灵性枯竭，被迫回归血肉之躯。", 255, 50, 50);
                }
                else
                {
                    // 免疫伤害
                    Player.immune = true;
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
            CheckConquerorRitual();
        }

        // ===================================================
        // 4. 数值加成系统
        // ===================================================
        private void ApplySequenceStats()
        {
            float giantMult = GetSequenceMultiplier(currentSequence);
            float hunterMult = GetSequenceMultiplier(currentHunterSequence);
            float moonMult = GetSequenceMultiplier(currentMoonSequence);
            float foolMult = GetSequenceMultiplier(currentFoolSequence); // 计算愚者倍率

            // --- 通用成长 ---
            float maxMult = Math.Max(giantMult, Math.Max(hunterMult, Math.Max(moonMult, foolMult)));
            if (maxMult > 1f)
            {
                Player.moveSpeed += 0.15f * maxMult;
                Player.maxRunSpeed += 1.5f * maxMult;
                Player.jumpSpeedBoost += 1.2f * (maxMult - 1f);
            }

            // --- 巨人/战士 ---
            if (currentSequence <= 9) { Player.statDefense += (int)(8 * giantMult); Player.GetDamage(DamageClass.Melee) += 0.12f * giantMult; Player.GetCritChance(DamageClass.Melee) += 5; Player.statLifeMax2 += (int)(100 * giantMult); }
            if (currentSequence <= 8) { Player.GetAttackSpeed(DamageClass.Melee) += 0.15f; Player.endurance += 0.05f; Player.noKnockback = true; }
            if (currentSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.10f; Player.GetCritChance(DamageClass.Generic) += 5; Player.GetArmorPenetration(DamageClass.Generic) += 10 * giantMult; }
            if (currentSequence <= 6)
            {
                Lighting.AddLight(Player.Center, 1.5f, 1.5f, 1.5f);
                Player.statDefense += 15;
                Player.lifeRegen += (int)(3 * giantMult);
                if (dawnArmorActive && !dawnArmorBroken)
                {
                    Player.statDefense += (int)(40 * giantMult);
                    Player.endurance += 0.15f * giantMult;
                }
            }
            if (currentSequence <= 5)
            {
                Player.statDefense += 20;
                Player.endurance += 0.05f;
                Player.buffImmune[BuffID.Confused] = true;
                if (isGuardianStance)
                {
                    Player.statDefense += (int)(100 * giantMult);
                    Player.endurance += 0.3f * giantMult;
                }
            }
            if (currentSequence <= 4) { Player.statLifeMax2 += (int)(500 * giantMult); Player.GetDamage(DamageClass.Generic) += 0.20f; Player.GetCritChance(DamageClass.Generic) += 10; Player.nightVision = true; Player.detectCreature = true; Player.buffImmune[BuffID.CursedInferno] = true; Player.buffImmune[BuffID.ShadowFlame] = true; }
            if (currentSequence <= 3) { Player.statDefense += 30; Player.lifeRegen += 5; Player.GetAttackSpeed(DamageClass.Melee) += 0.20f; Player.blackBelt = true; }
            if (currentSequence <= 2) { Player.statLifeMax2 += 2000; Player.statDefense += 50; Player.endurance += 0.15f; Player.GetDamage(DamageClass.Generic) += 0.20f; }

            // --- 猎人途径 ---
            if (currentHunterSequence <= 9) { Player.GetDamage(DamageClass.Ranged) += 0.15f; Player.GetDamage(DamageClass.Melee) += 0.05f; Player.detectCreature = true; Player.dangerSense = true; }
            if (currentHunterSequence <= 8) { Player.statDefense += 10; Player.aggro += 300; Player.lifeRegen += 2; }
            if (currentHunterSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.15f * hunterMult; Player.buffImmune[BuffID.OnFire] = true; Player.buffImmune[BuffID.OnFire3] = true; Player.buffImmune[BuffID.Frostburn] = true; Player.resistCold = true; }
            if (currentHunterSequence <= 6) { Player.GetCritChance(DamageClass.Generic) += 15; Player.manaCost -= 0.20f; }
            if (currentHunterSequence <= 5) { Player.GetArmorPenetration(DamageClass.Generic) += 30 * hunterMult; Player.GetCritChance(DamageClass.Generic) += 20; }
            if (currentHunterSequence <= 4) { Player.statDefense += (int)(50 * hunterMult); Player.endurance += 0.10f; Player.maxMinions += 5; Player.noKnockback = true; }
            if (currentHunterSequence <= 3) { Player.maxMinions += 10; Player.maxTurrets += 3; Player.GetDamage(DamageClass.Summon) += 0.40f; }
            if (currentHunterSequence <= 2) { Player.statLifeMax2 += 600; Player.statManaMax2 += 300; Player.GetDamage(DamageClass.Generic) += 0.40f; Player.buffImmune[BuffID.WindPushed] = true; }
            if (currentHunterSequence <= 1) { Player.statDefense += 100; Player.endurance += 0.25f; Player.GetDamage(DamageClass.Generic) += 1.0f; Player.GetCritChance(DamageClass.Generic) += 40; Player.aggro += 2000; Player.buffImmune[BuffID.Weak] = true; Player.buffImmune[BuffID.BrokenArmor] = true; Player.buffImmune[BuffID.WitheredArmor] = true; Player.buffImmune[BuffID.WitheredWeapon] = true; }

            // --- 月亮途径 ---
            if (currentMoonSequence <= 9)
            {
                Player.buffImmune[BuffID.Poisoned] = true;
                Player.buffImmune[BuffID.Venom] = true;
                Player.detectCreature = true;
                Player.lifeRegen += (int)(3 * moonMult);
                Player.statLifeMax2 += (int)(30 * moonMult);
            }
            if (currentMoonSequence <= 8) { Player.statDefense += 8; Player.GetDamage(DamageClass.Generic) += 0.15f * moonMult; Player.moveSpeed += 0.3f; Player.maxMinions += (int)(2 * moonMult); Player.dangerSense = true; }
            if (currentMoonSequence <= 7)
            {
                Player.statLifeMax2 += 100;
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
            if (currentMoonSequence <= 4) { Player.statLifeMax2 += 300; Player.statManaMax2 += 200; Player.GetDamage(DamageClass.Magic) += 0.30f * moonMult; Player.GetDamage(DamageClass.Summon) += 0.30f * moonMult; }
            if (currentMoonSequence <= 3) { Player.maxMinions += (int)(5 * moonMult); Player.GetDamage(DamageClass.Summon) += 0.40f * moonMult; Player.GetKnockback(DamageClass.Summon) += 3f; Player.statManaMax2 += 300; Player.manaCost -= 0.25f; }
            if (currentMoonSequence <= 2)
            {
                Player.statLifeMax2 += 1500;
                Player.lifeRegen += 60;
                Player.buffImmune[BuffID.Bleeding] = true; Player.buffImmune[BuffID.Poisoned] = true; Player.buffImmune[BuffID.Venom] = true; Player.buffImmune[BuffID.CursedInferno] = true; Player.buffImmune[BuffID.Ichor] = true; Player.buffImmune[BuffID.Frozen] = true;
                if (isCreationDomain) { Player.lifeRegen += 60; Player.statDefense += 50; Lighting.AddLight(Player.Center, 0.1f, 0.8f, 0.2f); Player.flowerBoots = true; }
            }
            if (currentMoonSequence <= 1)
            {
                Player.statLifeMax2 += 3000;
                Player.endurance += 0.20f;
                Player.GetDamage(DamageClass.Generic) += 0.80f;
                for (int i = 0; i < BuffID.Count; i++) { if (Main.debuff[i]) Player.buffImmune[i] = true; }
                if (isCreationDomain) { Player.GetDamage(DamageClass.Generic) += 0.50f; Lighting.AddLight(Player.Center, 1.0f, 0.4f, 0.7f); }
            }
            // --- 愚者途径 (The Fool) ---
            if (currentFoolSequence <= 9) 
            {
                Player.GetDamage(DamageClass.Magic) += 0.10f * foolMult;
                Player.GetCritChance(DamageClass.Magic) += 5;

                // 2. 灵性提升：占卜家拥有较高的灵性
                Player.statManaMax2 += (int)(40 * foolMult);

                // 3. 直觉：微弱的危险感知
                Player.dangerSense = true;

                // 4. 灵视 (Spirit Vision)：如果不手动关闭，常驻开启
                if (isSpiritVisionActive)
                {
                    // 1.5f 表示每帧消耗 1.5 (即每秒消耗 90 点)，这是非常大的消耗
                    // 如果灵性不足，自动关闭
                    if (!TryConsumeSpirituality(1.5f, true))
                    {
                        isSpiritVisionActive = false;
                        Main.NewText("灵性枯竭，灵视被迫中断！", 255, 50, 50);
                    }
                    else
                    {
                        Lighting.AddLight(Player.Center, 0.4f, 0.4f, 1.0f);
                    }
                }

                // 5. 幸运提升 (玄学)
                Player.luck += 0.5f * foolMult;
            }
            if (currentFoolSequence <= 8)
            {
                Player.moveSpeed += 0.3f;        // 移速增加
                Player.jumpSpeedBoost += 1.5f;   // 跳跃速度
                Player.accRunSpeed += 2.0f;      // 跑步加速度

                // 2. 技巧性格斗：通用伤害与近战攻速提升
                Player.GetDamage(DamageClass.Generic) += 0.15f * foolMult;
                Player.GetAttackSpeed(DamageClass.Melee) += 0.15f;

                // 3. 直觉预感 (进攻端)：精准预判目标行动 = 暴击率大幅提升
                Player.GetCritChance(DamageClass.Generic) += 10;

                // 4. 直觉预感 (防守端)：闪避能力 (BlackBelt效果: 10%几率闪避)
                Player.blackBelt = true;

                // 灵性上限继续提升
                Player.statManaMax2 += (int)(60 * foolMult);
            }
            if (currentFoolSequence <= 7)
            {
                Player.GetAttackSpeed(DamageClass.Generic) += 0.2f; // 全攻速提升
                Player.manaCost -= 0.15f; // 施法消耗降低 (不需灌注灵性)

                // 2. 骨骼软化：免疫束缚类Debuff
                Player.buffImmune[BuffID.Webbed] = true;
                Player.buffImmune[BuffID.Stoned] = true;

                // 3. 虚假的水下呼吸 (自带呼吸管)
                Player.gills = true;
                Player.ignoreWater = true; // 在水中移动不受阻力

                // 灵性上限大幅提升
                Player.statManaMax2 += (int)(100 * foolMult);
            }
            if (currentFoolSequence <= 6)
            {
                // 1. 观察与回忆：获得信息类道具效果
                Player.accCritterGuide = true; // 生物分析
                Player.accStopwatch = true;    // 秒表
                Player.accOreFinder = true;    // 金属探测

                // 2. 占卜与格斗增强
                Player.GetDamage(DamageClass.Generic) += 0.15f * foolMult; // 再次提升伤害
                Player.GetCritChance(DamageClass.Generic) += 10;           // 再次提升暴击

                // 3. 空气弹威力翻倍 (通过增加魔法伤害倍率来实现，或者在 ModifyWeaponDamage 里特判)
                // 这里简单点，直接给高额魔法伤害加成，体现"法术威力大增"
                Player.GetDamage(DamageClass.Magic) += 0.3f * foolMult;

                // 4. 虚假的水下呼吸 (管道翻倍 -> 水下无限呼吸)
                Player.gills = true;

                // 5. 无面伪装 (Faceless Mode)
                if (isFacelessActive)
                {
                    // 改变容貌：不仅降低仇恨，还增加闪避
                    Player.aggro -= 1000; // 极低仇恨，敌人很难发现你
                    Player.shroomiteStealth = true; // 获得像蘑菇套一样的隐身视觉效果
                    Player.statDefense += 10; // 伪装带来的防御

                    // 消耗灵性维持
                    if (!TryConsumeSpirituality(1.0f, true))
                    {
                        isFacelessActive = false;
                        Main.NewText("灵性不足，伪装失效！", 255, 50, 50);
                    }
                }

                // 灵性上限继续提升
                Player.statManaMax2 += (int)(150 * foolMult);
            }
            if (currentFoolSequence <= 5)
            {
                // 1. 灵体之线视野：能看见100米内的灵体 (永久猎人药水 + 探宝)
                Player.detectCreature = true;
                Player.dangerSense = true;
                Player.findTreasure = true;

                // 2. 秘偶化：召唤栏位 +3 (可以操纵3个秘偶)
                Player.maxMinions += 3;

                // 3. 空气弹威力再次提升
                Player.GetDamage(DamageClass.Magic) += 0.2f * foolMult;

                // 4. 操纵火焰与跳跃：增强
                // (这部分通过 foolMult 自动加成伤害，火焰跳跃距离在 ProcessTriggers 里写了逻辑)

                // 灵性上限质变
                Player.statManaMax2 += (int)(200 * foolMult);
            }
            if (currentFoolSequence <= 4)
            {
                // 1. 半神质变：全属性大幅提升
                Player.statLifeMax2 += 500;
                Player.statDefense += 20;
                Player.GetDamage(DamageClass.Generic) += 0.2f * foolMult;

                // 2. 灵体之线：操纵范围提升，秘偶数量大增
                // 虽然原著是50个，但泰拉瑞亚里太多会卡，这里给 +7 (总共10个左右)
                Player.maxMinions += 7;

                // 3. 变形：更强的隐秘能力 (极难被发现)
                Player.aggro -= 2000;

                // 4. 空气弹：威力变为炮弹 (ModifyWeaponDamage里处理)

                // 灵性上限半神级
                Player.statManaMax2 += (int)(400 * foolMult);
            }
            if (currentFoolSequence <= 3)
            {
                // 1. 基础属性：圣者级
                Player.statLifeMax2 += 1000;
                Player.statDefense += 40;
                Player.GetDamage(DamageClass.Generic) += 0.4f * foolMult;

                // 2. 灵之虫强化：上限 600
                // (在PreKill里逻辑处理)

                // 3. 昨日重现 (Buff效果)
                if (isBorrowingPower)
                {
                    // 全属性爆发 (模拟全盛时期的力量)
                    Player.GetDamage(DamageClass.Generic) += 0.5f; // 额外+50%
                    Player.statDefense += 50;
                    Player.lifeRegen += 20;
                    Player.moveSpeed += 0.5f;
                    Player.endurance += 0.2f; // 20%免伤

                    // 视觉特效：历史残影
                    if (Main.rand.NextBool(3))
                    {
                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke, 0, 0, 100, Color.Gray, 1.5f);
                    }
                }
                // 1. 历史投影 (Y键)
                // 序列3: 上限3个, 召唤Boss
                // 序列2: 上限9个, Shift+Y 召唤历史场景
                if (LotMKeybinds.Fool_History.JustPressed && currentFoolSequence <= 3)
                {
                    // 手动计数当前存在的历史投影
                    int currentProjections = 0;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active &&
                            Main.projectile[i].owner == Player.whoAmI &&
                            Main.projectile[i].type == ModContent.ProjectileType<HistoricalBossProjectile>())
                        {
                            currentProjections++;
                        }
                    }

                    // 设定上限：奇迹师(9个) / 古代学者(3个)
                    int maxProj = (currentFoolSequence <= 2) ? 9 : 3;

                    // [序列2特权] 按住 Shift 键 + Y：召唤历史场景 (领域)
                    if (currentFoolSequence <= 2 && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                    {
                        // 检查是否已有场景 (避免重叠召唤)
                        if (Player.ownedProjectileCounts[ModContent.ProjectileType<HistoricalSceneProjectile>()] < 1)
                        {
                            if (TryConsumeSpirituality(1000))
                            {
                                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
                                    ModContent.ProjectileType<HistoricalSceneProjectile>(), 0, 0, Player.whoAmI);

                                SoundEngine.PlaySound(SoundID.Item4, Player.position);
                                Main.NewText("一段遗落的历史重临世间... (全属性增益领域)", 200, 200, 255);
                            }
                        }
                        else
                        {
                            Main.NewText("历史场景已存在。", 150, 150, 150);
                        }
                    }
                    // 普通召唤：Boss 投影
                    else if (currentProjections < maxProj)
                    {
                        if (TryConsumeSpirituality(500))
                        {
                            // === Boss 池构建 ===
                            System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>> defeatedBosses = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>>();

                            // 辅助添加函数
                            void AddBoss(int id, int power) { defeatedBosses.Add(new System.Collections.Generic.KeyValuePair<int, int>(id, power)); }

                            // 根据击杀记录添加 Boss (ID, 强度分)
                            if (NPC.downedSlimeKing) AddBoss(NPCID.KingSlime, 40);
                            if (NPC.downedBoss1) AddBoss(NPCID.EyeofCthulhu, 50);
                            if (NPC.downedBoss2) AddBoss(NPCID.EaterofWorldsHead, 60);
                            if (NPC.downedQueenBee) AddBoss(NPCID.QueenBee, 70);
                            if (NPC.downedBoss3) AddBoss(NPCID.SkeletronHead, 80);
                            if (Main.hardMode) AddBoss(NPCID.WallofFlesh, 100);
                            if (NPC.downedQueenSlime) AddBoss(NPCID.QueenSlimeBoss, 120);
                            if (NPC.downedMechBossAny) AddBoss(NPCID.TheDestroyer, 140);
                            if (NPC.downedPlantBoss) AddBoss(NPCID.Plantera, 180);
                            if (NPC.downedGolemBoss) AddBoss(NPCID.Golem, 200);
                            if (NPC.downedFishron) AddBoss(NPCID.DukeFishron, 250);
                            if (NPC.downedEmpressOfLight) AddBoss(NPCID.HallowBoss, 260);
                            if (NPC.downedMoonlord) AddBoss(NPCID.MoonLordHead, 350);

                            // 保底
                            if (defeatedBosses.Count == 0) AddBoss(NPCID.BlueSlime, 20);

                            // === 幸运加权随机 ===
                            float roll = Main.rand.NextFloat();
                            roll += Player.luck * 0.5f;
                            if (roll > 0.99f) roll = 0.99f;
                            if (roll < 0) roll = 0;

                            int index = (int)(defeatedBosses.Count * roll);
                            if (index >= defeatedBosses.Count) index = defeatedBosses.Count - 1;

                            int bossID = defeatedBosses[index].Key;
                            int power = defeatedBosses[index].Value;

                            SoundEngine.PlaySound(SoundID.Item113, Player.position);

                            // 伤害计算：基础强度 * 5 * 序列加成
                            // 序列2时伤害再翻倍
                            float mult = GetSequenceMultiplier(currentFoolSequence);
                            if (currentFoolSequence <= 2) mult *= 2;

                            int dmg = (int)(power * 5 * mult);

                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
                                ModContent.ProjectileType<HistoricalBossProjectile>(), dmg, 4f, Player.whoAmI, bossID);

                            Main.NewText($"历史降临: {Lang.GetNPCNameValue(bossID)}", 200, 200, 200);
                        }
                    }
                    else
                    {
                        Main.NewText($"历史投影数量已达上限 ({maxProj})", 150, 150, 150);
                    }
                }

                // 2. 干扰命运 (G键)
                if (LotMKeybinds.Fool_Distort.JustPressed)
                {
                    // 序列2：开启/关闭 命运干扰光环
                    if (currentFoolSequence <= 2)
                    {
                        fateDisturbanceActive = !fateDisturbanceActive;
                        if (fateDisturbanceActive)
                            Main.NewText("命运干扰：开启 (自身幸运UP / 敌人攻击失效)", 200, 100, 255);
                        else
                            Main.NewText("命运干扰：关闭", 150, 150, 150);
                    }
                    // 序列6：单次干扰 (扔硬币)
                    else if (currentFoolSequence <= 6)
                    {
                        if (distortCooldown <= 0 && TryConsumeSpirituality(30))
                        {
                            SoundEngine.PlaySound(SoundID.Item18, Player.position);
                            distortCooldown = 600;

                            foreach (NPC npc in Main.ActiveNPCs)
                            {
                                if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 600f)
                                {
                                    npc.AddBuff(BuffID.Confused, 300);
                                    npc.AddBuff(BuffID.Midas, 600);
                                    npc.damage = (int)(npc.damage * 0.8f);
                                }
                            }
                            Main.NewText("金币在指间翻滚，命运已被干扰。", 255, 215, 0);
                        }
                        else if (distortCooldown > 0) Main.NewText("直觉干扰冷却中...", 150, 150, 150);
                    }
                }

                // 3. 奇迹愿望 (V键) - 序列2 核心技能
                // 逻辑：短按切换，长按1秒实现
                if (currentFoolSequence <= 2)
                {
                    // A. 短按切换愿望
                    if (LotMKeybinds.Fool_Miracle.JustPressed)
                    {
                        selectedWish++;
                        if (selectedWish > 3) selectedWish = 0;

                        string wishName = "";
                        switch (selectedWish)
                        {
                            case 0: wishName = "奇迹：生命复苏 (Full Heal)"; break;
                            case 1: wishName = "奇迹：毁灭天灾 (Damage)"; break;
                            case 2: wishName = "奇迹：传送 (Teleport)"; break;
                            case 3: wishName = "奇迹：昼夜更替 (Time)"; break;
                        }
                        Main.NewText($"当前愿望: {wishName} (长按以实现)", 255, 215, 0);
                        wishCastTimer = 0; // 切换时重置计时
                    }

                    // B. 长按实现愿望
                    if (LotMKeybinds.Fool_Miracle.Current)
                    {
                        wishCastTimer++;

                        // 视觉提示：蓄力中
                        if (wishCastTimer > 0 && wishCastTimer < 60 && wishCastTimer % 10 == 0)
                        {
                            Dust.NewDust(Player.position, Player.width, Player.height, DustID.Enchanted_Gold, 0, -2, 0, default, 1f);
                        }

                        // 达到 60 帧 (1秒) -> 触发
                        if (wishCastTimer == 60)
                        {
                            if (miracleCooldown <= 0 && TryConsumeSpirituality(2000)) // 消耗2000灵性
                            {
                                SoundEngine.PlaySound(SoundID.Item29, Player.position);
                                miracleCooldown = 3600; // 60秒冷却

                                switch (selectedWish)
                                {
                                    case 0: // 生命复苏
                                        Player.statLife = Player.statLifeMax2;
                                        Player.HealEffect(Player.statLifeMax2);
                                        for (int i = 0; i < Player.MaxBuffs; i++) if (Main.debuff[Player.buffType[i]]) Player.DelBuff(i); // 清除Debuff
                                        Main.NewText("愿望实现：生命已重置！", 0, 255, 0);
                                        break;

                                    case 1: // 毁灭天灾
                                        Main.NewText("愿望实现：降下毁灭！", 255, 0, 0);
                                        foreach (NPC npc in Main.ActiveNPCs)
                                        {
                                            if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 1500f)
                                            {
                                                // 造成 10000 点真实伤害
                                                Player.ApplyDamageToNPC(npc, 10000, 0, 0, false);
                                                // 视觉特效：落雷
                                                Projectile.NewProjectile(Player.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileID.Electrosphere, 1000, 0, Player.whoAmI);
                                            }
                                        }
                                        break;

                                    case 2: // 传送 (随机传送到地表某处，或者传送到鼠标位置)
                                            // 这里改为传送到鼠标位置比较实用
                                        Player.Teleport(Main.MouseWorld, 1);
                                        Main.NewText("愿望实现：空间跨越！", 0, 255, 255);
                                        break;

                                    case 3: // 昼夜更替
                                        if (Main.dayTime)
                                        {
                                            Main.time = 0;
                                            Main.dayTime = false;
                                            Main.NewText("愿望实现：黑夜降临。", 150, 150, 150);
                                        }
                                        else
                                        {
                                            Main.time = 0;
                                            Main.dayTime = true;
                                            Main.NewText("愿望实现：晨曦到来。", 255, 255, 0);
                                        }
                                        break;
                                }
                            }
                            else if (miracleCooldown > 0)
                            {
                                Main.NewText($"奇迹需要积攒力量... ({miracleCooldown / 60}s)", 150, 150, 150);
                            }
                            else
                            {
                                Main.NewText("灵性不足以支撑奇迹！", 255, 50, 50);
                            }
                        }
                    }
                    else
                    {
                        // 松开按键，重置计时
                        wishCastTimer = 0;
                    }
                }
                // 序列6: 无面伪装 (V键) - 如果不是序列2，V键作为无面伪装
                else if (LotMKeybinds.Fool_Faceless.JustPressed && currentFoolSequence <= 6)
                {
                    isFacelessActive = !isFacelessActive;
                    if (isFacelessActive)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, Player.position);
                        Main.NewText("你改变了容貌与气质...", 200, 200, 200);
                    }
                    else
                    {
                        Main.NewText("恢复原貌。", 200, 200, 200);
                    }
                }
                if (currentFoolSequence <= 1)
                {
                    // 1. 神性属性
                    Player.statLifeMax2 += 2000;
                    Player.statDefense += 100;
                    Player.GetDamage(DamageClass.Generic) += 1.0f * foolMult; // 伤害翻倍
                    Player.statManaMax2 += (int)(500000 * foolMult); // 半神灵性

                    // 2. 诡秘之境 (被动光环)
                    // 减速敌人，转化掉落物
                    if (Main.GameUpdateCount % 10 == 0)
                    {
                        ProcessRealmOfMysteries();
                    }

                    // 3. 嫁接：攻击增益
                    if (graftingMode == 2) // 攻击嫁接
                    {
                        Player.GetCritChance(DamageClass.Generic) += 100; // 必定暴击
                        Player.GetArmorPenetration(DamageClass.Generic) += 9999; // 真实伤害 (穿透所有护甲)
                    }
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
            if (isArmyOfOne){ if (currentHunterSequence <= 4 && TryConsumeSpirituality(5.0f, true)){int bonusMinions = 5;  if (currentHunterSequence <= 1) bonusMinions = 40;else if (currentHunterSequence <= 2) bonusMinions = 20;else if (currentHunterSequence <= 3) bonusMinions = 10;  Player.maxMinions += bonusMinions; } else{ isArmyOfOne = false;} }

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
            // 愚者干扰命运：光环效果
            if (currentFoolSequence <= 2 && fateDisturbanceActive)
            {
                // 特效圈
                if (Main.GameUpdateCount % 20 == 0)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 pos = Player.Center + Main.rand.NextVector2Circular(600, 600);
                        Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, Vector2.Zero, 150, default, 0.5f);
                        d.noGravity = true;
                    }
                }

                // 削弱敌人
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 600f)
                    {
                        // 命运错乱：伤害降低，且大概率攻击落空
                        npc.damage = (int)(npc.defDamage * 0.5f);
                        npc.AddBuff(BuffID.Confused, 2);
                        npc.AddBuff(BuffID.Midas, 2); // 掉落增加 (好运)
                    }
                }
            }

            if (isFireEnchanted) { if (currentHunterSequence <= 6 && TryConsumeSpirituality(0.16f, true)) { } else isFireEnchanted = false; }
            if (isFlameCloakActive) { if (currentHunterSequence <= 7 && TryConsumeSpirituality(1.0f, true)) { Player.buffImmune[BuffID.Chilled] = true; } else isFlameCloakActive = false; }
            if (fireTeleportCooldown > 0) fireTeleportCooldown--;
            if (isMercuryForm) { if (!TryConsumeSpirituality(20.0f, true)) isMercuryForm = false; else { Player.moveSpeed += 2.0f; Player.invis = true; Rectangle myRect = Player.getRect(); myRect.Inflate(10, 10); foreach (NPC npc in Main.npc) { if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(myRect)) { if (npc.immune[Player.whoAmI] == 0) { int damage = (int)((Player.GetDamage(DamageClass.Melee).ApplyTo(50) * 5f) * giantMult); Player.ApplyDamageToNPC(npc, damage, 10f, Player.direction, false); npc.immune[Player.whoAmI] = 10; npc.AddBuff(BuffID.Slow, 300); npc.AddBuff(BuffID.Frostburn, 300); } } } } }
            if (currentSequence <= 3 && !isMercuryForm) { if (Player.velocity.Length() < 0.1f) { stealthTimer++; if (stealthTimer > 60) { Player.invis = true; Player.aggro -= 1000; } } else { stealthTimer = 0; } }
            if (isGuardianStance) { Player.velocity.X = 0; Player.statDefense += 80; Player.noKnockback = true; }
            if (glacierCooldown > 0) glacierCooldown--;
        }

        // 6. 攻击
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { ApplyHitEffects(target); CheckRitualKill(target); CheckExecution(target); }
        private void ApplyHitEffects(NPC target) { if (currentSequence <= 4) target.AddBuff(BuffID.Ichor, 300); if (currentHunterSequence <= 7) target.AddBuff(BuffID.OnFire, 300); if (isCalamityGiant) target.AddBuff(BuffID.Electrified, 300); if (currentHunterSequence <= 1) target.AddBuff(ModContent.BuffType<ConquerorWill>(), 600); if (currentMoonSequence <= 7 && (Player.HeldItem.DamageType == DamageClass.Melee || Player.HeldItem.DamageType == DamageClass.SummonMeleeSpeed || Player.HeldItem.DamageType == DamageClass.Summon)) { target.AddBuff(BuffID.Ichor, 300); if (Main.rand.NextBool(3)) target.AddBuff(BuffID.Poisoned, 300); } }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f; }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f; }
        private void CheckExecution(NPC target) { if (currentHunterSequence <= 5 && !target.boss && target.life < target.lifeMax * 0.2f) target.SimpleStrikeNPC(9999, 0); }
        private void CheckRitualKill(NPC target) { if (target.life <= 0) { if (currentSequence == 5 && demonHunterRitualProgress < DEMON_HUNTER_RITUAL_TARGET) { if (target.type == NPCID.RedDevil) { demonHunterRitualProgress++; } } } }
        public override void PostHurt(Player.HurtInfo info) { if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken) { dawnArmorCurrentHP -= info.Damage; if (dawnArmorCurrentHP <= 0) { dawnArmorCurrentHP = 0; dawnArmorActive = false; dawnArmorBroken = true; dawnArmorCooldownTimer = DAWN_ARMOR_COOLDOWN_MAX; Main.NewText("铠甲已重铸", 100, 255, 100); } } }
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

            if (currentFoolSequence <= 7 && Main.rand.NextFloat() < paperChance)
            {
                Player.SetImmuneTimeForAllTypes(90); // 1.5秒无敌

                // 特效：碎纸屑 (修复了 DustID.Paper 报错)
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.Confetti, 0, 0, 0, default, 1.2f);
                }

                // 瞬移一小段距离 (模拟替身换位)
                Player.position += Main.rand.NextVector2Circular(100, 100);

                // 【序列6特权：反占卜】清除所有 Debuff
                if (currentFoolSequence <= 6)
                {
                    for (int i = 0; i < Player.MaxBuffs; i++)
                    {
                        if (Player.buffType[i] > 0 && Main.debuff[Player.buffType[i]])
                        {
                            Player.DelBuff(i);
                            i--; // 索引回退，防止跳过
                        }
                    }
                    CombatText.NewText(Player.getRect(), Color.Gray, "反占卜!", true);
                }
                else
                {
                    CombatText.NewText(Player.getRect(), Color.LightYellow, "纸人替身!", true);
                }

                return true;
            }

            // 3. 序列8 小丑 (愚者途径) - 10% 几率直觉闪避
            // (只有当上面的纸人替身没触发时，才会走到这里判定)
            else if (currentFoolSequence <= 8 && Main.rand.NextFloat() < 0.1f)
            {
                Player.SetImmuneTimeForAllTypes(60);
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.Confetti, 0, 0, 0, default, 1.2f);
                }
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
            if (LotMKeybinds.Fool_SpiritVision.JustPressed && currentFoolSequence <= 9)
            {
                isSpiritVisionActive = !isSpiritVisionActive;
                if (isSpiritVisionActive)
                {
                    SoundEngine.PlaySound(SoundID.Item8, Player.position);
                    Main.NewText("灵视开启。", 200, 200, 255);
                }
                else
                {
                    Main.NewText("灵视关闭。", 150, 150, 150);
                }
            }

            if (currentFoolSequence <= 1) // 序列1: 诡秘侍者
            {
                // Shift + V : 奇迹愿望 (保留序列2的能力)
                if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                {
                    HandleMiracleWish();
                }
                // 单按 V : 灵肉转化 (序列1核心)
                else if (LotMKeybinds.Fool_SpiritForm.JustPressed)
                {
                    isSpiritForm = !isSpiritForm;
                    if (isSpiritForm)
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item8, Player.position);
                        Main.NewText("灵体化：物理免疫 / 穿墙 / 极速", 200, 200, 255);
                    }
                    else
                    {
                        Main.NewText("回归血肉之躯", 150, 150, 150);
                    }
                }
            }
            else if (currentFoolSequence <= 2) // 序列2: 奇迹师
            {
                // V : 奇迹愿望
                HandleMiracleWish();
            }
            else if (currentFoolSequence <= 6) // 序列6: 无面人
            {
                // V : 无面伪装
                if (LotMKeybinds.Fool_Faceless.JustPressed) // 注意这里用了 Faceless 的绑定，如果 V 键都被绑定为同一个键位，检测哪个都行
                {
                    isFacelessActive = !isFacelessActive;
                    if (isFacelessActive)
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item8, Player.position);
                        Main.NewText("你改变了容貌...", 200, 200, 200);
                    }
                    else
                    {
                        Main.NewText("恢复原貌。", 200, 200, 200);
                    }
                }
            }

            // ---------------------------------------------------------
            // 2. 嫁接 & 命运干扰 (G键 复合逻辑)
            // ---------------------------------------------------------
            if (currentFoolSequence <= 1) // 序列1
            {
                // Shift + G : 命运干扰光环 (保留序列2能力)
                if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) && LotMKeybinds.Fool_Grafting.JustPressed)
                {
                    fateDisturbanceActive = !fateDisturbanceActive;
                    PrintFateState();
                }
                // 单按 G : 嫁接切换
                else if (LotMKeybinds.Fool_Grafting.JustPressed)
                {
                    graftingMode++;
                    if (graftingMode > 2) graftingMode = 0;
                    string modeName = graftingMode == 0 ? "关闭" : (graftingMode == 1 ? "空间嫁接 (反弹)" : "概念嫁接 (必杀)");
                    Main.NewText($"嫁接模式: {modeName}", 100, 100, 255);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.position);
                }
            }
            else if (currentFoolSequence <= 2) // 序列2
            {
                // G : 命运干扰光环
                if (LotMKeybinds.Fool_Distort.JustPressed)
                {
                    fateDisturbanceActive = !fateDisturbanceActive;
                    PrintFateState();
                }
            }
            else if (currentFoolSequence <= 6) // 序列6
            {
                // G : 命运干扰 (单次)
                if (LotMKeybinds.Fool_Distort.JustPressed)
                {
                    if (distortCooldown <= 0 && TryConsumeSpirituality(30))
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item18, Player.position);
                        distortCooldown = 600;
                        foreach (NPC npc in Main.ActiveNPCs)
                        {
                            if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 600f)
                            {
                                npc.AddBuff(BuffID.Confused, 300);
                                npc.AddBuff(BuffID.Midas, 600);
                                npc.damage = (int)(npc.damage * 0.8f);
                            }
                        }
                        Main.NewText("金币翻滚，命运已被干扰。", 255, 215, 0);
                    }
                }
            }

            // ---------------------------------------------------------
            // 3. 历史投影 (Y键)
            // ---------------------------------------------------------
            if (currentFoolSequence <= 3)
            {
                // Shift + Y : 历史场景 (序列2特权)
                if (currentFoolSequence <= 2 && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) && LotMKeybinds.Fool_History.JustPressed)
                {
                    // 检查是否存在
                    bool exists = false;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile p = Main.projectile[i];
                        if (p.active && p.owner == Player.whoAmI && p.type == ModContent.ProjectileType<Projectiles.HistoricalSceneProjectile>())
                        {
                            p.Kill(); exists = true;
                            Main.NewText("历史场景已消散。", 150, 150, 150);
                            break;
                        }
                    }
                    if (!exists && TryConsumeSpirituality(1000))
                    {
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.HistoricalSceneProjectile>(), 0, 0, Player.whoAmI);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.position);
                        Main.NewText("历史场景降临...", 200, 200, 255);
                    }
                }
                // 单按 Y : Boss 投影 (序列3基础)
                else if (LotMKeybinds.Fool_History.JustPressed)
                {
                    int currentProjections = 0;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].owner == Player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<Projectiles.HistoricalBossProjectile>())
                            currentProjections++;
                    }

                    // 序列2上限9个，序列3上限1个 (根据您最后的要求，这里改为1)
                    // 您之前的要求是：同一时间只能存在一个Boss帮助打怪
                    // 如果您想改回9个，把下面的 1 改成 (currentFoolSequence <= 2 ? 9 : 1)
                    int maxProj = 1;

                    if (currentProjections < maxProj)
                    {
                        if (TryConsumeSpirituality(500))
                        {
                            // 构建 Boss 池
                            System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>> defeatedBosses = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>>();
                            void AddBoss(int id, int power) { defeatedBosses.Add(new System.Collections.Generic.KeyValuePair<int, int>(id, power)); }

                            if (NPC.downedSlimeKing) AddBoss(NPCID.KingSlime, 40);
                            if (NPC.downedBoss1) AddBoss(NPCID.EyeofCthulhu, 50);
                            if (NPC.downedBoss2) AddBoss(NPCID.EaterofWorldsHead, 60);
                            if (NPC.downedQueenBee) AddBoss(NPCID.QueenBee, 70);
                            if (NPC.downedBoss3) AddBoss(NPCID.SkeletronHead, 80);
                            if (Main.hardMode) AddBoss(NPCID.WallofFlesh, 100);
                            if (NPC.downedQueenSlime) AddBoss(NPCID.QueenSlimeBoss, 120);
                            if (NPC.downedMechBossAny) AddBoss(NPCID.TheDestroyer, 140);
                            if (NPC.downedPlantBoss) AddBoss(NPCID.Plantera, 180);
                            if (NPC.downedGolemBoss) AddBoss(NPCID.Golem, 200);
                            if (NPC.downedFishron) AddBoss(NPCID.DukeFishron, 250);
                            if (NPC.downedEmpressOfLight) AddBoss(NPCID.HallowBoss, 260);
                            if (NPC.downedMoonlord) AddBoss(NPCID.MoonLordHead, 350);
                            if (defeatedBosses.Count == 0) AddBoss(NPCID.BlueSlime, 20);

                            // 幸运随机
                            float roll = Main.rand.NextFloat() + Player.luck * 0.5f;
                            if (roll > 0.99f) roll = 0.99f; if (roll < 0) roll = 0;
                            int index = (int)(defeatedBosses.Count * roll);
                            int bossID = defeatedBosses[index].Key;
                            int power = defeatedBosses[index].Value;

                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item113, Player.position);

                            // 伤害计算 (序列2翻倍)
                            float mult = GetSequenceMultiplier(currentFoolSequence);
                            if (currentFoolSequence <= 2) mult *= 2;
                            int dmg = (int)(power * 5 * mult);

                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.HistoricalBossProjectile>(), dmg, 4f, Player.whoAmI, bossID);
                            Main.NewText($"历史投影: {Lang.GetNPCNameValue(bossID)}", 200, 200, 200);
                        }
                    }
                    else
                    {
                        Main.NewText($"只能维持 {maxProj} 个历史投影。", 150, 150, 150);
                    }
                }
            }

            // ---------------------------------------------------------
            // 4. 其他技能
            // ---------------------------------------------------------

            // 昨日重现 (U键) - 序列3
            if (LotMKeybinds.Fool_Borrow.JustPressed && currentFoolSequence <= 3)
            {
                if (!Player.HasBuff(ModContent.BuffType<Buffs.YesterdayBuff>()))
                {
                    if (borrowUsesDaily < 10 && TryConsumeSpirituality(100))
                    {
                        borrowUsesDaily++;
                        Player.AddBuff(ModContent.BuffType<Buffs.YesterdayBuff>(), 18000);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.position);
                        Player.statLife = Player.statLifeMax2; Player.statMana = Player.statManaMax2; Player.HealEffect(Player.statLifeMax2);
                        Main.NewText($"昨日重现！(剩余: {10 - borrowUsesDaily})", 0, 255, 255);
                    }
                    else Main.NewText("次数耗尽。", 150, 150, 150);
                }
                else Main.NewText("力量正在涌动...", 150, 150, 150);
            }

            // 秘偶互换 (T键) - 序列4
            if (LotMKeybinds.Fool_Swap.JustPressed && currentFoolSequence <= 4)
            {
                if (swapCooldown <= 0 && TryConsumeSpirituality(50))
                {
                    // 寻找最近的 历史投影 或 普通秘偶
                    Projectile closest = null;
                    float minDst = 99999f;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile p = Main.projectile[i];
                        if (p.active && p.owner == Player.whoAmI && (p.type == ModContent.ProjectileType<Projectiles.MarionetteMinion>() || p.type == ModContent.ProjectileType<Projectiles.HistoricalBossProjectile>()))
                        {
                            float dst = Vector2.Distance(Player.Center, p.Center);
                            if (dst < minDst) { minDst = dst; closest = p; }
                        }
                    }
                    if (closest != null)
                    {
                        Vector2 temp = Player.Center; Player.Teleport(closest.Center, 1); closest.Center = temp;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item6, Player.position);
                        swapCooldown = 60; Main.NewText("位置互换", 200, 200, 255);
                    }
                    else Main.NewText("无秘偶可换。", 150, 150, 150);
                }
            }

            // 控灵 (R键) - 序列4
            if (LotMKeybinds.Fool_Control.JustPressed && currentFoolSequence <= 4)
            {
                if (spiritControlCooldown <= 0 && TryConsumeSpirituality(100))
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item103, Player.position);
                    spiritControlCooldown = 1200;
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 1000f)
                        {
                            npc.AddBuff(BuffID.Stoned, 180); npc.AddBuff(BuffID.Confused, 300);
                        }
                    }
                    Main.NewText("灵体震慑！", 148, 0, 211);
                }
                else if (spiritControlCooldown > 0) Main.NewText($"冷却中: {spiritControlCooldown / 60}s", 150, 150, 150);
            }
            // ==========================================
            // 技能：操纵灵体之线 (按住按键持续施法)
            // ==========================================
            if (LotMKeybinds.Fool_Threads.Current && currentFoolSequence <= 5)
            {
                // 1. 寻找目标
                if (spiritThreadTargetIndex == -1)
                {
                    float minDesc = 400f;
                    int foundIndex = -1;

                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        // 【核心修改】允许锁定敌人 OR 城镇NPC (仅限序列2以上为了仪式)
                        // CanBeChasedBy 默认排除城镇NPC，所以我们要手动加上 npc.townNPC
                        bool isValidTarget = npc.CanBeChasedBy() || (npc.townNPC && currentFoolSequence <= 2);

                        if (isValidTarget && npc.Distance(Main.MouseWorld) < minDesc)
                        {
                            minDesc = npc.Distance(Main.MouseWorld);
                            foundIndex = npc.whoAmI;
                        }
                    }

                    if (foundIndex != -1)
                    {
                        spiritThreadTargetIndex = foundIndex;
                        Main.NewText("已抓住灵体之线...", 180, 80, 255);
                    }
                }

                // 2. 持续控制
                if (spiritThreadTargetIndex != -1 && TryConsumeSpirituality(1.0f))
                {
                    NPC target = Main.npc[spiritThreadTargetIndex];

                    // 距离检测
                    if (!target.active || (target.life <= 0 && !target.townNPC) || target.Distance(Player.Center) > 1000f)
                    {
                        spiritThreadTargetIndex = -1;
                        spiritThreadTimer = 0;
                        return;
                    }

                    // 施加控制特效
                    target.AddBuff(ModContent.BuffType<SpiritControlDebuff>(), 2);

                    // 连线特效
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 mid = Vector2.Lerp(Player.Center, target.Center, Main.rand.NextFloat());
                        Dust.NewDust(mid, 0, 0, DustID.PurpleCrystalShard, 0, 0, 0, default, 1f);
                    }

                    // 速度计算
                    int speed = (currentFoolSequence <= 4) ? 5 : 1;
                    spiritThreadTimer += speed;

                    // 进度提示
                    if (Main.GameUpdateCount % 60 == 0)
                    {
                        float progress = (float)spiritThreadTimer / CONTROL_TIME_REQUIRED;
                        CombatText.NewText(target.getRect(), Color.Purple, $"{(int)(progress * 100)}%", false, false);
                    }

                    // 3. 转化完成
                    if (spiritThreadTimer >= CONTROL_TIME_REQUIRED)
                    {
                        // 【核心修改：分支逻辑】

                        // 情况A: 如果是城镇 NPC (用于仪式)
                        if (target.townNPC)
                        {
                            // 施加永久(或极长时间)的秘偶标记 Buff
                            target.AddBuff(ModContent.BuffType<MarionetteTownNPCBuff>(), 36000); // 10分钟

                            Main.NewText($"{target.FullName} 已转化为秘偶！", 148, 0, 211);
                            SoundEngine.PlaySound(SoundID.NPCDeath6, target.Center);

                            // 稍微回点血防止直接死掉
                            target.life = target.lifeMax;
                        }
                        // 情况B: 如果是敌人 (正常战斗)
                        else
                        {
                            int damage = 99999;
                            if (target.boss) damage = 2000;
                            Player.ApplyDamageToNPC(target, damage, 0, 0, false);

                            int minionDamage = (int)(100 * GetSequenceMultiplier(currentFoolSequence));
                            Projectile.NewProjectile(Player.GetSource_FromThis(), target.Center, Vector2.Zero,
                                ModContent.ProjectileType<MarionetteMinion>(), minionDamage, 2f, Player.whoAmI);

                            Main.NewText("转化成功！目标已成为秘偶。", 200, 100, 255);
                            SoundEngine.PlaySound(SoundID.NPCDeath6, target.Center);
                        }

                        // 重置
                        spiritThreadTargetIndex = -1;
                        spiritThreadTimer = 0;
                    }
                }
            }
            else
            {
                spiritThreadTargetIndex = -1;
                spiritThreadTimer = 0;
            }
            if (LotMKeybinds.Fool_Divination.JustPressed && currentFoolSequence <= 9)
            {
                if (divinationCooldown <= 0 && TryConsumeSpirituality(10))
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, Player.position);
                    divinationCooldown = 600;
                    int maxRoll = (currentFoolSequence <= 4) ? 6 : 3;
                    int roll = Main.rand.Next(maxRoll);
                    string res = "未知";
                    if (roll == 0) { res = "厄运"; Player.AddBuff(BuffID.WaterCandle, 3600); }
                    else if (roll == 1) { res = "财富"; Player.AddBuff(BuffID.Spelunker, 3600); }
                    else if (roll == 2) { res = "启示"; Player.AddBuff(BuffID.Clairvoyance, 3600); }
                    else if (roll == 3) { res = "战斗"; Player.AddBuff(BuffID.Wrath, 3600); }
                    else if (roll == 4) { res = "生存"; Player.AddBuff(BuffID.Ironskin, 3600); }
                    else if (roll == 5) { res = "眷顾"; Player.AddBuff(BuffID.Lucky, 3600); }
                    Main.NewText($"占卜结果: {res}", 200, 200, 255);
                }
            }

            // 火焰跳跃 (F键) - 序列7
            if (LotMKeybinds.Fool_FlameJump.JustPressed && currentFoolSequence <= 7)
            {
                float range = (currentFoolSequence <= 3) ? 20000f : ((currentFoolSequence <= 6) ? 650f : 500f);
                if (Player.Distance(Main.MouseWorld) < range)
                {
                    if (flameJumpCooldown <= 0 && TryConsumeSpirituality(15))
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, Player.position);
                        Player.Teleport(Main.MouseWorld, 1);
                        flameJumpCooldown = 60;
                    }
                }
            }
        }

        // 辅助方法：处理奇迹愿望
        private void HandleMiracleWish()
        {
            // 1. 切换 (短按)
            if (LotMKeybinds.Fool_Miracle.JustPressed || LotMKeybinds.Fool_SpiritForm.JustPressed) // 兼容两种按键触发
            {
                selectedWish++; if (selectedWish > 3) selectedWish = 0;
                string n = selectedWish == 0 ? "生命" : selectedWish == 1 ? "毁灭" : selectedWish == 2 ? "传送" : "昼夜";
                Main.NewText($"愿望: {n} (长按实现)", 255, 215, 0);
                wishCastTimer = 0;
            }
            // 2. 实现 (长按)
            if (LotMKeybinds.Fool_Miracle.Current || LotMKeybinds.Fool_SpiritForm.Current)
            {
                wishCastTimer++;
                if (wishCastTimer == 60)
                {
                    if (miracleCooldown <= 0 && TryConsumeSpirituality(2000))
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, Player.position);
                        miracleCooldown = 3600;
                        if (selectedWish == 0) { Player.statLife = Player.statLifeMax2; Player.HealEffect(Player.statLifeMax2); Main.NewText("生命复苏!", 0, 255, 0); }
                        else if (selectedWish == 1)
                        {
                            foreach (NPC n in Main.ActiveNPCs) if (!n.friendly && !n.dontTakeDamage && n.Distance(Player.Center) < 1500f)
                                {
                                    Player.ApplyDamageToNPC(n, 10000, 0, 0, false);
                                    Projectile.NewProjectile(Player.GetSource_FromThis(), n.Center, Vector2.Zero, ProjectileID.Electrosphere, 1000, 0, Player.whoAmI);
                                }
                            Main.NewText("毁灭天灾!", 255, 0, 0);
                        }
                        else if (selectedWish == 2) { Player.Teleport(Main.MouseWorld, 1); Main.NewText("空间跨越!", 0, 255, 255); }
                        else if (selectedWish == 3) { Main.time = 0; Main.dayTime = !Main.dayTime; Main.NewText("昼夜更替。", 200, 200, 200); }
                    }
                    else if (miracleCooldown > 0) Main.NewText($"冷却: {miracleCooldown / 60}s", 150, 150, 150);
                }
            }
            else wishCastTimer = 0;
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
        public override bool CanUseItem(Item item) { if (isFireForm || isMercuryForm || isGuardianStance || isMoonlightized || isBatSwarm) return false; return base.CanUseItem(item); }
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

            spiritualityMax = max;
        }
        private void HandleSpiritualityRegen(){spiritualityRegenTimer++;if (spiritualityRegenTimer >= 60){spiritualityRegenTimer = 0;float r = 2f + (spiritualityMax * 0.01f);if (currentMoonSequence <= 2) r += (spiritualityMax * 0.04f); spiritualityCurrent += r;if (spiritualityCurrent > spiritualityMax){spiritualityCurrent = spiritualityMax;}}}
        private void HandleDawnArmorLogic() { if (dawnArmorBroken) { dawnArmorCooldownTimer--; if (dawnArmorCooldownTimer <= 0) { dawnArmorBroken = false; dawnArmorCurrentHP = MaxDawnArmorHP; Main.NewText("铠甲已重铸", 100, 255, 100); } } else if (!dawnArmorActive && dawnArmorCurrentHP < MaxDawnArmorHP && Main.GameUpdateCount % 2 == 0) dawnArmorCurrentHP++; }
        private void SpawnVisualDust() { for (int i = 0; i < 40; i++) Dust.NewDustPerfect(Player.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(5f, 5f), 100, default, 2.0f).noGravity = true; }
        private void CheckConquerorRitual() { if (currentHunterSequence == 2 && !conquerorRitualComplete && ConquerorSpawnSystem.StopSpawning) { bool enemyExists = false; for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.townNPC && npc.lifeMax > 5 && !npc.dontTakeDamage) { enemyExists = true; break; } } if (!enemyExists) { conquerorRitualComplete = true; Main.NewText("这片大陆已无敌手... 征服的意志已达成！", 255, 0, 0); SoundEngine.PlaySound(SoundID.Roar, Player.position); } } }
        private void ProcessRealmOfMysteries()
        {
            // A. 压制敌人 (灵体之线僵硬)
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < realmRange)
                {
                    npc.AddBuff(BuffID.Slow, 60); // 强力减速
                    npc.AddBuff(ModContent.BuffType<Buffs.SpiritControlDebuff>(), 60); // 变色/僵硬

                    // 几率产生幻觉 (Confusion)
                    if (Main.rand.NextBool(50)) npc.AddBuff(BuffID.Confused, 120);
                }
            }

            // B. 再生：吞噬掉落物转化为状态
            foreach (Item item in Main.item)
            {
                if (item.active && item.Distance(Player.Center) < realmRange)
                {
                    // 吸取掉落物
                    item.velocity = (Player.Center - item.Center).SafeNormalize(Vector2.Zero) * 15f;

                    if (item.Distance(Player.Center) < 50f)
                    {
                        // "吃掉"物品
                        int value = item.value * item.stack;
                        if (value > 0)
                        {
                            // 转化为血量
                            int heal = Math.Max(1, value / 1000);
                            Player.statLife += heal;
                            Player.HealEffect(heal);

                            // 转化为灵性 (可选)
                            // spiritualityCurrent += heal;

                            item.active = false; // 物品消失

                            // 特效
                            for (int k = 0; k < 10; k++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.SpectreStaff, 0, 0, 0, default, 1.5f);
                        }
                    }
                }
            }
        }
    }

    public class ApothecaryCrafting : Terraria.ModLoader.GlobalItem { public override void OnCreated(Terraria.Item item, ItemCreationContext context) { if (context is RecipeItemCreationContext) { Player p = Main.LocalPlayer; if (p != null && p.active && p.GetModPlayer<LotMPlayer>().currentMoonSequence <= 9) { bool isP = item.consumable && (item.buffType > 0 || item.healLife > 0 || item.healMana > 0); if (isP) p.QuickSpawnItem(item.GetSource_FromThis(), item.type, item.stack); } } } }
}