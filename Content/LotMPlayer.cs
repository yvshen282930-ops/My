using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures; // 必须引用，用于 ItemCreationContext
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
        public int currentMoonSequence = 10;   // 月亮途径 (9-5)

        // 判断是否是非凡者 (任意途径序列 < 10)
        public bool IsBeyonder => currentSequence < 10 || currentHunterSequence < 10 || currentMoonSequence < 10;

        // 灵性系统
        public float spiritualityCurrent = 100;
        public int spiritualityMax = 100;
        private int spiritualityRegenTimer = 0;

        // --- 巨人途径技能状态 ---
        public bool dawnArmorActive = false;
        public bool dawnArmorBroken = false;
        public int dawnArmorCurrentHP = 250;
        public const int DAWN_ARMOR_MAX_HP = 250;
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

        // --- 月亮途径技能状态 (包含序列5深红学者) ---
        public bool isVampireWings = false;    // 吸血鬼：黑暗之翼开关
        public int abyssShackleCooldown = 0;   // 吸血鬼：深渊枷锁冷却
        public int elixirCooldown = 0;         // 魔药教授：生命灵液冷却
        public bool isFullMoonActive = false;  // 深红学者：满月领域
        public bool isMoonlightized = false;   // 深红学者：月光化

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

            // 仪式计时器
            if (weatherRitualTimer > 0)
            {
                weatherRitualTimer--;
                if (weatherRitualTimer == 0 && !weatherRitualComplete)
                {
                    if (weatherRitualCount > 0)
                        Main.NewText("符文共鸣消散了...", 200, 200, 200);
                    weatherRitualCount = 0;
                }
            }

            // 状态自动解除 (序列不足时关闭技能)
            if (currentSequence > 3) isMercuryForm = false;
            if (currentHunterSequence > 4) { isFireForm = false; isArmyOfOne = false; }
            if (currentHunterSequence > 2) isCalamityGiant = false;
            if (currentMoonSequence > 7) isVampireWings = false;
            if (currentMoonSequence > 5) { isFullMoonActive = false; isMoonlightized = false; }

            arsonistFireImmune = false;

            ApplySequenceStats();
            CheckConquerorRitual();
        }

        // ===================================================
        // 4. 数值加成系统
        // ===================================================
        private void ApplySequenceStats()
        {
            // --- 巨人/战士途径 (Sequence 9 - 2) ---
            if (currentSequence <= 9) { Player.statDefense += 5; Player.GetDamage(DamageClass.Melee) += 0.10f; Player.GetCritChance(DamageClass.Melee) += 5; Player.moveSpeed += 0.1f; }
            if (currentSequence <= 8) { Player.GetAttackSpeed(DamageClass.Melee) += 0.10f; Player.endurance += 0.05f; Player.noKnockback = true; }
            if (currentSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.10f; Player.GetCritChance(DamageClass.Generic) += 5; Player.GetArmorPenetration(DamageClass.Generic) += 5; }
            if (currentSequence <= 6) { Lighting.AddLight(Player.Center, 1.5f, 1.5f, 1.5f); Player.statDefense += 10; Player.lifeRegen += 2; }
            if (currentSequence <= 5) { Player.statDefense += 15; Player.endurance += 0.05f; Player.buffImmune[BuffID.Confused] = true; Player.buffImmune[BuffID.Darkness] = true; Player.buffImmune[BuffID.Silenced] = true; }
            if (currentSequence <= 4) { Player.statLifeMax2 += 100; Player.GetDamage(DamageClass.Generic) += 0.15f; Player.GetCritChance(DamageClass.Generic) += 10; Player.nightVision = true; Player.detectCreature = true; Player.buffImmune[BuffID.CursedInferno] = true; Player.buffImmune[BuffID.ShadowFlame] = true; }
            if (currentSequence <= 3) { Player.statDefense += 20; Player.lifeRegen += 5; Player.GetAttackSpeed(DamageClass.Melee) += 0.15f; Player.blackBelt = true; }
            if (currentSequence <= 2) { Player.statLifeMax2 += 400; Player.statDefense += 30; Player.endurance += 0.10f; Player.GetDamage(DamageClass.Generic) += 0.20f; }

            // --- 猎人途径 (Sequence 9 - 1) ---
            if (currentHunterSequence <= 9) { Player.moveSpeed += 0.15f; Player.GetDamage(DamageClass.Ranged) += 0.10f; Player.GetDamage(DamageClass.Melee) += 0.05f; Player.detectCreature = true; Player.dangerSense = true; }
            if (currentHunterSequence <= 8) { Player.statDefense += 8; Player.aggro += 300; Player.lifeRegen += 2; }
            if (currentHunterSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.15f; Player.buffImmune[BuffID.OnFire] = true; Player.buffImmune[BuffID.OnFire3] = true; Player.buffImmune[BuffID.Frostburn] = true; Player.resistCold = true; }
            if (currentHunterSequence <= 6) { Player.GetCritChance(DamageClass.Generic) += 10; Player.manaCost -= 0.15f; }
            if (currentHunterSequence <= 5) { Player.GetArmorPenetration(DamageClass.Generic) += 20; Player.GetCritChance(DamageClass.Generic) += 15; }
            if (currentHunterSequence <= 4) { Player.statDefense += 40; Player.endurance += 0.15f; Player.maxMinions += 2; Player.noKnockback = true; }
            if (currentHunterSequence <= 3) { Player.maxMinions += 3; Player.maxTurrets += 2; Player.GetDamage(DamageClass.Summon) += 0.30f; }
            if (currentHunterSequence <= 2) { Player.statLifeMax2 += 500; Player.statManaMax2 += 200; Player.GetDamage(DamageClass.Generic) += 0.30f; Player.buffImmune[BuffID.WindPushed] = true; }
            if (currentHunterSequence <= 1) { Player.statDefense += 80; Player.endurance += 0.30f; Player.GetDamage(DamageClass.Generic) += 0.80f; Player.GetCritChance(DamageClass.Generic) += 30; Player.aggro += 2000; Player.buffImmune[BuffID.Weak] = true; Player.buffImmune[BuffID.BrokenArmor] = true; Player.buffImmune[BuffID.WitheredArmor] = true; Player.buffImmune[BuffID.WitheredWeapon] = true; }

            // --- 月亮途径 (Sequence 9 - 5) ---
            if (currentMoonSequence <= 9) // 药师
            {
                Player.buffImmune[BuffID.Poisoned] = true;
                Player.buffImmune[BuffID.Venom] = true;
                Player.detectCreature = true;
                Player.lifeRegen += 2;
                Player.statLifeMax2 += 20;
            }
            if (currentMoonSequence <= 8) // 驯兽师
            {
                Player.statDefense += 5;
                Player.GetDamage(DamageClass.Generic) += 0.10f;
                Player.moveSpeed += 0.2f;
                Player.jumpSpeedBoost += 1.5f;
                Player.dangerSense = true;
                Player.maxMinions += 1;
            }
            if (currentMoonSequence <= 7) // 吸血鬼
            {
                Player.statLifeMax2 += 50;
                Player.lifeRegen += 5;
                Player.moveSpeed += 0.3f;
                Player.jumpSpeedBoost += 2.0f;
                Player.noFallDmg = true;

                // 厌恶阳光
                if (Main.dayTime && Player.ZoneOverworldHeight && Player.behindBackWall == false)
                {
                    Player.statDefense -= 5;
                    Player.lifeRegen -= 3;
                    Player.GetDamage(DamageClass.Generic) -= 0.1f;
                }
            }
            if (currentMoonSequence <= 6) // 魔药教授
            {
                Player.statManaMax2 += 50;
                Player.GetDamage(DamageClass.Magic) += 0.15f;
                Player.pStone = true; // 炼金石
                Player.buffImmune[BuffID.OnFire] = true;
                Player.buffImmune[BuffID.Frostburn] = true;
                Player.buffImmune[BuffID.CursedInferno] = true;
            }
            if (currentMoonSequence <= 5) // 深红学者
            {
                Player.lifeRegen += 10;
                Player.moveSpeed += 0.5f;
                Player.maxRunSpeed += 2f;

                Player.buffImmune[BuffID.Confused] = true;
                Player.buffImmune[BuffID.Darkness] = true;
                Player.buffImmune[BuffID.Silenced] = true;
                Player.buffImmune[BuffID.Blackout] = true;

                // 满月领域加成
                if (isFullMoonActive)
                {
                    Player.GetDamage(DamageClass.Magic) += 0.30f;
                    Player.statDefense -= 10;
                    // 这里只加一点微弱的光，主要靠弹幕发光
                    Lighting.AddLight(Player.Center, 0.1f, 0.1f, 0.2f);
                    Player.manaRegen += 20;
                    Player.manaRegenDelayBonus += 5;
                }
            }
        }

        // ===================================================
        // 5. 视觉特效与翅膀
        // ===================================================
        public override void FrameEffects()
        {
            if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken)
            {
                int dyeId = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BrightSilverDye);
                Player.cHead = dyeId; Player.cBody = dyeId; Player.cLegs = dyeId;
            }

            // 吸血鬼：黑暗之翼 (Bat Wings, Black Dye)
            if (isVampireWings)
            {
                Player.wings = 12;
                int blackDyeId = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BlackDye);
                Player.cWings = blackDyeId;
            }

            // 月光化 (Red Acid Dye)
            if (isMoonlightized)
            {
                int redDyeId = GameShaders.Armor.GetShaderIdFromItemId(ItemID.RedAcidDye);
                Player.cHead = redDyeId; Player.cBody = redDyeId; Player.cLegs = redDyeId;
            }
        }

        public override void PostUpdateMiscEffects()
        {
            // 灾祸巨人
            if (isCalamityGiant) { if (!TryConsumeSpirituality(100.0f, true)) { isCalamityGiant = false; Main.NewText("灵性枯竭！", 255, 50, 50); return; } Player.statDefense += 200; Player.endurance += 0.3f; Player.GetDamage(DamageClass.Generic) += 1.0f; Player.moveSpeed += 2f; Player.maxRunSpeed += 10f; Player.noKnockback = true; Player.wingsLogic = 0; Player.wingTime = 9999; Player.gravity = 0f; Player.statLifeMax2 += 2000; Player.invis = true; if (Main.rand.NextBool(2)) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Electric, 0, 0, 0, default, 1.5f); }

            // 集众
            if (isArmyOfOne) { if (currentHunterSequence <= 4 && TryConsumeSpirituality(5.0f, true)) { Player.maxMinions += 5; } else isArmyOfOne = false; }

            // 火焰形态
            if (isFireForm) { if (!TryConsumeSpirituality(50.0f, true)) { isFireForm = false; Main.NewText("灵性枯竭！", 255, 50, 50); return; } Player.noKnockback = true; Player.wingsLogic = 0; Player.wingTime = 9999; Player.rocketTime = 9999; Player.noItems = true; if (currentHunterSequence <= 1) { /*Conqueror Charge Logic*/ Player.moveSpeed += 5.0f; Player.maxRunSpeed += 30f; Player.runAcceleration *= 10f; Player.jumpSpeedBoost += 30f; Player.statDefense += 200; Player.endurance += 0.4f; Vector2 tip = Player.Center + Player.velocity * 2f; for (int i = 0; i < 5; i++) { Vector2 pos = Vector2.Lerp(Player.Center, tip, i / 5f) + Main.rand.NextVector2Circular(10, 10); int d = Dust.NewDust(pos, 0, 0, DustID.Shadowflame, 0, 0, 0, default, 2.5f); Main.dust[d].noGravity = true; Main.dust[d].velocity = -Player.velocity * 0.2f; } Rectangle myRect = Player.getRect(); myRect.Inflate(40, 40); if (Player.velocity.Length() > 5f) { for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(myRect)) { if (npc.immune[Player.whoAmI] <= 0) { int dashDamage = (int)(Player.GetDamage(DamageClass.Melee).ApplyTo(2000)); Player.ApplyDamageToNPC(npc, dashDamage, 30f, Player.direction, true); npc.AddBuff(ModContent.BuffType<ConquerorWill>(), 600); npc.immune[Player.whoAmI] = 6; SoundEngine.PlaySound(SoundID.Item74, npc.Center); for (int k = 0; k < 20; k++) Dust.NewDust(npc.position, npc.width, npc.height, DustID.Shadowflame, 0, 0, 0, default, 3f); } } } } } else { /*Knight Fire*/ Player.moveSpeed += 3.0f; Player.maxRunSpeed += 15f; Player.runAcceleration *= 4f; Player.jumpSpeedBoost += 20f; Player.gravity *= 0.5f; Player.statDefense += 100; for (int i = 0; i < 3; i++) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.SolarFlare, 0, 0, 0, default, 2f); Main.dust[d].noGravity = true; Main.dust[d].velocity = Player.velocity * 0.5f; } } }

            // 黑暗之翼
            if (isVampireWings)
            {
                if (!TryConsumeSpirituality(0.2f, true)) { isVampireWings = false; Main.NewText("灵性耗尽！", 200, 50, 50); return; }
                Player.wingTime = 1000; Player.wingTimeMax = 1000; Player.wingsLogic = 12; Player.noFallDmg = true;
                if (Main.rand.NextBool(4)) Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Shadowflame, 0, 0, 100).noGravity = true;
            }

            // 月光化
            if (isMoonlightized)
            {
                if (!TryConsumeSpirituality(0.5f, true)) { isMoonlightized = false; Main.NewText("灵性耗尽！", 200, 50, 50); return; }
                Player.immune = true; Player.immuneTime = 2; Player.noItems = true; Player.noKnockback = true; Player.invis = true; Player.maxRunSpeed += 10f; Player.moveSpeed += 2.0f;
                if (Main.rand.NextBool(2)) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.VampireHeal, Player.velocity.X, Player.velocity.Y, 100, default, 1.5f); Main.dust[d].noGravity = true; }
            }

            // 满月领域 (生成法阵)
            if (isFullMoonActive)
            {
                if (!TryConsumeSpirituality(0.1f, true)) { isFullMoonActive = false; return; }

                // 生成法阵弹幕
                if (Player.ownedProjectileCounts[ModContent.ProjectileType<FullMoonCircle>()] < 1)
                {
                    Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<FullMoonCircle>(), 0, 0, Player.whoAmI);
                }

                // 环境光
                if (Main.rand.NextBool(10))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(100, 100);
                    // 冰蓝/白色粒子
                    Dust d = Dust.NewDustPerfect(pos, DustID.BlueCrystalShard, Vector2.Zero, 150, default, 1.0f);
                    d.noGravity = true;
                }
            }

            if (isFireEnchanted) { if (currentHunterSequence <= 6 && TryConsumeSpirituality(0.16f, true)) { } else isFireEnchanted = false; }
            if (isFlameCloakActive) { if (currentHunterSequence <= 7 && TryConsumeSpirituality(1.0f, true)) { Player.buffImmune[BuffID.Chilled] = true; Player.buffImmune[BuffID.Frozen] = true; Player.buffImmune[BuffID.Poisoned] = true; Player.buffImmune[BuffID.Venom] = true; if (Main.rand.NextBool(4)) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, 0, 0, 0, default, 1.5f); Main.dust[d].noGravity = true; Main.dust[d].velocity *= 0.5f; } } else isFlameCloakActive = false; }
            if (fireTeleportCooldown > 0) fireTeleportCooldown--;
            if (isMercuryForm) { if (!TryConsumeSpirituality(20.0f, true)) isMercuryForm = false; else { Player.moveSpeed += 2.0f; Player.invis = true; Rectangle myRect = Player.getRect(); myRect.Inflate(10, 10); foreach (NPC npc in Main.npc) { if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(myRect)) { if (npc.immune[Player.whoAmI] == 0) { int damage = (int)(Player.GetDamage(DamageClass.Melee).ApplyTo(50) * 5f); Player.ApplyDamageToNPC(npc, damage, 10f, Player.direction, false); npc.immune[Player.whoAmI] = 10; npc.AddBuff(BuffID.Slow, 300); npc.AddBuff(BuffID.Frostburn, 300); } } } } }
            if (currentSequence <= 3 && !isMercuryForm) { if (Player.velocity.Length() < 0.1f) { stealthTimer++; if (stealthTimer > 60) { Player.invis = true; Player.aggro -= 1000; } } else { stealthTimer = 0; } }
            if (isGuardianStance) { Player.velocity.X = 0; Player.statDefense += 80; Player.noKnockback = true; if (Player.ownedProjectileCounts[ModContent.ProjectileType<GuardianShieldProjectile>()] <= 0) Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<GuardianShieldProjectile>(), 0, 0, Player.whoAmI); }
            if (glacierCooldown > 0) glacierCooldown--;
        }

        // 6. 攻击
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { ApplyHitEffects(target); CheckRitualKill(target); CheckExecution(target); }
        private void ApplyHitEffects(NPC target) { if (currentSequence <= 4) target.AddBuff(BuffID.Ichor, 300); if (currentHunterSequence <= 7) target.AddBuff(BuffID.OnFire, 300); if (isCalamityGiant) target.AddBuff(BuffID.Electrified, 300); if (currentHunterSequence <= 1) target.AddBuff(ModContent.BuffType<ConquerorWill>(), 600); if (currentMoonSequence <= 7 && (Player.HeldItem.DamageType == DamageClass.Melee || Player.HeldItem.DamageType == DamageClass.SummonMeleeSpeed || Player.HeldItem.DamageType == DamageClass.Summon)) { target.AddBuff(BuffID.Ichor, 300); if (Main.rand.NextBool(3)) target.AddBuff(BuffID.Poisoned, 300); } }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f; }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f; }
        private void CheckExecution(NPC target) { if (currentHunterSequence <= 5 && !target.boss && target.life < target.lifeMax * 0.2f) target.SimpleStrikeNPC(9999, 0); }
        private void CheckRitualKill(NPC target) { if (target.life <= 0) { if (currentSequence == 5 && demonHunterRitualProgress < DEMON_HUNTER_RITUAL_TARGET) { if (target.type == NPCID.RedDevil) { demonHunterRitualProgress++; if (demonHunterRitualProgress >= DEMON_HUNTER_RITUAL_TARGET) { Main.NewText("【猎魔仪式】已完成！", 200, 0, 255); SoundEngine.PlaySound(SoundID.Roar, Player.position); } else CombatText.NewText(Player.getRect(), Color.Purple, $"猎魔: {demonHunterRitualProgress}/{DEMON_HUNTER_RITUAL_TARGET}"); } } if (currentHunterSequence == 5 && ironBloodRitualProgress < IRON_BLOOD_RITUAL_TARGET) { if (Player.slotsMinions >= 5f) { ironBloodRitualProgress++; if (ironBloodRitualProgress >= IRON_BLOOD_RITUAL_TARGET) { Main.NewText("【铁血仪式】已完成！", 255, 69, 0); SoundEngine.PlaySound(SoundID.Roar, Player.position); } else if (ironBloodRitualProgress % 10 == 0) { CombatText.NewText(Player.getRect(), Color.OrangeRed, $"征服: {ironBloodRitualProgress}/{IRON_BLOOD_RITUAL_TARGET}"); } } } } }
        public override void PostHurt(Player.HurtInfo info) { if (currentSequence <= 3) { for (int i = 0; i < 20; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Silver, 0, 0, 100, default, 1.5f); } if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken) { dawnArmorCurrentHP -= info.Damage; if (dawnArmorCurrentHP <= 0) { dawnArmorCurrentHP = 0; dawnArmorActive = false; dawnArmorBroken = true; dawnArmorCooldownTimer = DAWN_ARMOR_COOLDOWN_MAX; SoundEngine.PlaySound(SoundID.Shatter, Player.position); Main.NewText("警告：【黎明铠甲】破碎！", 255, 50, 50); } } if (currentSequence == 6 && guardianRitualProgress < GUARDIAN_RITUAL_TARGET) { bool npcNearby = false; for (int i = 0; i < Main.maxNPCs; i++) { if (Main.npc[i].active && Main.npc[i].townNPC && Main.npc[i].Distance(Player.Center) < 800f) { npcNearby = true; break; } } if (npcNearby) { guardianRitualProgress += info.Damage; if (guardianRitualProgress >= GUARDIAN_RITUAL_TARGET) Main.NewText("【守护仪式】已完成！", 0, 255, 0); } } }
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { if (currentSequence <= 2 && twilightResurrectionCooldown <= 0) { SoundEngine.PlaySound(SoundID.Item119, Player.position); for (int i = 0; i < 100; i++) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.OrangeTorch, 0, 0, 0, default, 3f); Main.dust[d].noGravity = true; Main.dust[d].velocity *= 3f; } Player.statLife = Player.statLifeMax2; Player.HealEffect(Player.statLifeMax2); Player.immune = true; Player.immuneTime = 180; Player.Spawn(PlayerSpawnContext.ReviveFromDeath); twilightResurrectionCooldown = TWILIGHT_RESURRECTION_MAX; Main.NewText("黄昏的权柄让你拒绝了死亡！(冷却开始)", 255, 100, 0); return false; } if (currentHunterSequence == 5 && ironBloodRitualProgress < IRON_BLOOD_RITUAL_TARGET && ironBloodRitualProgress > 0) { ironBloodRitualProgress = 0; Main.NewText("失败... 统帅不应倒下。仪式进度已重置。", 255, 50, 50); } return true; }

        // 7. 按键
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // 月亮按键
            if (LotMKeybinds.Moon_Wings.JustPressed && currentMoonSequence <= 7) { isVampireWings = !isVampireWings; if (isVampireWings) { SoundEngine.PlaySound(SoundID.Item103, Player.position); Main.NewText("黑暗之翼：展开", 180, 0, 0); } else Main.NewText("黑暗之翼：收起", 200, 200, 200); }
            if (LotMKeybinds.Moon_Shackles.JustPressed && currentMoonSequence <= 7) { if (currentMoonSequence <= 5 && isFullMoonActive) { if (fireTeleportCooldown <= 0 && TryConsumeSpirituality(50)) { Vector2 targetPos = Main.MouseWorld; if (Player.Distance(targetPos) < 800f && Collision.CanHit(Player.position, Player.width, Player.height, targetPos, Player.width, Player.height)) { SoundEngine.PlaySound(SoundID.Item8, Player.position); Player.Teleport(targetPos, 1); fireTeleportCooldown = 30; } } } else if (abyssShackleCooldown <= 0 && TryConsumeSpirituality(30)) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 12f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<AbyssShackleProjectile>(), 20, 0f, Player.whoAmI); SoundEngine.PlaySound(SoundID.Item8, Player.position); abyssShackleCooldown = 180; } }
            if (LotMKeybinds.Moon_Grenade.JustPressed && currentMoonSequence <= 6) { if (TryConsumeSpirituality(20)) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 14f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<AlchemicalGrenade>(), 60 + (6 - currentMoonSequence) * 20, 5f, Player.whoAmI); SoundEngine.PlaySound(SoundID.Item1, Player.position); } }
            if (LotMKeybinds.Moon_Elixir.JustPressed && currentMoonSequence <= 6) { if (elixirCooldown <= 0 && TryConsumeSpirituality(200)) { int heal = 1000; Player.statLife += heal; if (Player.statLife > Player.statLifeMax2) Player.statLife = Player.statLifeMax2; Player.HealEffect(heal); SoundEngine.PlaySound(SoundID.Item3, Player.position); elixirCooldown = 3600; Main.NewText("服用生命灵液！", 50, 255, 50); } else if (elixirCooldown > 0) Main.NewText($"灵液冷却: {elixirCooldown / 60}s", 200, 50, 50); }
            if (LotMKeybinds.Moon_Moonlight.JustPressed && currentMoonSequence <= 5) { isMoonlightized = !isMoonlightized; if (isMoonlightized) { SoundEngine.PlaySound(SoundID.Item8, Player.position); Main.NewText("身体化为绯红月光...", 255, 100, 100); } else Main.NewText("解除月光化", 200, 200, 200); }
            if (LotMKeybinds.Moon_FullMoon.JustPressed && currentMoonSequence <= 5) { isFullMoonActive = !isFullMoonActive; if (isFullMoonActive) { SoundEngine.PlaySound(SoundID.Item29, Player.position); Main.NewText("满月降临", 255, 50, 50); } else Main.NewText("满月隐去", 200, 200, 200); }

            // 猎人/巨人按键
            if (LotMKeybinds.RP_Transformation.JustPressed) { if (currentHunterSequence <= 2) { isCalamityGiant = !isCalamityGiant; if (isCalamityGiant) Main.NewText("灾祸巨人形态", 0, 255, 255); } else if (currentHunterSequence <= 4) { isFireForm = !isFireForm; if (isFireForm) Main.NewText("火焰形态", 255, 100, 0); } }
            if (LotMKeybinds.RP_Flash.JustPressed && currentHunterSequence <= 6) { if (fireTeleportCooldown <= 0 && TryConsumeSpirituality(100)) { Vector2 targetPos = Main.MouseWorld; if (Player.Distance(targetPos) < 600f && Collision.CanHit(Player.position, Player.width, Player.height, targetPos, Player.width, Player.height)) { SoundEngine.PlaySound(SoundID.Item14, Player.position); for (int i = 0; i < 20; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, 0, 0, 0, default, 2f); Player.Teleport(targetPos, 1); fireTeleportCooldown = 60; } } }
            if (LotMKeybinds.RP_Bomb.JustPressed && currentHunterSequence <= 7) { if (TryConsumeSpirituality(50)) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 10f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<PyromaniacBomb>(), 100, 5f, Player.whoAmI); } }
            if (LotMKeybinds.RP_Cloak.JustPressed && currentHunterSequence <= 7) { isFlameCloakActive = !isFlameCloakActive; if (isFlameCloakActive) Main.NewText("火焰披风开启", 255, 100, 0); }
            if (LotMKeybinds.RP_Slash.JustPressed && currentHunterSequence <= 5) { if (!isFireForm && TryConsumeSpirituality(100)) Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<ReaperSlashProjectile>(), 200, 10f, Player.whoAmI); }
            if (LotMKeybinds.RP_Enchant.JustPressed && currentHunterSequence <= 6) { isFireEnchanted = !isFireEnchanted; Main.NewText("武器附魔切换", 255, 100, 0); }
            if (currentHunterSequence <= 7) { if (LotMKeybinds.RP_Skill.Current) { if (TryConsumeSpirituality(0.5f)) { isChargingFireball = true; fireballChargeTimer++; } } if (LotMKeybinds.RP_Skill.JustReleased && isChargingFireball) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 12f; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<PyromaniacFireball>(), 100, 4f, Player.whoAmI); isChargingFireball = false; fireballChargeTimer = 0; } }
            if (LotMKeybinds.RP_Army.JustPressed && currentHunterSequence <= 4) { if (!isFireForm) { isArmyOfOne = !isArmyOfOne; Main.NewText("集众切换", 255, 100, 0); } }
            if (LotMKeybinds.RP_Weather.JustPressed && currentHunterSequence <= 2) { if (TryConsumeSpirituality(200, true)) Projectile.NewProjectile(Player.GetSource_FromThis(), Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<WeatherStrikeLightning>(), 500, 5f, Player.whoAmI); }
            if (LotMKeybinds.RP_Glacier.JustPressed && currentHunterSequence <= 2) { if (glacierCooldown <= 0 && TryConsumeSpirituality(1000, true)) { Main.NewText("冰河世纪！", 0, 255, 255); glacierCooldown = 1800; } }

            if (LotMKeybinds.Giant_Mercury.JustPressed && currentSequence <= 3) { if (!isGuardianStance) { if (!isMercuryForm) { if (TryConsumeSpirituality(500)) isMercuryForm = true; } else isMercuryForm = false; } }
            if (LotMKeybinds.Giant_Armor.JustPressed && currentSequence <= 6) { if (!isMercuryForm && !dawnArmorBroken) dawnArmorActive = !dawnArmorActive; }
            if (currentSequence <= 5 && LotMKeybinds.Giant_Guardian.Current && !isMercuryForm) { if (TryConsumeSpirituality(10.0f)) isGuardianStance = true; else isGuardianStance = false; } else isGuardianStance = false;
        }

        // 8. 辅助
        public bool TryConsumeSpirituality(float amount, bool isMaintenance = false) { if (isCalamityGiant && !isMaintenance) return true; if (spiritualityCurrent >= amount) { spiritualityCurrent -= amount; return true; } return false; }
        public override void ModifyScreenPosition() { if (shakeTime > 0) { Main.screenPosition += Main.rand.NextVector2Circular(shakePower, shakePower); shakeTime--; } base.ModifyScreenPosition(); }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { if (isCalamityGiant || isFireForm || isMercuryForm || isMoonlightized) { drawInfo.hideEntirePlayer = true; return; } if (currentSequence <= 3 && stealthTimer > 60) { drawInfo.hideEntirePlayer = true; drawInfo.shadow = 0f; a = 0f; return; } if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken && !Player.shroomiteStealth) { r = 2.0f; g = 2.0f; b = 2.0f; fullBright = true; if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Player.Center, DustID.Enchanted_Gold, Vector2.Zero, 0, default, 0.8f).noGravity = true; } }
        public override bool CanUseItem(Item item) { if (isFireForm || isMercuryForm || isGuardianStance || isMoonlightized) return false; return base.CanUseItem(item); }
        public override bool FreeDodge(Player.HurtInfo info) { if (isCalamityGiant || isMercuryForm || isMoonlightized) return true; return base.FreeDodge(info); }
        private void CalculateMaxSpirituality() { int max = 100; if (currentMoonSequence <= 7) max = 600; if (currentMoonSequence <= 6) max = 1000; if (currentMoonSequence <= 5) max = 2000; spiritualityMax = max; }
        private void HandleSpiritualityRegen() { spiritualityRegenTimer++; if (spiritualityRegenTimer >= 60) { spiritualityRegenTimer = 0; float r = 1f; if (currentMoonSequence <= 6) r += 5f; if (currentMoonSequence <= 5) r += 10f; spiritualityCurrent += r; } }
        private void HandleDawnArmorLogic() { if (dawnArmorBroken) { dawnArmorCooldownTimer--; if (dawnArmorCooldownTimer <= 0) { dawnArmorBroken = false; dawnArmorCurrentHP = DAWN_ARMOR_MAX_HP; Main.NewText("铠甲已重铸", 100, 255, 100); } } else if (!dawnArmorActive && dawnArmorCurrentHP < DAWN_ARMOR_MAX_HP && Main.GameUpdateCount % 2 == 0) dawnArmorCurrentHP++; }
        private void SpawnVisualDust() { for (int i = 0; i < 40; i++) Dust.NewDustPerfect(Player.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(5f, 5f), 100, default, 2.0f).noGravity = true; }
        private void CheckConquerorRitual() { if (currentHunterSequence == 2 && !conquerorRitualComplete && ConquerorSpawnSystem.StopSpawning) { bool enemyExists = false; for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.townNPC && npc.lifeMax > 5 && !npc.dontTakeDamage) { enemyExists = true; break; } } if (!enemyExists) { conquerorRitualComplete = true; Main.NewText("这片大陆已无敌手... 征服的意志已达成！", 255, 0, 0); SoundEngine.PlaySound(SoundID.Roar, Player.position); } } }
    }

    public class ApothecaryCrafting : Terraria.ModLoader.GlobalItem
    {
        public override void OnCreated(Terraria.Item item, ItemCreationContext context)
        {
            if (context is RecipeItemCreationContext)
            {
                Player player = Main.LocalPlayer;
                if (player != null && player.active && player.GetModPlayer<LotMPlayer>().currentMoonSequence <= 9)
                {
                    bool isPotion = item.consumable && (item.buffType > 0 || item.healLife > 0 || item.healMana > 0);
                    if (isPotion) { player.QuickSpawnItem(item.GetSource_FromThis(), item.type, item.stack); if (Main.rand.NextBool(5)) CombatText.NewText(player.getRect(), Color.LightGreen, "药师双倍产出!", true); }
                }
            }
        }
    }
}