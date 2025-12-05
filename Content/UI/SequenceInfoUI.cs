using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using zhashi.Content;

namespace zhashi.Content.UI
{
    public class SequenceStatusUI : UIState
    {
        private UIImage sequenceIcon;
        private string currentTexturePath = ""; // 缓存路径防止闪烁

        public override void OnInitialize()
        {
            sequenceIcon = new UIImage(ModContent.Request<Texture2D>("zhashi/Content/Items/Potions/Hunter/HunterPotion"));
            sequenceIcon.Left.Set(20, 0f);
            sequenceIcon.Top.Set(80, 0f);
            sequenceIcon.Width.Set(44, 0f);
            sequenceIcon.Height.Set(44, 0f);
            Append(sequenceIcon);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            string newPath = GetTexturePath(modPlayer);

            // 只有路径改变时才重新加载图片，解决闪烁问题
            if (newPath != currentTexturePath)
            {
                currentTexturePath = newPath;

                if (!string.IsNullOrEmpty(newPath))
                {
                    sequenceIcon.SetImage(ModContent.Request<Texture2D>(newPath, ReLogic.Content.AssetRequestMode.ImmediateLoad));
                    sequenceIcon.Color = Color.White;
                }
                else
                {
                    sequenceIcon.SetImage(ModContent.Request<Texture2D>("zhashi/Content/Items/Potions/Hunter/HunterPotion"));
                    sequenceIcon.Color = Color.Transparent;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (sequenceIcon.IsMouseHovering && !string.IsNullOrEmpty(currentTexturePath))
            {
                Player player = Main.LocalPlayer;
                var modPlayer = player.GetModPlayer<LotMPlayer>();

                string tooltip = GetTooltipText(modPlayer);

                if (!string.IsNullOrEmpty(tooltip))
                {
                    Main.instance.MouseText(tooltip);
                }
            }
        }

        private string GetTexturePath(LotMPlayer modPlayer)
        {
            if (modPlayer.currentHunterSequence <= 9)
            {
                string baseP = "zhashi/Content/Items/Potions/Hunter/";
                switch (modPlayer.currentHunterSequence)
                {
                    case 9: return baseP + "HunterPotion";
                    case 8: return baseP + "ProvokerPotion";
                    case 7: return baseP + "PyromaniacPotion";
                    case 6: return baseP + "ConspiratorPotion";
                    case 5: return baseP + "ReaperPotion";
                    case 4: return baseP + "IronBloodedKnightPotion";
                    case 3: return baseP + "WarBishopPotion";
                    case 2: return baseP + "WeatherWarlockPotion";
                    case 1: return baseP + "ConquerorPotion";
                    default: return "";
                }
            }
            else if (modPlayer.currentSequence <= 9)
            {
                string baseP = "zhashi/Content/Items/Potions/";
                switch (modPlayer.currentSequence)
                {
                    case 9: return baseP + "WarriorPotion";
                    case 8: return baseP + "PugilistPotion";
                    case 7: return baseP + "WeaponMasterPotion";
                    case 6: return baseP + "DawnKnightPotion";
                    case 5: return baseP + "GuardianPotion";
                    case 4: return baseP + "DemonHunterPotion";
                    case 3: return baseP + "SilverKnightPotion";
                    case 2: return baseP + "GloryPotion";
                    // case 1: return baseP + "TwilightGiantPotion"; // 预留
                    default: return "";
                }
            }
            return "";
        }

        // 【究极完整版数值说明】
        private string GetTooltipText(LotMPlayer modPlayer)
        {
            // --- 猎人途径 ---
            if (modPlayer.currentHunterSequence <= 9)
            {
                switch (modPlayer.currentHunterSequence)
                {
                    case 9:
                        return "[c/FF4500:序列9：猎人]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 移动速度 +15%\n" +
                               "• 远程伤害 +10%\n" +
                               "• 近战伤害 +5%\n" +
                               "[c/00FFFF:特殊能力]\n" +
                               "• 危险感知 & 生物探测";

                    case 8:
                        return "[c/FF4500:序列8：挑衅者]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 防御力 +8\n" +
                               "• 仇恨值 +300 (更容易被攻击)\n" +
                               "• 生命回复 +2\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 挑衅 (F键): 激怒周围敌人";

                    case 7:
                        return "[c/FF4500:序列7：纵火家]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 全伤害 +15%\n" +
                               "[c/00FFFF:状态免疫]\n" +
                               "• 免疫着火、狱火、霜火、寒冷\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 操纵火焰 (G键): 武器附魔\n" +
                               "• 火球术 (按住G): 蓄力发射爆裂火球";

                    case 6:
                        return "[c/FF4500:序列6：阴谋家]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 暴击率 +10%\n" +
                               "• 魔力消耗 -15%\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 火焰闪现 (V键): 瞬间移动至光标处\n" +
                               $"  (当前状态: {(modPlayer.fireTeleportCooldown > 0 ? $"[c/FF0000:冷却 {modPlayer.fireTeleportCooldown / 60}s]" : "[c/00FF00:就绪]")})";

                    case 5:
                        return "[c/FF4500:序列5：收割者]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 护甲穿透 +20\n" +
                               "• 暴击率 +15%\n" +
                               "• 暴击伤害 +50%\n" +
                               "[c/FFA500:被动技能]\n" +
                               "• 弱点收割: 命中非Boss敌人时，\n" +
                               "  若血量低于20%直接斩杀";

                    case 4:
                        return "[c/FF4500:序列4：铁血骑士]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 防御力 +40 / 免伤 +15%\n" +
                               "• 召唤栏 +2 / 免疫击退\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 火焰化 (K键): 元素化身，大幅提升机动性，\n" +
                               "  但无法使用物品\n" +
                               "• 集众 (Z键): 消耗灵性增强召唤物";

                    case 3:
                        return "[c/FF4500:序列3：战争主教]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 召唤伤害 +30%\n" +
                               "• 召唤栏 +3 / 哨兵栏 +2\n" +
                               "[c/00FFFF:特殊能力]\n" +
                               "• 战争光环: 提升周围队友能力\n" +
                               "• 天气掌控资格 (可使用天气符文)";

                    case 2:
                        return "[c/FF4500:序列2：天气术士]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 生命上限 +500 / 魔力上限 +200\n" +
                               "• 全伤害 +30%\n" +
                               "• 免疫风推与强风\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 灾祸巨人 (K键): 化身风暴巨人\n" +
                               "• 召唤雷击 (U键): 消耗200灵性\n" +
                               $"• 冰河世纪 (I键): 全屏冻结 {(modPlayer.glacierCooldown > 0 ? $"[c/FF0000:({modPlayer.glacierCooldown / 60}s)]" : "[c/00FF00:(就绪)]")}";

                    case 1:
                        return "[c/FF00FF:序列1：征服者]\n" +
                               "----------------\n" +
                               "[c/FF0000:征服权柄]\n" +
                               "• 全图威压: 普通怪物停止自然刷新\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 防御力 +80 / 免伤 +30%\n" +
                               "• 全伤害 +80% / 暴击率 +30%\n" +
                               "• 仇恨值 +2000 (绝对仇恨)\n" +
                               "• 免疫破甲、虚弱、萎缩\n" +
                               "[c/FFFF00:特殊技]\n" +
                               "• 紫焰长枪: 火焰化(K键)冲锋造成巨额伤害";
                }
            }
            // --- 巨人/战士途径 ---
            else if (modPlayer.currentSequence <= 9)
            {
                switch (modPlayer.currentSequence)
                {
                    case 9:
                        return "[c/C0C0C0:序列9：战士]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 防御力 +5\n" +
                               "• 近战伤害 +10% / 近战暴击 +5%\n" +
                               "• 移动速度 +10%\n" +
                               "[c/00FFFF:被动]\n" +
                               "• 武器大师雏形: 熟练度提升";

                    case 8:
                        return "[c/C0C0C0:序列8：格斗家]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 近战攻速 +10%\n" +
                               "• 伤害减免 +5%\n" +
                               "• 免疫击退";

                    case 7:
                        return "[c/C0C0C0:序列7：武器大师]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 全伤害 +10%\n" +
                               "• 暴击率 +5%\n" +
                               "• 护甲穿透 +5\n" +
                               "[c/00FFFF:被动]\n" +
                               "• 极致的武器运用技巧";

                    case 6:
                        return "[c/C0C0C0:序列6：黎明骑士]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 防御力 +10 / 生命回复 +2\n" +
                               "• 自身散发光芒\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 黎明铠甲 (C键): 召唤光之护盾\n" +
                               $"  状态: {(modPlayer.dawnArmorBroken ? $"[c/FF0000:破碎 ({modPlayer.dawnArmorCooldownTimer / 60}s)]" : (modPlayer.dawnArmorActive ? "[c/00FF00:激活中]" : "[c/FFFF00:就绪]"))}";

                    case 5:
                        return "[c/C0C0C0:序列5：守护者]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 防御力 +15 / 伤害减免 +5%\n" +
                               "• 免疫困惑、黑暗、沉默\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 守护姿态 (按住Z): 举盾防御，\n" +
                               "  并为周围队友承担伤害";

                    case 4:
                        return "[c/C0C0C0:序列4：猎魔者]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 生命上限 +100\n" +
                               "• 全伤害 +15% / 暴击率 +10%\n" +
                               "• 获得夜视、生物探测\n" +
                               "[c/00FFFF:状态免疫]\n" +
                               "• 免疫诅咒狱火、暗影焰";

                    case 3:
                        return "[c/C0C0C0:序列3：银骑士]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 防御力 +20 / 生命回复 +5\n" +
                               "• 近战攻速 +15%\n" +
                               "• 获得闪避能力 (黑带)\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 水银化 (X键): 全身液态化，免疫物理攻击\n" +
                               "  但无法攻击，持续消耗灵性";

                    case 2:
                        return "[c/C0C0C0:序列2：荣耀者]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 生命上限 +400\n" +
                               "• 防御力 +30 / 伤害减免 +10%\n" +
                               "• 全伤害 +20%\n" +
                               "[c/FFA500:黄昏权柄]\n" +
                               "• 拒绝死亡: 受到致命伤时完全复活\n" +
                               $"  冷却时间: {(modPlayer.twilightResurrectionCooldown > 0 ? $"[c/FF0000:{modPlayer.twilightResurrectionCooldown / 60}s]" : "[c/00FF00:就绪]")}";

                    case 1:
                        return "[c/C0C0C0:序列1：黄昏巨人]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 生命上限 +1000\n" +
                               "• 防御力 +100 / 伤害减免 +20%\n" +
                               "• 全伤害 +50%\n" +
                               "[c/00FFFF:被动]\n" +
                               "• 体型巨大化，俯视众生";
                }
            }
            return "";
        }
    }

    public class SequenceUISystem : ModSystem
    {
        internal SequenceStatusUI MyUIState;
        private UserInterface _myUserInterface;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                MyUIState = new SequenceStatusUI();
                MyUIState.Activate();
                _myUserInterface = new UserInterface();
                _myUserInterface.SetState(MyUIState);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _myUserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "zhashi: Sequence UI",
                    delegate {
                        _myUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}