using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures; // 必须引用
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using zhashi.Content.Projectiles;
using zhashi.Content.Items.Weapons;
using zhashi.Content.Buffs;
using zhashi.System;

namespace zhashi.Content
{
    public class LotMPlayer : ModPlayer
    {
        // ===================================================
        // 1. 核心变量定义
        // ===================================================
        public int currentSequence = 10;       // 巨人/战士途径 (9-2)
        public int currentHunterSequence = 10; // 猎人途径 (9-1)
        public int currentMoonSequence = 10;   // 月亮途径 (9-2)

        public bool IsBeyonder => currentSequence < 10 || currentHunterSequence < 10 || currentMoonSequence < 10;

        // 灵性系统
        public float spiritualityCurrent = 100;
        public int spiritualityMax = 100;
        private int spiritualityRegenTimer = 0;

        // --- 巨人途径技能状态 ---
        public bool dawnArmorActive = false;
        public bool dawnArmorBroken = false;
        public int dawnArmorCurrentHP = 250;
        // 动态计算铠甲最大耐久
        public int MaxDawnArmorHP => (int)(250 * GetSequenceMultiplier(currentSequence));
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
        public bool isVampireWings = false;    // 吸血鬼：黑暗之翼
        public int abyssShackleCooldown = 0;   // 吸血鬼：深渊枷锁
        public int elixirCooldown = 0;         // 魔药教授：生命灵液
        public bool isFullMoonActive = false;  // 深红学者：满月领域
        public bool isMoonlightized = false;   // 深红学者：月光化
        public bool isBatSwarm = false;        // 巫王：蝙蝠化身
        public int paperFigurineCooldown = 0;  // 巫王：月亮纸人
        public int darknessGazeCooldown = 0;   // 巫王：黑暗凝视
        public int summonGateCooldown = 0;     // 召唤大师：召唤之门

        // 序列2：创生者
        public bool isCreationDomain = false;  // 创生领域
        public int purifyCooldown = 0;         // 净化冷却

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
            tag["Spirituality"] = spiritualityCurrent;
            tag["GuardianRitual"] = guardianRitualProgress;
            tag["DemonHunterRitual"] = demonHunterRitualProgress;
            tag["IronBloodRitual"] = ironBloodRitualProgress;
            tag["WeatherRitualComplete"] = weatherRitualComplete;
            tag["ConquerorRitual"] = conquerorRitualComplete;
            tag["ResurrectionCooldown"] = twilightResurrectionCooldown;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("CurrentSequence")) currentSequence = tag.GetInt("CurrentSequence");
            if (tag.ContainsKey("HunterSequence")) currentHunterSequence = tag.GetInt("HunterSequence");
            if (tag.ContainsKey("MoonSequence")) currentMoonSequence = tag.GetInt("MoonSequence");

            if (tag.ContainsKey("Spirituality")) spiritualityCurrent = tag.GetFloat("Spirituality");
            if (tag.ContainsKey("GuardianRitual")) guardianRitualProgress = tag.GetInt("GuardianRitual");
            if (tag.ContainsKey("DemonHunterRitual")) demonHunterRitualProgress = tag.GetInt("DemonHunterRitual");
            if (tag.ContainsKey("IronBloodRitual")) ironBloodRitualProgress = tag.GetInt("IronBloodRitual");
            if (tag.ContainsKey("WeatherRitualComplete")) weatherRitualComplete = tag.GetBool("WeatherRitualComplete");
            if (tag.ContainsKey("ConquerorRitual")) conquerorRitualComplete = tag.GetBool("ConquerorRitual");
            if (tag.ContainsKey("ResurrectionCooldown")) twilightResurrectionCooldown = tag.GetInt("ResurrectionCooldown");
        }

        // ===================================================
        // 3. 属性重置与核心逻辑
        // ===================================================
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
        // 4. 数值加成系统 (含移动/跳跃成长)
        // ===================================================
        private void ApplySequenceStats()
        {
            // 获取倍率
            float giantMult = GetSequenceMultiplier(currentSequence);
            float hunterMult = GetSequenceMultiplier(currentHunterSequence);
            float moonMult = GetSequenceMultiplier(currentMoonSequence);

            // --- 通用成长：移动速度 & 跳跃高度 ---
            // 取各途径最高倍率
            float maxMult = Math.Max(giantMult, Math.Max(hunterMult, moonMult));
            if (maxMult > 1f)
            {
                // 移动速度成长
                Player.moveSpeed += 0.1f * maxMult;
                Player.maxRunSpeed += 1f * maxMult;

                // 【关键修复】跳跃高度成长 (序列越高跳得越高)
                // 基础加成 + (序列加成)
                Player.jumpSpeedBoost += 1.0f * (maxMult - 1f);
            }

            // --- 巨人/战士途径 ---
            if (currentSequence <= 9) { Player.statDefense += (int)(5 * giantMult); Player.GetDamage(DamageClass.Melee) += 0.10f * giantMult; Player.GetCritChance(DamageClass.Melee) += 5; }
            if (currentSequence <= 8) { Player.GetAttackSpeed(DamageClass.Melee) += 0.10f; Player.endurance += 0.05f; Player.noKnockback = true; }
            if (currentSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.10f; Player.GetCritChance(DamageClass.Generic) += 5; Player.GetArmorPenetration(DamageClass.Generic) += 5 * giantMult; }
            if (currentSequence <= 6)
            {
                Lighting.AddLight(Player.Center, 1.5f, 1.5f, 1.5f);
                Player.statDefense += 10;
                Player.lifeRegen += (int)(2 * giantMult);
                if (dawnArmorActive && !dawnArmorBroken)
                {
                    Player.statDefense += (int)(30 * giantMult);
                    Player.endurance += 0.1f * giantMult;
                }
            }
            if (currentSequence <= 5)
            {
                Player.statDefense += 15;
                Player.endurance += 0.05f;
                Player.buffImmune[BuffID.Confused] = true;
                if (isGuardianStance)
                {
                    Player.statDefense += (int)(80 * giantMult);
                    Player.endurance += 0.2f * giantMult;
                }
            }
            if (currentSequence <= 4) { Player.statLifeMax2 += (int)(100 * giantMult); Player.GetDamage(DamageClass.Generic) += 0.15f; Player.GetCritChance(DamageClass.Generic) += 10; Player.nightVision = true; Player.detectCreature = true; Player.buffImmune[BuffID.CursedInferno] = true; Player.buffImmune[BuffID.ShadowFlame] = true; }
            if (currentSequence <= 3) { Player.statDefense += 20; Player.lifeRegen += 5; Player.GetAttackSpeed(DamageClass.Melee) += 0.15f; Player.blackBelt = true; }
            if (currentSequence <= 2) { Player.statLifeMax2 += 400; Player.statDefense += 30; Player.endurance += 0.10f; Player.GetDamage(DamageClass.Generic) += 0.20f; }

            // --- 猎人途径 ---
            if (currentHunterSequence <= 9) { Player.GetDamage(DamageClass.Ranged) += 0.10f; Player.GetDamage(DamageClass.Melee) += 0.05f; Player.detectCreature = true; Player.dangerSense = true; }
            if (currentHunterSequence <= 8) { Player.statDefense += 8; Player.aggro += 300; Player.lifeRegen += 2; }
            if (currentHunterSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.15f * hunterMult; Player.buffImmune[BuffID.OnFire] = true; Player.buffImmune[BuffID.OnFire3] = true; Player.buffImmune[BuffID.Frostburn] = true; Player.resistCold = true; }
            if (currentHunterSequence <= 6) { Player.GetCritChance(DamageClass.Generic) += 10; Player.manaCost -= 0.15f; }
            if (currentHunterSequence <= 5) { Player.GetArmorPenetration(DamageClass.Generic) += 20 * hunterMult; Player.GetCritChance(DamageClass.Generic) += 15; }
            if (currentHunterSequence <= 4) { Player.statDefense += 40; Player.endurance += 0.15f; Player.maxMinions += 2; Player.noKnockback = true; }
            if (currentHunterSequence <= 3) { Player.maxMinions += 3; Player.maxTurrets += 2; Player.GetDamage(DamageClass.Summon) += 0.30f; }
            if (currentHunterSequence <= 2) { Player.statLifeMax2 += 500; Player.statManaMax2 += 200; Player.GetDamage(DamageClass.Generic) += 0.30f; Player.buffImmune[BuffID.WindPushed] = true; }
            if (currentHunterSequence <= 1) { Player.statDefense += 80; Player.endurance += 0.30f; Player.GetDamage(DamageClass.Generic) += 0.80f; Player.GetCritChance(DamageClass.Generic) += 30; Player.aggro += 2000; Player.buffImmune[BuffID.Weak] = true; Player.buffImmune[BuffID.BrokenArmor] = true; Player.buffImmune[BuffID.WitheredArmor] = true; Player.buffImmune[BuffID.WitheredWeapon] = true; }

            // --- 月亮途径 ---
            if (currentMoonSequence <= 9)
            {
                Player.buffImmune[BuffID.Poisoned] = true;
                Player.buffImmune[BuffID.Venom] = true;
                Player.detectCreature = true;
                Player.lifeRegen += (int)(2 * moonMult);
                Player.statLifeMax2 += (int)(20 * moonMult);
            }
            if (currentMoonSequence <= 8)
            {
                Player.statDefense += 5;
                Player.GetDamage(DamageClass.Generic) += 0.10f * moonMult;
                Player.maxMinions += (int)(1 * moonMult);
                // 额外跳跃加成
                Player.jumpSpeedBoost += 1.5f;
                Player.dangerSense = true;
            }
            if (currentMoonSequence <= 7)
            {
                Player.statLifeMax2 += 50;
                Player.lifeRegen += (int)(5 * moonMult);
                Player.noFallDmg = true;
                if (Main.dayTime && Player.ZoneOverworldHeight && Player.behindBackWall == false) { Player.statDefense -= 5; Player.lifeRegen -= 3; Player.GetDamage(DamageClass.Generic) -= 0.1f; }
            }
            if (currentMoonSequence <= 6)
            {
                Player.statManaMax2 += 50;
                Player.GetDamage(DamageClass.Magic) += 0.15f * moonMult;
                Player.pStone = true;
                Player.buffImmune[BuffID.OnFire] = true;
                Player.buffImmune[BuffID.Frostburn] = true;
                Player.buffImmune[BuffID.CursedInferno] = true;
            }
            if (currentMoonSequence <= 5)
            {
                Player.lifeRegen += 10;
                Player.buffImmune[BuffID.Confused] = true;
                Player.buffImmune[BuffID.Darkness] = true;
                Player.buffImmune[BuffID.Silenced] = true;
                Player.buffImmune[BuffID.Blackout] = true;
                if (isFullMoonActive)
                {
                    Player.GetDamage(DamageClass.Magic) += 0.30f * moonMult;
                    Player.statDefense -= 10;
                    Lighting.AddLight(Player.Center, 0.6f, 0.7f, 0.9f);
                    Player.manaRegen += (int)(20 * moonMult);
                    Player.manaRegenDelayBonus += 5;
                }
            }
            if (currentMoonSequence <= 4)
            {
                Player.statLifeMax2 += 200;
                Player.statManaMax2 += 100;
                Player.GetDamage(DamageClass.Magic) += 0.20f * moonMult;
                Player.GetDamage(DamageClass.Summon) += 0.20f * moonMult;
                Player.lifeRegen += 15;
            }
            if (currentMoonSequence <= 3)
            {
                Player.maxMinions += (int)(4 * moonMult);
                Player.GetDamage(DamageClass.Summon) += 0.30f * moonMult;
                Player.GetKnockback(DamageClass.Summon) += 2f; // 1.4.4 API
                Player.statManaMax2 += 200;
                Player.manaCost -= 0.20f;
            }
            if (currentMoonSequence <= 2)
            {
                Player.statLifeMax2 += 1000;
                Player.lifeRegen += 50;
                Player.buffImmune[BuffID.Bleeding] = true;
                Player.buffImmune[BuffID.Poisoned] = true;
                Player.buffImmune[BuffID.Venom] = true;
                Player.buffImmune[BuffID.CursedInferno] = true;
                Player.buffImmune[BuffID.Ichor] = true;
                Player.buffImmune[BuffID.Frozen] = true;

                if (isCreationDomain)
                {
                    Player.lifeRegen += 50;
                    Player.statDefense += 30;
                    Lighting.AddLight(Player.Center, 0.1f, 0.8f, 0.2f);
                    Player.flowerBoots = true; // 花靴效果
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
            if (isBatSwarm) { drawInfo.hideEntirePlayer = true; drawInfo.shadow = 0f; r = 0f; g = 0f; b = 0f; a = 0f; fullBright = false; return; }
            if (isCalamityGiant || isFireForm || isMercuryForm || isMoonlightized) { drawInfo.hideEntirePlayer = true; return; }
            if (currentSequence <= 3 && stealthTimer > 60) { drawInfo.hideEntirePlayer = true; drawInfo.shadow = 0f; a = 0f; return; }
            if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken && !Player.shroomiteStealth) { r = 2.0f; g = 2.0f; b = 2.0f; fullBright = true; if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Player.Center, DustID.Enchanted_Gold, Vector2.Zero, 0, default, 0.8f).noGravity = true; }
        }

        public override void PostUpdateMiscEffects()
        {
            float moonMult = GetSequenceMultiplier(currentMoonSequence);
            float giantMult = GetSequenceMultiplier(currentSequence);
            float hunterMult = GetSequenceMultiplier(currentHunterSequence);

            // 猎人/巨人特效
            if (isCalamityGiant) { if (!TryConsumeSpirituality(100.0f, true)) isCalamityGiant = false; else { Player.statDefense += 200; Player.wingsLogic = 0; Player.wingTime = 9999; Player.gravity = 0f; Player.statLifeMax2 += 2000; Player.invis = true; if (Main.rand.NextBool(2)) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Electric, 0, 0, 0, default, 1.5f); } }
            if (isArmyOfOne) { if (currentHunterSequence <= 4 && TryConsumeSpirituality(5.0f, true)) { Player.maxMinions += 5; } else isArmyOfOne = false; }

            if (isFireForm)
            {
                if (!TryConsumeSpirituality(50.0f, true)) { isFireForm = false; Main.NewText("灵性枯竭！", 255, 50, 50); return; }
                Player.noKnockback = true; Player.wingsLogic = 0; Player.wingTime = 9999; Player.rocketTime = 9999; Player.noItems = true;
                // 征服者冲锋
                if (currentHunterSequence <= 1)
                {
                    Player.moveSpeed += 5.0f; Player.maxRunSpeed += 30f; Player.runAcceleration *= 10f; Player.jumpSpeedBoost += 30f; Player.statDefense += 200; Player.endurance += 0.4f;
                    Vector2 tip = Player.Center + Player.velocity * 2f; for (int i = 0; i < 5; i++) { Vector2 pos = Vector2.Lerp(Player.Center, tip, i / 5f) + Main.rand.NextVector2Circular(10, 10); int d = Dust.NewDust(pos, 0, 0, DustID.Shadowflame, 0, 0, 0, default, 2.5f); Main.dust[d].noGravity = true; Main.dust[d].velocity = -Player.velocity * 0.2f; }
                    Rectangle myRect = Player.getRect(); myRect.Inflate(40, 40);
                    if (Player.velocity.Length() > 5f) { for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(myRect)) { if (npc.immune[Player.whoAmI] <= 0) { int dashDamage = (int)(Player.GetDamage(DamageClass.Melee).ApplyTo(2000) * giantMult); Player.ApplyDamageToNPC(npc, dashDamage, 30f, Player.direction, true); npc.AddBuff(ModContent.BuffType<ConquerorWill>(), 600); npc.immune[Player.whoAmI] = 6; SoundEngine.PlaySound(SoundID.Item74, npc.Center); for (int k = 0; k < 20; k++) Dust.NewDust(npc.position, npc.width, npc.height, DustID.Shadowflame, 0, 0, 0, default, 3f); } } } }
                }
                else { Player.moveSpeed += 3.0f; Player.maxRunSpeed += 15f; Player.runAcceleration *= 4f; Player.jumpSpeedBoost += 20f; Player.gravity *= 0.5f; Player.statDefense += 100; for (int i = 0; i < 3; i++) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.SolarFlare, 0, 0, 0, default, 2f); Main.dust[d].noGravity = true; Main.dust[d].velocity = Player.velocity * 0.5f; } }
            }

            // 月亮特效
            if (isVampireWings)
            {
                if (!TryConsumeSpirituality(0.2f, true)) { isVampireWings = false; Main.NewText("灵性耗尽，黑暗之翼消散！", 200, 50, 50); return; }
                Player.wingTime = 1000; Player.wingTimeMax = 1000; Player.wingsLogic = 12; Player.noFallDmg = true;
                if (Main.rand.NextBool(4)) Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Shadowflame, 0, 0, 100).noGravity = true;
            }

            if (isMoonlightized)
            {
                if (!TryConsumeSpirituality(0.5f, true)) { isMoonlightized = false; Main.NewText("灵性耗尽！", 200, 50, 50); return; }
                Player.immune = true; Player.immuneTime = 2; Player.noItems = true; Player.noKnockback = true; Player.invis = true; Player.maxRunSpeed += 10f; Player.moveSpeed += 2.0f;
                if (Main.rand.NextBool(2)) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.VampireHeal, Player.velocity.X, Player.velocity.Y, 100, default, 1.5f); Main.dust[d].noGravity = true; }
            }

            if (isFullMoonActive)
            {
                if (!TryConsumeSpirituality(0.1f, true)) { isFullMoonActive = false; return; }
                Lighting.AddLight(Player.Center, 0.6f, 0.7f, 0.9f);
                if (Player.ownedProjectileCounts[ModContent.ProjectileType<FullMoonCircle>()] < 1) { Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<FullMoonCircle>(), 0, 0, Player.whoAmI); }
                if (Main.rand.NextBool(10)) { Vector2 pos = Player.Center + Main.rand.NextVector2Circular(100, 100); Dust d = Dust.NewDustPerfect(pos, DustID.BlueCrystalShard, Vector2.Zero, 150, default, 1.0f); d.noGravity = true; }
            }

            // 蝙蝠化身
            if (isBatSwarm)
            {
                if (!TryConsumeSpirituality(0.5f, true)) { isBatSwarm = false; Main.NewText("灵性耗尽，蝙蝠化身解除！", 200, 50, 50); return; }
                Player.mount.Dismount(Player); Player.noFallDmg = true; Player.noKnockback = true; Player.wingTime = 1000; Player.wingsLogic = 12; Player.noItems = true; Player.invis = true;

                int maxBats = 60;
                int targetBatCount = (int)(maxBats * ((float)Player.statLife / Player.statLifeMax2));
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
                if (Main.GameUpdateCount % 60 == 0) { for (int i = 0; i < Main.maxPlayers; i++) { Player p = Main.player[i]; if (p.active && !p.dead && p.whoAmI != Player.whoAmI && p.Distance(Player.Center) < 800f) { p.HealEffect(50); p.statLife += 50; if (p.statLife > p.statLifeMax2) p.statLife = p.statLifeMax2; } } }
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
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { if (currentSequence <= 2 && twilightResurrectionCooldown <= 0) { SoundEngine.PlaySound(SoundID.Item119, Player.position); Player.statLife = Player.statLifeMax2; Player.HealEffect(Player.statLifeMax2); Player.immune = true; Player.immuneTime = 180; twilightResurrectionCooldown = TWILIGHT_RESURRECTION_MAX; return false; } return true; }

        // 7. 按键
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            float moonMult = GetSequenceMultiplier(currentMoonSequence);
            float hunterMult = GetSequenceMultiplier(currentHunterSequence);
            float giantMult = GetSequenceMultiplier(currentSequence);

            // 月亮
            if (LotMKeybinds.Moon_Wings.JustPressed && currentMoonSequence <= 7) { isVampireWings = !isVampireWings; if (isVampireWings) { isBatSwarm = false; SoundEngine.PlaySound(SoundID.Item103, Player.position); Main.NewText("黑暗之翼：展开", 180, 0, 0); } else Main.NewText("黑暗之翼：收起", 200, 200, 200); }
            if (LotMKeybinds.Moon_BatSwarm.JustPressed && currentMoonSequence <= 4) { isBatSwarm = !isBatSwarm; if (isBatSwarm) { isVampireWings = false; isMoonlightized = false; SoundEngine.PlaySound(SoundID.Item103, Player.position); Main.NewText("化身为蝙蝠群...", 100, 0, 200); } else Main.NewText("解除化身", 200, 200, 200); }
            if (LotMKeybinds.Moon_PaperFigurine.JustPressed && currentMoonSequence <= 4)
            {
                if (currentMoonSequence <= 2)
                {
                    if (paperFigurineCooldown <= 0 && TryConsumeSpirituality(5000)) { SoundEngine.PlaySound(SoundID.Item119, Player.position); Main.NewText("万物滋长：生命归顺！", 100, 255, 100); foreach (NPC npc in Main.ActiveNPCs) { if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 1500f) { if (npc.boss) { int dmg = (int)(5000 * moonMult); npc.SimpleStrikeNPC(dmg, 0, true, 0, DamageClass.Magic); CombatText.NewText(npc.getRect(), Color.Red, "生命剥夺!", true); } else { npc.AddBuff(ModContent.BuffType<TamedBuff>(), 36000); CombatText.NewText(npc.getRect(), Color.LightGreen, "新生!", true); for (int i = 0; i < 20; i++) Dust.NewDust(npc.position, npc.width, npc.height, DustID.Terra, 0, 0, 0, default, 1.5f); } } } paperFigurineCooldown = 18000; } else if (paperFigurineCooldown > 0) Main.NewText($"转化冷却: {paperFigurineCooldown / 60}s", 150, 150, 150);
                }
                else if (paperFigurineCooldown <= 0 && TryConsumeSpirituality(100)) { Player.velocity = -Player.velocity * 2f; if (Player.velocity.Length() < 10f) Player.velocity = new Vector2(-Player.direction * 10f, -5f); Player.immune = true; Player.immuneTime = 60; for (int i = 0; i < 30; i++) Dust.NewDust(Player.oldPosition, Player.width, Player.height, DustID.Confetti, 0, 0, 100, default, 1.5f); SoundEngine.PlaySound(SoundID.Item6, Player.position); paperFigurineCooldown = 900; } else if (paperFigurineCooldown > 0) Main.NewText($"纸人冷却: {paperFigurineCooldown / 60}s", 150, 150, 150);
            }
            if (LotMKeybinds.Moon_Gaze.JustPressed && currentMoonSequence <= 4) { if (darknessGazeCooldown <= 0 && TryConsumeSpirituality(200)) { bool hit = false; for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && npc.getRect().Contains(Main.MouseWorld.ToPoint())) { int dmg = (int)(5000 * moonMult); npc.SimpleStrikeNPC(dmg, 0, true, 0, DamageClass.Magic); npc.AddBuff(BuffID.Darkness, 600); npc.AddBuff(BuffID.ShadowFlame, 600); hit = true; } } if (hit) { SoundEngine.PlaySound(SoundID.Item104, Main.MouseWorld); darknessGazeCooldown = 1200; } } else if (darknessGazeCooldown > 0) Main.NewText($"凝视冷却: {darknessGazeCooldown / 60}s", 150, 150, 150); }
            if (LotMKeybinds.Moon_Shackles.JustPressed && currentMoonSequence <= 7) { if (currentMoonSequence <= 5 && isFullMoonActive) { if (fireTeleportCooldown <= 0 && TryConsumeSpirituality(50)) { Vector2 targetPos = Main.MouseWorld; if (Player.Distance(targetPos) < 800f && Collision.CanHit(Player.position, Player.width, Player.height, targetPos, Player.width, Player.height)) { SoundEngine.PlaySound(SoundID.Item8, Player.position); Player.Teleport(targetPos, 1); fireTeleportCooldown = 30; } } } else if (abyssShackleCooldown <= 0 && TryConsumeSpirituality(30)) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 12f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<AbyssShackleProjectile>(), 20, 0f, Player.whoAmI); SoundEngine.PlaySound(SoundID.Item8, Player.position); abyssShackleCooldown = 180; } }
            if (LotMKeybinds.Moon_Grenade.JustPressed && currentMoonSequence <= 6)
            {
                if (currentMoonSequence <= 2) { if (purifyCooldown <= 0 && TryConsumeSpirituality(1000)) { int radius = 60; int centerX = (int)(Player.Center.X / 16f); int centerY = (int)(Player.Center.Y / 16f); for (int x = centerX - radius; x <= centerX + radius; x++) for (int y = centerY - radius; y <= centerY + radius; y++) WorldGen.Convert(x, y, 0, 0); SoundEngine.PlaySound(SoundID.Item29, Player.position); Main.NewText("大地重获新生。", 100, 255, 100); purifyCooldown = 600; } }
                else if (TryConsumeSpirituality(20)) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 14f; int dmg = (int)(60 * moonMult); Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<AlchemicalGrenade>(), dmg, 5f, Player.whoAmI); SoundEngine.PlaySound(SoundID.Item1, Player.position); }
            }
            if (LotMKeybinds.Moon_Elixir.JustPressed && currentMoonSequence <= 6)
            {
                if (currentMoonSequence <= 2) { if (elixirCooldown <= 0 && TryConsumeSpirituality(2000)) { int heal = Player.statLifeMax2 - Player.statLife; Player.statLife = Player.statLifeMax2; Player.HealEffect(heal); for (int i = 0; i < Player.MaxBuffs; i++) if (Main.debuff[Player.buffType[i]]) Player.DelBuff(i); SoundEngine.PlaySound(SoundID.Item4, Player.position); Main.NewText("生命奇迹：重获新生！", 0, 255, 255); elixirCooldown = 3600; } else if (elixirCooldown > 0) Main.NewText($"奇迹冷却: {elixirCooldown / 60}s", 150, 150, 150); }
                else if (elixirCooldown <= 0 && TryConsumeSpirituality(200)) { int heal = (int)(1000 * moonMult); Player.statLife += heal; if (Player.statLife > Player.statLifeMax2) Player.statLife = Player.statLifeMax2; Player.HealEffect(heal); SoundEngine.PlaySound(SoundID.Item3, Player.position); elixirCooldown = 3600; Main.NewText("服用生命灵液！", 50, 255, 50); } else if (elixirCooldown > 0) Main.NewText($"灵液冷却: {elixirCooldown / 60}s", 200, 50, 50);
            }
            if (LotMKeybinds.Moon_Moonlight.JustPressed && currentMoonSequence <= 5) { isMoonlightized = !isMoonlightized; if (isMoonlightized) { isBatSwarm = false; SoundEngine.PlaySound(SoundID.Item8, Player.position); Main.NewText("身体化为绯红月光...", 255, 100, 100); } else Main.NewText("解除月光化", 200, 200, 200); }
            if (LotMKeybinds.Moon_FullMoon.JustPressed && currentMoonSequence <= 5)
            {
                if (currentMoonSequence <= 2) { isCreationDomain = !isCreationDomain; if (isCreationDomain) { isFullMoonActive = false; SoundEngine.PlaySound(SoundID.Item29, Player.position); Main.NewText("创生领域：生命主宰", 0, 255, 0); } else Main.NewText("领域收起", 200, 200, 200); }
                else { isFullMoonActive = !isFullMoonActive; if (isFullMoonActive) { SoundEngine.PlaySound(SoundID.Item29, Player.position); Main.NewText("满月降临", 255, 50, 50); } else Main.NewText("满月隐去", 200, 200, 200); }
            }
            if (LotMKeybinds.Moon_SummonGate.JustPressed && currentMoonSequence <= 3) { if (summonGateCooldown <= 0 && TryConsumeSpirituality(300)) { Vector2 spawnPos = Player.Center + new Vector2(Player.direction * 50, -50); Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<SummoningDoorProjectile>(), 0, 0f, Player.whoAmI); summonGateCooldown = 18000; Main.NewText("召唤之门已打开！", 150, 100, 255); } else if (summonGateCooldown > 0) Main.NewText($"召唤冷却: {summonGateCooldown / 60}s", 150, 150, 150); }

            // 猎人/巨人
            if (LotMKeybinds.RP_Transformation.JustPressed) { if (currentHunterSequence <= 2) { isCalamityGiant = !isCalamityGiant; if (isCalamityGiant) Main.NewText("灾祸巨人形态", 0, 255, 255); } else if (currentHunterSequence <= 4) { isFireForm = !isFireForm; if (isFireForm) Main.NewText("火焰形态", 255, 100, 0); } }
            if (LotMKeybinds.RP_Flash.JustPressed && currentHunterSequence <= 6) { if (fireTeleportCooldown <= 0 && TryConsumeSpirituality(100)) { Vector2 targetPos = Main.MouseWorld; if (Player.Distance(targetPos) < 600f && Collision.CanHit(Player.position, Player.width, Player.height, targetPos, Player.width, Player.height)) { SoundEngine.PlaySound(SoundID.Item14, Player.position); for (int i = 0; i < 20; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, 0, 0, 0, default, 2f); Player.Teleport(targetPos, 1); fireTeleportCooldown = 60; } } }
            if (LotMKeybinds.RP_Bomb.JustPressed && currentHunterSequence <= 7) { if (TryConsumeSpirituality(50)) { int dmg = (int)(100 * hunterMult); Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 10f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<PyromaniacBomb>(), dmg, 5f, Player.whoAmI); } }
            if (LotMKeybinds.RP_Cloak.JustPressed && currentHunterSequence <= 7) { isFlameCloakActive = !isFlameCloakActive; if (isFlameCloakActive) Main.NewText("火焰披风开启", 255, 100, 0); }
            if (LotMKeybinds.RP_Slash.JustPressed && currentHunterSequence <= 5) { if (!isFireForm && TryConsumeSpirituality(100)) { int dmg = (int)(200 * hunterMult); Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<ReaperSlashProjectile>(), dmg, 10f, Player.whoAmI); } }
            if (LotMKeybinds.RP_Enchant.JustPressed && currentHunterSequence <= 6) { isFireEnchanted = !isFireEnchanted; Main.NewText("武器附魔切换", 255, 100, 0); }
            if (currentHunterSequence <= 7) { if (LotMKeybinds.RP_Skill.Current) { if (TryConsumeSpirituality(0.5f)) { isChargingFireball = true; fireballChargeTimer++; } } if (LotMKeybinds.RP_Skill.JustReleased && isChargingFireball) { int dmg = (int)(100 * hunterMult); Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 12f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<PyromaniacFireball>(), dmg, 4f, Player.whoAmI); isChargingFireball = false; fireballChargeTimer = 0; } }
            if (LotMKeybinds.RP_Army.JustPressed && currentHunterSequence <= 4) { if (!isFireForm) { isArmyOfOne = !isArmyOfOne; Main.NewText("集众切换", 255, 100, 0); } }
            if (LotMKeybinds.RP_Weather.JustPressed && currentHunterSequence <= 2) { if (TryConsumeSpirituality(200, true)) { int dmg = (int)(500 * hunterMult); Projectile.NewProjectile(Player.GetSource_FromThis(), Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<WeatherStrikeLightning>(), dmg, 5f, Player.whoAmI); } }
            if (LotMKeybinds.RP_Glacier.JustPressed && currentHunterSequence <= 2)
            {
                if (glacierCooldown <= 0 && TryConsumeSpirituality(1000))
                {
                    Main.NewText("冰河世纪！", 0, 255, 255);
                    glacierCooldown = 1800;
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < 2000f)
                        {
                            npc.AddBuff(BuffID.Frozen, 300);
                            npc.AddBuff(BuffID.Frostburn2, 600);
                            npc.AddBuff(BuffID.Slow, 600);
                            if (!npc.boss) npc.velocity = Vector2.Zero;
                        }
                    }
                }
            }

            if (LotMKeybinds.Giant_Mercury.JustPressed && currentSequence <= 3) { if (!isGuardianStance) { if (!isMercuryForm) { if (TryConsumeSpirituality(500)) isMercuryForm = true; } else isMercuryForm = false; } }
            if (LotMKeybinds.Giant_Armor.JustPressed && currentSequence <= 6) { if (!isMercuryForm && !dawnArmorBroken) dawnArmorActive = !dawnArmorActive; }
            if (currentSequence <= 5 && LotMKeybinds.Giant_Guardian.Current && !isMercuryForm) { if (TryConsumeSpirituality(10.0f)) isGuardianStance = true; else isGuardianStance = false; } else isGuardianStance = false;
        }

        // 8. 辅助
        public float GetSequenceMultiplier(int seq) { if (seq > 9) return 1f; return 1f + (9 - seq) * 0.3f; }
        public bool TryConsumeSpirituality(float amount, bool isMaintenance = false) { if (isCalamityGiant && !isMaintenance) return true; if (spiritualityCurrent >= amount) { spiritualityCurrent -= amount; return true; } return false; }
        public override void ModifyScreenPosition() { if (shakeTime > 0) { Main.screenPosition += Main.rand.NextVector2Circular(shakePower, shakePower); shakeTime--; } base.ModifyScreenPosition(); }
        public override bool CanUseItem(Item item) { if (isFireForm || isMercuryForm || isGuardianStance || isMoonlightized || isBatSwarm) return false; return base.CanUseItem(item); }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (isCalamityGiant || isMercuryForm || isMoonlightized) return true;
            return base.FreeDodge(info);
        }

        private void CalculateMaxSpirituality()
        {
            float giantMult = GetSequenceMultiplier(currentSequence);
            float hunterMult = GetSequenceMultiplier(currentHunterSequence);
            float moonMult = GetSequenceMultiplier(currentMoonSequence);

            int max = 100;
            if (currentSequence < 10) max = (int)(200 * giantMult * giantMult);
            if (currentHunterSequence < 10) { int hMax = (int)(200 * hunterMult * hunterMult); if (hMax > max) max = hMax; }
            if (currentMoonSequence < 10) { int mMax = (int)(200 * moonMult * moonMult); if (mMax > max) max = mMax; }

            if (currentMoonSequence <= 4) max = 5000;
            if (currentMoonSequence <= 3) max = 20000;
            if (currentMoonSequence <= 2) max = 100000;

            if (currentSequence <= 2) max = 50000;
            if (currentHunterSequence <= 1) max = 100000;

            spiritualityMax = max;
        }

        private void HandleSpiritualityRegen()
        {
            spiritualityRegenTimer++;
            if (spiritualityRegenTimer >= 60)
            {
                spiritualityRegenTimer = 0;
                float baseRegen = 2f;
                float giantMult = GetSequenceMultiplier(currentSequence);
                float hunterMult = GetSequenceMultiplier(currentHunterSequence);
                float moonMult = GetSequenceMultiplier(currentMoonSequence);

                float bestMult = 1f;
                if (giantMult > bestMult) bestMult = giantMult;
                if (hunterMult > bestMult) bestMult = hunterMult;
                if (moonMult > bestMult) bestMult = moonMult;

                float finalRegen = baseRegen * bestMult * bestMult;
                if (currentMoonSequence <= 2) finalRegen += 200f;

                if (spiritualityCurrent < spiritualityMax) spiritualityCurrent += finalRegen;
                if (spiritualityCurrent > spiritualityMax) spiritualityCurrent = spiritualityMax;
            }
        }

        private void HandleDawnArmorLogic() { if (dawnArmorBroken) { dawnArmorCooldownTimer--; if (dawnArmorCooldownTimer <= 0) { dawnArmorBroken = false; dawnArmorCurrentHP = MaxDawnArmorHP; Main.NewText("铠甲已重铸", 100, 255, 100); } } else if (!dawnArmorActive && dawnArmorCurrentHP < MaxDawnArmorHP && Main.GameUpdateCount % 2 == 0) dawnArmorCurrentHP++; }
        private void SpawnVisualDust() { for (int i = 0; i < 40; i++) Dust.NewDustPerfect(Player.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(5f, 5f), 100, default, 2.0f).noGravity = true; }
        private void CheckConquerorRitual() { if (currentHunterSequence == 2 && !conquerorRitualComplete && ConquerorSpawnSystem.StopSpawning) { bool enemyExists = false; for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.townNPC && npc.lifeMax > 5 && !npc.dontTakeDamage) { enemyExists = true; break; } } if (!enemyExists) { conquerorRitualComplete = true; Main.NewText("这片大陆已无敌手... 征服的意志已达成！", 255, 0, 0); SoundEngine.PlaySound(SoundID.Roar, Player.position); } } }
    }

    public class ApothecaryCrafting : Terraria.ModLoader.GlobalItem { public override void OnCreated(Terraria.Item item, ItemCreationContext context) { if (context is RecipeItemCreationContext) { Player p = Main.LocalPlayer; if (p != null && p.active && p.GetModPlayer<LotMPlayer>().currentMoonSequence <= 9) { bool isP = item.consumable && (item.buffType > 0 || item.healLife > 0 || item.healMana > 0); if (isP) p.QuickSpawnItem(item.GetSource_FromThis(), item.type, item.stack); } } } }
}