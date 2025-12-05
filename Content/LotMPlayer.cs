using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Shaders; // 必须引用，用于获取染料ID
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
        public int currentMoonSequence = 10;   // 月亮途径 (9-7)

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

        // --- 月亮途径技能状态 ---
        public bool isVampireWings = false;    // 吸血鬼：黑暗之翼开关
        public int abyssShackleCooldown = 0;   // 吸血鬼：深渊枷锁冷却

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
        // 3. 每帧重置与核心逻辑
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

            // 仪式计时器
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

            arsonistFireImmune = false;

            ApplySequenceStats();
            CheckConquerorRitual();
        }

        // ===================================================
        // 4. 数值加成系统
        // ===================================================
        private void ApplySequenceStats()
        {
            // --- 巨人/战士 ---
            if (currentSequence <= 9) { Player.statDefense += 5; Player.GetDamage(DamageClass.Melee) += 0.10f; Player.GetCritChance(DamageClass.Melee) += 5; Player.moveSpeed += 0.1f; }
            if (currentSequence <= 8) { Player.GetAttackSpeed(DamageClass.Melee) += 0.10f; Player.endurance += 0.05f; Player.noKnockback = true; }
            if (currentSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.10f; Player.GetCritChance(DamageClass.Generic) += 5; Player.GetArmorPenetration(DamageClass.Generic) += 5; }
            if (currentSequence <= 6) { Lighting.AddLight(Player.Center, 1.5f, 1.5f, 1.5f); Player.statDefense += 10; Player.lifeRegen += 2; }
            if (currentSequence <= 5) { Player.statDefense += 15; Player.endurance += 0.05f; Player.buffImmune[BuffID.Confused] = true; }
            if (currentSequence <= 4) { Player.statLifeMax2 += 100; Player.GetDamage(DamageClass.Generic) += 0.15f; Player.GetCritChance(DamageClass.Generic) += 10; Player.nightVision = true; }
            if (currentSequence <= 3) { Player.statDefense += 20; Player.lifeRegen += 5; Player.GetAttackSpeed(DamageClass.Melee) += 0.15f; Player.blackBelt = true; }
            if (currentSequence <= 2) { Player.statLifeMax2 += 400; Player.statDefense += 30; Player.endurance += 0.10f; Player.GetDamage(DamageClass.Generic) += 0.20f; }

            // --- 猎人 ---
            if (currentHunterSequence <= 9) { Player.moveSpeed += 0.15f; Player.GetDamage(DamageClass.Ranged) += 0.10f; Player.GetDamage(DamageClass.Melee) += 0.05f; Player.detectCreature = true; Player.dangerSense = true; }
            if (currentHunterSequence <= 8) { Player.statDefense += 8; Player.aggro += 300; Player.lifeRegen += 2; }
            if (currentHunterSequence <= 7) { Player.GetDamage(DamageClass.Generic) += 0.15f; Player.buffImmune[BuffID.OnFire] = true; Player.buffImmune[BuffID.OnFire3] = true; Player.buffImmune[BuffID.Frostburn] = true; Player.resistCold = true; }
            if (currentHunterSequence <= 6) { Player.GetCritChance(DamageClass.Generic) += 10; Player.manaCost -= 0.15f; }
            if (currentHunterSequence <= 5) { Player.GetArmorPenetration(DamageClass.Generic) += 20; Player.GetCritChance(DamageClass.Generic) += 15; }
            if (currentHunterSequence <= 4) { Player.statDefense += 40; Player.endurance += 0.15f; Player.maxMinions += 2; Player.noKnockback = true; }
            if (currentHunterSequence <= 3) { Player.maxMinions += 3; Player.maxTurrets += 2; Player.GetDamage(DamageClass.Summon) += 0.30f; }
            if (currentHunterSequence <= 2) { Player.statLifeMax2 += 500; Player.statManaMax2 += 200; Player.GetDamage(DamageClass.Generic) += 0.30f; Player.buffImmune[BuffID.WindPushed] = true; }
            if (currentHunterSequence <= 1) { Player.statDefense += 80; Player.endurance += 0.30f; Player.GetDamage(DamageClass.Generic) += 0.80f; Player.GetCritChance(DamageClass.Generic) += 30; Player.aggro += 2000; Player.buffImmune[BuffID.Weak] = true; Player.buffImmune[BuffID.BrokenArmor] = true; Player.buffImmune[BuffID.WitheredArmor] = true; Player.buffImmune[BuffID.WitheredWeapon] = true; }

            // --- 月亮 (Moon) ---
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

                Player.noFallDmg = true; // 免疫摔伤 (1.4.4 API)

                // 厌恶阳光
                if (Main.dayTime && Player.ZoneOverworldHeight && Player.behindBackWall == false)
                {
                    Player.statDefense -= 5;
                    Player.lifeRegen -= 3;
                    Player.GetDamage(DamageClass.Generic) -= 0.1f;
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

            // 【吸血鬼】黑暗之翼
            if (isVampireWings)
            {
                // 1. 设置为原版蝙蝠翅膀 (ID: 12)
                Player.wings = 12;

                // 2. 【核心修改】强制应用黑色染料
                // 这会让红褐色的蝙蝠翅膀变成纯黑色/暗影色
                int blackDyeId = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BlackDye);
                Player.cWings = blackDyeId;
            }
        }

        public override void PostUpdateMiscEffects()
        {
            // 灾祸巨人
            if (isCalamityGiant) { if (!TryConsumeSpirituality(100.0f, true)) { isCalamityGiant = false; Main.NewText("灵性枯竭，灾祸形态解除！", 255, 50, 50); return; } Player.statDefense += 200; Player.endurance += 0.3f; Player.GetDamage(DamageClass.Generic) += 1.0f; Player.moveSpeed += 2f; Player.maxRunSpeed += 10f; Player.noKnockback = true; Player.wingsLogic = 0; Player.wingTime = 9999; Player.gravity = 0f; Player.statLifeMax2 += 2000; Player.invis = true; if (Main.rand.NextBool(2)) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Electric, 0, 0, 0, default, 1.5f); }

            // 集众
            if (isArmyOfOne) { if (currentHunterSequence <= 4 && TryConsumeSpirituality(5.0f, true)) { Player.maxMinions += 5; } else isArmyOfOne = false; }

            // 火焰形态
            if (isFireForm)
            {
                if (!TryConsumeSpirituality(50.0f, true)) { isFireForm = false; Main.NewText("灵性枯竭，火焰熄灭！", 255, 50, 50); return; }
                Player.noKnockback = true; Player.wingsLogic = 0; Player.wingTime = 9999; Player.rocketTime = 9999; Player.noItems = true;
                if (currentHunterSequence <= 1)
                {
                    Player.moveSpeed += 5.0f; Player.maxRunSpeed += 30f; Player.runAcceleration *= 10f; Player.jumpSpeedBoost += 30f; Player.statDefense += 200; Player.endurance += 0.4f;
                    Vector2 tip = Player.Center + Player.velocity * 2f; for (int i = 0; i < 5; i++) { Vector2 pos = Vector2.Lerp(Player.Center, tip, i / 5f) + Main.rand.NextVector2Circular(10, 10); int d = Dust.NewDust(pos, 0, 0, DustID.Shadowflame, 0, 0, 0, default, 2.5f); Main.dust[d].noGravity = true; Main.dust[d].velocity = -Player.velocity * 0.2f; }
                    Rectangle myRect = Player.getRect(); myRect.Inflate(40, 40);
                    if (Player.velocity.Length() > 5f) { for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(myRect)) { if (npc.immune[Player.whoAmI] <= 0) { int dashDamage = (int)(Player.GetDamage(DamageClass.Melee).ApplyTo(2000)); Player.ApplyDamageToNPC(npc, dashDamage, 30f, Player.direction, true); npc.AddBuff(ModContent.BuffType<ConquerorWill>(), 600); npc.immune[Player.whoAmI] = 6; SoundEngine.PlaySound(SoundID.Item74, npc.Center); for (int k = 0; k < 20; k++) Dust.NewDust(npc.position, npc.width, npc.height, DustID.Shadowflame, 0, 0, 0, default, 3f); } } } }
                }
                else
                {
                    Player.moveSpeed += 3.0f; Player.maxRunSpeed += 15f; Player.runAcceleration *= 4f; Player.jumpSpeedBoost += 20f; Player.gravity *= 0.5f; Player.statDefense += 100; for (int i = 0; i < 3; i++) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.SolarFlare, 0, 0, 0, default, 2f); Main.dust[d].noGravity = true; Main.dust[d].velocity = Player.velocity * 0.5f; }
                }
            }

            // 【吸血鬼】黑暗之翼飞行逻辑
            if (isVampireWings)
            {
                if (!TryConsumeSpirituality(0.2f, true))
                {
                    isVampireWings = false;
                    Main.NewText("灵性耗尽，黑暗之翼消散！", 200, 50, 50);
                    return;
                }

                // 赋予无限飞行
                Player.wingTime = 1000;
                Player.wingTimeMax = 1000;
                Player.wingsLogic = 12;     // 物理逻辑对应 Bat Wings
                Player.noFallDmg = true;    // 免疫摔伤

                // 背后粒子特效
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Shadowflame, -Player.velocity.X * 0.5f, -Player.velocity.Y * 0.5f, 100, default, 1.0f);
                    d.noGravity = true;
                }
            }

            if (isFireEnchanted) { if (currentHunterSequence <= 6 && TryConsumeSpirituality(0.16f, true)) { } else isFireEnchanted = false; }
            if (isFlameCloakActive) { if (currentHunterSequence <= 7 && TryConsumeSpirituality(1.0f, true)) { Player.buffImmune[BuffID.Chilled] = true; Player.buffImmune[BuffID.Frozen] = true; Player.buffImmune[BuffID.Poisoned] = true; Player.buffImmune[BuffID.Venom] = true; if (Main.rand.NextBool(4)) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, 0, 0, 0, default, 1.5f); Main.dust[d].noGravity = true; Main.dust[d].velocity *= 0.5f; } } else isFlameCloakActive = false; }
            if (fireTeleportCooldown > 0) fireTeleportCooldown--;
            if (isMercuryForm) { if (!TryConsumeSpirituality(20.0f, true)) { isMercuryForm = false; Main.NewText("灵性枯竭，被迫退出水银形态！", 255, 50, 50); return; } Player.moveSpeed += 2.0f; Player.maxRunSpeed += 10f; Player.runAcceleration *= 3f; Player.noKnockback = true; Player.invis = true; Rectangle myRect = Player.getRect(); myRect.Inflate(10, 10); foreach (NPC npc in Main.npc) { if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.getRect().Intersects(myRect)) { if (npc.immune[Player.whoAmI] == 0) { int damage = (int)(Player.GetDamage(DamageClass.Melee).ApplyTo(50) * 5f); Player.ApplyDamageToNPC(npc, damage, 10f, Player.direction, false); npc.immune[Player.whoAmI] = 10; npc.AddBuff(BuffID.Slow, 300); npc.AddBuff(BuffID.Frostburn, 300); } } } }
            if (currentSequence <= 3 && !isMercuryForm) { if (Player.velocity.Length() < 0.1f) { stealthTimer++; if (stealthTimer > 60) { Player.invis = true; Player.aggro -= 1000; } } else { stealthTimer = 0; } }
            if (isGuardianStance) { Player.velocity.X = 0; if (Player.velocity.Y < 0) Player.velocity.Y = 0; Player.noItems = true; Player.statDefense += 80; Player.endurance += 0.5f; Player.noKnockback = true; Player.aggro += 1000; Player.hasPaladinShield = true; if (Player.ownedProjectileCounts[ModContent.ProjectileType<GuardianShieldProjectile>()] <= 0) Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<GuardianShieldProjectile>(), 0, 0, Player.whoAmI); }
            if (glacierCooldown > 0) glacierCooldown--;
        }

        // ===================================================
        // 6. 攻击与受击逻辑
        // ===================================================
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyHitEffects(target);
            CheckRitualKill(target);
            CheckExecution(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyHitEffects(target);
            CheckRitualKill(target);
            CheckExecution(target);
        }

        private void ApplyHitEffects(NPC target)
        {
            if (currentSequence <= 4) target.AddBuff(BuffID.Ichor, 300);
            if (currentSequence <= 2) { target.AddBuff(BuffID.Daybreak, 300); target.AddBuff(BuffID.BetsysCurse, 300); }
            if (currentHunterSequence <= 7) { target.AddBuff(BuffID.OnFire, 300); if (currentHunterSequence <= 5) target.AddBuff(BuffID.OnFire3, 300); }
            if (isCalamityGiant) { target.AddBuff(BuffID.Electrified, 300); target.AddBuff(BuffID.Frostburn2, 300); }
            if (currentHunterSequence <= 1) target.AddBuff(ModContent.BuffType<ConquerorWill>(), 600);

            // 吸血鬼：腐蚀之爪
            if (currentMoonSequence <= 7 && (Player.HeldItem.DamageType == DamageClass.Melee || Player.HeldItem.DamageType == DamageClass.SummonMeleeSpeed || Player.HeldItem.DamageType == DamageClass.Summon))
            {
                target.AddBuff(BuffID.Ichor, 300);
                if (Main.rand.NextBool(3)) target.AddBuff(BuffID.Poisoned, 300);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f; }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { if (currentHunterSequence <= 5) modifiers.CritDamage += 0.5f; }
        private void CheckExecution(NPC target) { if (currentHunterSequence <= 5) { if (!target.boss && target.lifeMax > 10 && target.life < target.lifeMax * 0.2f) { int executeDamage = target.life + 9999; target.SimpleStrikeNPC(executeDamage, 0, true, 0, DamageClass.Generic, true); CombatText.NewText(target.getRect(), Color.Red, "斩杀!", true); for (int i = 0; i < 30; i++) { int d = Dust.NewDust(target.position, target.width, target.height, DustID.Blood, 0, 0, 0, default, 2.5f); Main.dust[d].velocity *= 2f; } } } }
        private void CheckRitualKill(NPC target)
        {
            if (target.life <= 0)
            {
                if (currentSequence == 5 && demonHunterRitualProgress < DEMON_HUNTER_RITUAL_TARGET) { if (target.type == NPCID.RedDevil) { demonHunterRitualProgress++; if (demonHunterRitualProgress >= DEMON_HUNTER_RITUAL_TARGET) { Main.NewText("【猎魔仪式】已完成！", 200, 0, 255); SoundEngine.PlaySound(SoundID.Roar, Player.position); } else CombatText.NewText(Player.getRect(), Color.Purple, $"猎魔: {demonHunterRitualProgress}/{DEMON_HUNTER_RITUAL_TARGET}"); } }
                if (currentHunterSequence == 5 && ironBloodRitualProgress < IRON_BLOOD_RITUAL_TARGET) { if (Player.slotsMinions >= 5f) { ironBloodRitualProgress++; if (ironBloodRitualProgress >= IRON_BLOOD_RITUAL_TARGET) { Main.NewText("【铁血仪式】已完成！", 255, 69, 0); SoundEngine.PlaySound(SoundID.Roar, Player.position); } else if (ironBloodRitualProgress % 10 == 0) { CombatText.NewText(Player.getRect(), Color.OrangeRed, $"征服: {ironBloodRitualProgress}/{IRON_BLOOD_RITUAL_TARGET}"); } } }
            }
        }
        public override void PostHurt(Player.HurtInfo info)
        {
            if (currentSequence <= 3) { for (int i = 0; i < 20; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Silver, 0, 0, 100, default, 1.5f); }
            if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken) { dawnArmorCurrentHP -= info.Damage; if (dawnArmorCurrentHP <= 0) { dawnArmorCurrentHP = 0; dawnArmorActive = false; dawnArmorBroken = true; dawnArmorCooldownTimer = DAWN_ARMOR_COOLDOWN_MAX; SoundEngine.PlaySound(SoundID.Shatter, Player.position); Main.NewText("警告：【黎明铠甲】破碎！", 255, 50, 50); } }
            if (currentSequence == 6 && guardianRitualProgress < GUARDIAN_RITUAL_TARGET) { bool npcNearby = false; for (int i = 0; i < Main.maxNPCs; i++) { if (Main.npc[i].active && Main.npc[i].townNPC && Main.npc[i].Distance(Player.Center) < 800f) { npcNearby = true; break; } } if (npcNearby) { guardianRitualProgress += info.Damage; if (guardianRitualProgress >= GUARDIAN_RITUAL_TARGET) Main.NewText("【守护仪式】已完成！", 0, 255, 0); } }
        }
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (currentSequence <= 2 && twilightResurrectionCooldown <= 0) { SoundEngine.PlaySound(SoundID.Item119, Player.position); for (int i = 0; i < 100; i++) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.OrangeTorch, 0, 0, 0, default, 3f); Main.dust[d].noGravity = true; Main.dust[d].velocity *= 3f; } Player.statLife = Player.statLifeMax2; Player.HealEffect(Player.statLifeMax2); Player.immune = true; Player.immuneTime = 180; Player.Spawn(PlayerSpawnContext.ReviveFromDeath); twilightResurrectionCooldown = TWILIGHT_RESURRECTION_MAX; Main.NewText("黄昏的权柄让你拒绝了死亡！(冷却开始)", 255, 100, 0); return false; }
            if (currentHunterSequence == 5 && ironBloodRitualProgress < IRON_BLOOD_RITUAL_TARGET && ironBloodRitualProgress > 0) { ironBloodRitualProgress = 0; Main.NewText("失败... 统帅不应倒下。仪式进度已重置。", 255, 50, 50); }
            return true;
        }

        // ===================================================
        // 7. 输入处理 (按键)
        // ===================================================
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // --- 吸血鬼专属按键 ---
            // K键：黑暗之翼
            if (LotMKeybinds.DarknessWings.JustPressed)
            {
                if (currentMoonSequence <= 7)
                {
                    isVampireWings = !isVampireWings;
                    if (isVampireWings)
                    {
                        SoundEngine.PlaySound(SoundID.Item103, Player.position);
                        Main.NewText("黑暗之翼：展开", 180, 0, 0);
                    }
                    else Main.NewText("黑暗之翼：收起", 200, 200, 200);
                }
            }

            // V键：深渊枷锁
            if (LotMKeybinds.AbyssShackles.JustPressed)
            {
                if (currentMoonSequence <= 7)
                {
                    if (abyssShackleCooldown <= 0 && TryConsumeSpirituality(30))
                    {
                        Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 12f;
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<AbyssShackleProjectile>(), 20, 0f, Player.whoAmI);

                        SoundEngine.PlaySound(SoundID.Item8, Player.position);
                        abyssShackleCooldown = 180; // 3秒冷却
                    }
                }
            }

            // --- 猎人/巨人按键 (使用旧按键) ---
            if (LotMKeybinds.FireForm.JustPressed)
            {
                if (currentHunterSequence <= 2)
                {
                    isCalamityGiant = !isCalamityGiant;
                    if (isCalamityGiant) { SoundEngine.PlaySound(SoundID.Item119, Player.position); Main.NewText("灾祸巨人形态：降临！", 0, 255, 255); } else { Main.NewText("灾祸解除", 200, 200, 200); }
                }
                else if (currentHunterSequence <= 4)
                {
                    isFireForm = !isFireForm;
                    if (isFireForm) { SoundEngine.PlaySound(SoundID.Item20, Player.position); if (currentHunterSequence <= 1) Main.NewText("征服者：紫焰长枪 (毁灭一切阻挡者)", 180, 0, 255); else Main.NewText("铁血骑士：元素化 (禁用武器)", 255, 69, 0); } else Main.NewText("解除形态", 200, 200, 200);
                }
            }

            if (LotMKeybinds.ConspiratorMove.JustPressed && currentHunterSequence <= 6)
            {
                if (fireTeleportCooldown <= 0 && TryConsumeSpirituality(100)) { Vector2 targetPos = Main.MouseWorld; if (Player.Distance(targetPos) < 600f && Collision.CanHit(Player.position, Player.width, Player.height, targetPos, Player.width, Player.height)) { SoundEngine.PlaySound(SoundID.Item14, Player.position); for (int i = 0; i < 20; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, 0, 0, 0, default, 2f); Player.Teleport(targetPos, 1); SoundEngine.PlaySound(SoundID.Item74, Player.position); for (int i = 0; i < 30; i++) { int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, 0, 0, 0, default, 2f); Main.dust[d].velocity *= 2f; } fireTeleportCooldown = 60; } }
            }

            // ... (其他按键)
            if (LotMKeybinds.WeatherStrike.JustPressed && currentHunterSequence <= 2) { if (TryConsumeSpirituality(200, true)) { Projectile.NewProjectile(Player.GetSource_FromThis(), Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<WeatherStrikeLightning>(), 500, 5f, Player.whoAmI); } }
            if (LotMKeybinds.GlacierFreeze.JustPressed && currentHunterSequence <= 2) { if (glacierCooldown <= 0 && TryConsumeSpirituality(1000, true)) { Main.NewText("冰河世纪！", 0, 255, 255); SoundEngine.PlaySound(SoundID.Item28, Player.position); GlacierVisualSystem.StartFlash(); foreach (NPC npc in Main.npc) { if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 2000f) { npc.AddBuff(BuffID.Frozen, 300); npc.AddBuff(BuffID.Frostburn2, 600); npc.AddBuff(BuffID.Slow, 600); if (!npc.boss) npc.velocity = Vector2.Zero; } } glacierCooldown = 1800; } else if (glacierCooldown > 0) Main.NewText($"冰河冷却中: {glacierCooldown / 60}s", 200, 50, 50); }
            if (LotMKeybinds.ArmySkill.JustPressed && currentHunterSequence <= 4) { if (isFireForm) return; isArmyOfOne = !isArmyOfOne; if (isArmyOfOne) { SoundEngine.PlaySound(SoundID.Item79, Player.position); Main.NewText("集众：军团降临", 255, 100, 0); } else Main.NewText("集众：解散", 200, 200, 200); }
            if (LotMKeybinds.ReaperSlash.JustPressed && currentHunterSequence <= 5) { if (isFireForm) return; if (TryConsumeSpirituality(100)) Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<ReaperSlashProjectile>(), 200, 10f, Player.whoAmI); }
            if (LotMKeybinds.PyroBomb.JustPressed && currentHunterSequence <= 7) { if (TryConsumeSpirituality(50)) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 10f; int damage = 100 + (7 - currentHunterSequence) * 30; Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, ModContent.ProjectileType<PyromaniacBomb>(), damage, 5f, Player.whoAmI); } }
            if (LotMKeybinds.PyroCloak.JustPressed && currentHunterSequence <= 7) { isFlameCloakActive = !isFlameCloakActive; if (isFlameCloakActive) { SoundEngine.PlaySound(SoundID.Item20, Player.position); Main.NewText("火焰披风已开启", 255, 100, 0); } else Main.NewText("火焰披风已关闭", 200, 200, 200); }
            if (LotMKeybinds.PyroEnchant.JustPressed && currentHunterSequence <= 6) { isFireEnchanted = !isFireEnchanted; if (isFireEnchanted) { SoundEngine.PlaySound(SoundID.Item20, Player.position); Main.NewText("操纵火焰：武器已附魔", 255, 100, 0); } else Main.NewText("火焰熄灭", 200, 200, 200); }
            if (currentHunterSequence <= 7) { if (LotMKeybinds.PyroSkill.Current) { if (TryConsumeSpirituality(0.5f)) { isChargingFireball = true; fireballChargeTimer++; if (fireballChargeTimer > 180) fireballChargeTimer = 180; int dustType = DustID.Torch; if (fireballChargeTimer > 60) dustType = DustID.OrangeTorch; if (fireballChargeTimer > 120) dustType = DustID.WhiteTorch; Vector2 handPos = Player.Center + new Vector2(Player.direction * 15, 0); for (int i = 0; i < 3; i++) { Vector2 offset = Main.rand.NextVector2Circular(20, 20) * (1f + fireballChargeTimer / 60f); Vector2 spawnPos = handPos + offset; int d = Dust.NewDust(spawnPos, 0, 0, dustType, 0, 0, 100, default, 1.5f); Main.dust[d].noGravity = true; Main.dust[d].velocity = -offset * 0.1f; } } else { isChargingFireball = false; fireballChargeTimer = 0; } } if (LotMKeybinds.PyroSkill.JustReleased && isChargingFireball) { Vector2 dir = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 12f; int damage = 40 + (9 - currentHunterSequence) * 10; float multiplier = 1f + (fireballChargeTimer / 60f); int finalDamage = (int)(damage * multiplier); int projType = ModContent.ProjectileType<PyromaniacFireball>(); if (fireballChargeTimer > 120) { projType = ModContent.ProjectileType<PyromaniacFireballWhite>(); finalDamage = (int)(finalDamage * 1.5f); SoundEngine.PlaySound(SoundID.Item88, Player.position); } else { SoundEngine.PlaySound(SoundID.Item20, Player.position); } Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, dir, projType, finalDamage, 4f, Player.whoAmI); isChargingFireball = false; fireballChargeTimer = 0; } }
            if (LotMKeybinds.SilverSkill.JustPressed && currentSequence <= 3) { if (isGuardianStance) return; if (!isMercuryForm) { if (TryConsumeSpirituality(500)) { isMercuryForm = true; if (dawnArmorActive) { dawnArmorActive = false; Main.NewText("铠甲已解除，以此身化为水银...", 192, 192, 192); } SoundEngine.PlaySound(SoundID.Item29, Player.position); for (int i = 0; i < 50; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Silver, 0, 0, 0, default, 2f); } } else { isMercuryForm = false; SoundEngine.PlaySound(SoundID.Item4, Player.position); } }
            if (LotMKeybinds.Ability1.JustPressed) { if (isMercuryForm) return; if (currentSequence <= 6) { if (dawnArmorBroken) { Main.NewText($"铠甲重铸中... {dawnArmorCooldownTimer / 60}s", 200, 50, 50); return; } if (!dawnArmorActive) { if (TryConsumeSpirituality(200)) { dawnArmorActive = true; SoundEngine.PlaySound(SoundID.Item29, Player.position); SpawnVisualDust(); } } else { dawnArmorActive = false; SoundEngine.PlaySound(SoundID.Item4, Player.position); } } }
            if (currentSequence <= 5 && LotMKeybinds.GuardianSkill.Current && !isMercuryForm) { if (TryConsumeSpirituality(10.0f)) isGuardianStance = true; else isGuardianStance = false; } else { isGuardianStance = false; }
        }

        // ===================================================
        // 8. 辅助函数
        // ===================================================
        public bool TryConsumeSpirituality(float amount, bool isMaintenance = false) { if (isCalamityGiant && !isMaintenance) return true; if (spiritualityCurrent >= amount) { spiritualityCurrent -= amount; return true; } return false; }
        public override void ModifyScreenPosition() { if (shakeTime > 0) { Main.screenPosition += Main.rand.NextVector2Circular(shakePower, shakePower); shakeTime--; } base.ModifyScreenPosition(); }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { if (isCalamityGiant || isFireForm || isMercuryForm) { drawInfo.hideEntirePlayer = true; return; } if (currentSequence <= 3 && stealthTimer > 60) { drawInfo.hideEntirePlayer = true; drawInfo.shadow = 0f; a = 0f; return; } if (currentSequence <= 6 && dawnArmorActive && !dawnArmorBroken && !Player.shroomiteStealth) { r = 2.0f; g = 2.0f; b = 2.0f; fullBright = true; if (Main.rand.NextBool(20)) Dust.NewDustPerfect(Player.Center, DustID.Enchanted_Gold, Vector2.Zero, 0, default, 0.8f).noGravity = true; } }
        public override bool CanUseItem(Item item) { if (isFireForm || isMercuryForm || isGuardianStance) return false; return base.CanUseItem(item); }
        public override bool FreeDodge(Player.HurtInfo info) { if (isCalamityGiant && Main.rand.NextFloat() < 0.5f) { Player.SetImmuneTimeForAllTypes(60); return true; } if (isMercuryForm && Main.rand.NextFloat() < 0.75f) { Player.SetImmuneTimeForAllTypes(60); return true; } return base.FreeDodge(info); }

        private void CalculateMaxSpirituality()
        {
            int baseSpirit = 100;
            if (currentSequence <= 9) baseSpirit = 200; if (currentSequence <= 8) baseSpirit = 350; if (currentSequence <= 7) baseSpirit = 600; if (currentSequence <= 6) baseSpirit = 1200; if (currentSequence <= 5) baseSpirit = 2500; if (currentSequence <= 4) baseSpirit = 5000; if (currentSequence <= 3) baseSpirit = 10000; if (currentSequence <= 2) baseSpirit = 50000;
            int hunterSpirit = 100;
            if (currentHunterSequence <= 9) hunterSpirit = 200; if (currentHunterSequence <= 8) hunterSpirit = 350; if (currentHunterSequence <= 7) hunterSpirit = 600; if (currentHunterSequence <= 6) hunterSpirit = 1200; if (currentHunterSequence <= 5) hunterSpirit = 2500; if (currentHunterSequence <= 4) hunterSpirit = 6000; if (currentHunterSequence <= 3) hunterSpirit = 15000; if (currentHunterSequence <= 2) hunterSpirit = 50000; if (currentHunterSequence <= 1) hunterSpirit = 100000;

            // 月亮灵性
            int moonSpirit = 100;
            if (currentMoonSequence <= 9) moonSpirit = 200;
            if (currentMoonSequence <= 8) moonSpirit = 350;
            if (currentMoonSequence <= 7) moonSpirit = 600;

            int max = baseSpirit;
            if (hunterSpirit > max) max = hunterSpirit;
            if (moonSpirit > max) max = moonSpirit;

            spiritualityMax = max;
            if (spiritualityCurrent > spiritualityMax) spiritualityCurrent = spiritualityMax;
        }

        private void HandleSpiritualityRegen()
        {
            spiritualityRegenTimer++;
            if (spiritualityRegenTimer >= 60)
            {
                spiritualityRegenTimer = 0;
                float regenAmount = 1f;
                if (currentSequence < 10) regenAmount += (9 - currentSequence) * 2f;
                if (currentSequence <= 4) regenAmount += 30; if (currentSequence <= 3) regenAmount += 50; if (currentSequence <= 2) regenAmount += 200;
                if (currentHunterSequence <= 9) regenAmount += 1f;
                if (currentHunterSequence <= 6) regenAmount += 3f; if (currentHunterSequence <= 5) regenAmount += 5f; if (currentHunterSequence <= 4) regenAmount += 20f; if (currentHunterSequence <= 3) regenAmount += 50f; if (currentHunterSequence <= 2) regenAmount += 200f; if (currentHunterSequence <= 1) regenAmount += 500f;
                if (currentMoonSequence <= 9) regenAmount += 2f;
                if (currentMoonSequence <= 7) regenAmount += 3f;

                if (spiritualityCurrent < spiritualityMax)
                {
                    spiritualityCurrent += regenAmount;
                    if (spiritualityCurrent > spiritualityMax) spiritualityCurrent = spiritualityMax;
                }
            }
        }
        private void HandleDawnArmorLogic() { if (dawnArmorBroken) { dawnArmorCooldownTimer--; if (dawnArmorCooldownTimer <= 0) { dawnArmorBroken = false; dawnArmorCurrentHP = DAWN_ARMOR_MAX_HP; Main.NewText("铠甲已重铸", 100, 255, 100); } } else if (!dawnArmorActive && dawnArmorCurrentHP < DAWN_ARMOR_MAX_HP && Main.GameUpdateCount % 2 == 0) dawnArmorCurrentHP++; }
        private void SpawnVisualDust() { for (int i = 0; i < 40; i++) Dust.NewDustPerfect(Player.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(5f, 5f), 100, default, 2.0f).noGravity = true; }
        private void CheckConquerorRitual()
        {
            if (currentHunterSequence == 2 && !conquerorRitualComplete && ConquerorSpawnSystem.StopSpawning)
            {
                bool enemyExists = false; for (int i = 0; i < Main.maxNPCs; i++) { NPC npc = Main.npc[i]; if (npc.active && !npc.friendly && !npc.townNPC && npc.lifeMax > 5 && !npc.dontTakeDamage) { enemyExists = true; break; } }
                if (!enemyExists) { conquerorRitualComplete = true; Main.NewText("这片大陆已无敌手... 征服的意志已达成！", 255, 0, 0); SoundEngine.PlaySound(SoundID.Roar, Player.position); }
            }
        }
    }

    // 【药师合成钩子】使用 OnCreated
    public class ApothecaryCrafting : Terraria.ModLoader.GlobalItem
    {
        public override void OnCreated(Terraria.Item item, Terraria.DataStructures.ItemCreationContext context)
        {
            if (context is Terraria.DataStructures.RecipeItemCreationContext)
            {
                Player player = Main.LocalPlayer;
                if (player != null && player.active)
                {
                    var modPlayer = player.GetModPlayer<LotMPlayer>();
                    if (modPlayer.currentMoonSequence <= 9)
                    {
                        bool isPotion = item.consumable && (item.buffType > 0 || item.healLife > 0 || item.healMana > 0);
                        if (isPotion)
                        {
                            player.QuickSpawnItem(item.GetSource_FromThis(), item.type, item.stack);
                            if (Main.rand.NextBool(5)) CombatText.NewText(player.getRect(), Color.LightGreen, "药师双倍产出!", true);
                        }
                    }
                }
            }
        }
    }
}