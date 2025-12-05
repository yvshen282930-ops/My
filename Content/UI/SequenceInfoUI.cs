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
            // 初始默认图
            sequenceIcon = new UIImage(ModContent.Request<Texture2D>("zhashi/Content/Items/Potions/Hunter/HunterPotion"));

            // UI 位置设定 (左上角)
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

            // 获取当前应该显示的图标路径
            string newPath = GetTexturePath(modPlayer);

            // 【核心优化】只有当路径真正改变时才重新加载图片
            // 这彻底解决了图标闪烁和消失的问题
            if (newPath != currentTexturePath)
            {
                currentTexturePath = newPath;

                if (!string.IsNullOrEmpty(newPath))
                {
                    // 使用 ImmediateLoad 确保切换瞬间不留白
                    sequenceIcon.SetImage(ModContent.Request<Texture2D>(newPath, ReLogic.Content.AssetRequestMode.ImmediateLoad));
                    sequenceIcon.Color = Color.White;
                }
                else
                {
                    // 如果没有对应序列，隐藏图标
                    sequenceIcon.SetImage(ModContent.Request<Texture2D>("zhashi/Content/Items/Potions/Hunter/HunterPotion"));
                    sequenceIcon.Color = Color.Transparent;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // 鼠标悬停显示详细数值面板
            if (sequenceIcon.IsMouseHovering && !string.IsNullOrEmpty(currentTexturePath) && sequenceIcon.Color == Color.White)
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

        // ========================================================================
        // 1. 图标路径管理 (在这里添加新途径的图片路径)
        // ========================================================================
        private string GetTexturePath(LotMPlayer modPlayer)
        {
            string baseHunter = "zhashi/Content/Items/Potions/Hunter/";
            string baseWarrior = "zhashi/Content/Items/Potions/"; // 假设战士魔药在根目录
            string baseMoon = "zhashi/Content/Items/Potions/Moon/"; // 月亮魔药目录

            // --- 月亮途径 (Moon) ---
            if (modPlayer.currentMoonSequence <= 9)
            {
                switch (modPlayer.currentMoonSequence)
                {
                    case 9: return baseMoon + "ApothecaryPotion";
                    case 8: return baseMoon + "BeastTamerPotion";
                    case 7: return baseMoon + "VampirePotion";
                    case 6: return baseMoon + "PotionsProfessorPotion";
                    case 5: return baseMoon + "ScarletScholarPotion";
                    case 4: return baseMoon + "ShamanKingPotion";
                    case 3: return baseMoon + "SummoningMasterPotion";
                    case 2: return baseMoon + "LifeGiverPotion";
                    case 1: return baseMoon + "BeautyGoddessPotion";
                    default: return "";
                }
            }
            // --- 猎人途径 (Hunter) ---
            else if (modPlayer.currentHunterSequence <= 9)
            {
                switch (modPlayer.currentHunterSequence)
                {
                    case 9: return baseHunter + "HunterPotion";
                    case 8: return baseHunter + "ProvokerPotion";
                    case 7: return baseHunter + "PyromaniacPotion";
                    case 6: return baseHunter + "ConspiratorPotion";
                    case 5: return baseHunter + "ReaperPotion";
                    case 4: return baseHunter + "IronBloodedKnightPotion";
                    case 3: return baseHunter + "WarBishopPotion";
                    case 2: return baseHunter + "WeatherWarlockPotion";
                    case 1: return baseHunter + "ConquerorPotion";
                    default: return "";
                }
            }
            // --- 巨人/战士途径 (Warrior) ---
            else if (modPlayer.currentSequence <= 9)
            {
                switch (modPlayer.currentSequence)
                {
                    case 9: return baseWarrior + "WarriorPotion";
                    case 8: return baseWarrior + "PugilistPotion";
                    case 7: return baseWarrior + "WeaponMasterPotion";
                    case 6: return baseWarrior + "DawnKnightPotion";
                    case 5: return baseWarrior + "GuardianPotion";
                    case 4: return baseWarrior + "DemonHunterPotion";
                    case 3: return baseWarrior + "SilverKnightPotion";
                    case 2: return baseWarrior + "GloryPotion";
                    // case 1: return baseWarrior + "TwilightGiantPotion";
                    default: return "";
                }
            }
            return "";
        }

        // ========================================================================
        // 2. 详细数值与技能说明 (UI 显示的核心)
        // ========================================================================
        private string GetTooltipText(LotMPlayer modPlayer)
        {
            // --- 月亮途径 (Moon) ---
            if (modPlayer.currentMoonSequence <= 9)
            {
                switch (modPlayer.currentMoonSequence)
                {
                    case 9:
                        return "[c/00FF7F:序列9：药师]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 生命上限 +20\n" +
                               "• 生命回复 +2/s\n" +
                               "[c/00FFFF:被动能力]\n" +
                               "• 抗毒体质: 免疫中毒与毒液\n" +
                               "• 健康灵视: 获得生物探测能力";

                    case 8:
                        return "[c/00FF7F:序列8：驯兽师]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 力量与敏捷大幅提升\n" +
                               "[c/FFFF00:主动技能]\n" +
                               "• 驯化 (使用鞭子): 击打半血以下的非Boss生物\n" +
                               "  可将其驯服为友军，持续10分钟\n" +
                               "[c/00FFFF:被动能力]\n" +
                               "• 动物感官: 获得危险感知";

                    case 7:
                        return "[c/FF0000:序列7：吸血鬼]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 寿命/速度/自愈能力 极大提升\n" +
                               "• 免疫坠落伤害\n" +
                               "[c/FFFF00:黑暗法术]\n" +
                               "• 黑暗之翼: 短暂飞行与黑焰攻击\n" +
                               "• 腐蚀之爪: 近战附加破甲与腐蚀\n" +
                               "• 血仆转化: 强化并奴役人形敌人";

                    case 6:
                        return "[c/00FF7F:序列6：魔药教授]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 全属性均衡提升\n" +
                               "[c/00FFFF:核心能力]\n" +
                               "• 灵性辨识: 看到稀有材料的微光\n" +
                               "• 药剂调配: 可制作【隐身】【火龙吐息】\n" +
                               "  等特殊战斗药剂";

                    case 5:
                        return "[c/DC143C:序列5：深红学者]\n" +
                               "----------------\n" +
                               "[c/00FF00:属性加成]\n" +
                               "• 获得难以想象的恢复力\n" +
                               "• 免疫梦魇与精神控制\n" +
                               "[c/FFFF00:月光法术]\n" +
                               "• 满月领域: 制造利于灵性的环境\n" +
                               "• 月光化 (X键): 化身月光免疫物理攻击\n" +
                               "• 闪现: 在月光范围内瞬移";

                    case 4:
                        return "[c/8B0000:序列4：巫王]\n" +
                               "----------------\n" +
                               "[c/FF0000:半神位格]\n" +
                               "• 寿命突破1000年\n" +
                               "[c/FFFF00:诡异法术]\n" +
                               "• 替身法术: 使用月亮纸人抵挡致命伤\n" +
                               "• 黑暗凝视: 摧毁视线内目标的身体部件\n" +
                               "• 蝙蝠化身: 分解为蝙蝠群重组身体";

                    case 3:
                        return "[c/4B0082:序列3：召唤大师]\n" +
                               "----------------\n" +
                               "[c/00FFFF:召唤权柄]\n" +
                               "• 召唤之门: 从灵界召唤随机强大生物助战\n" +
                               "• 天使契约: 极低概率召唤天使投影\n" +
                               "[c/FF0000:警告]\n" +
                               "• 召唤可能引来未知的邪恶存在";

                    case 2:
                        return "[c/FF1493:序列2：创生者]\n" +
                               "----------------\n" +
                               "[c/00FF00:创造权柄]\n" +
                               "• 身体素质达到顶点\n" +
                               "• 治愈光环: 瞬间治愈周围所有友军\n" +
                               "• 赋予生命: 让无机物（如石头）活化\n" +
                               "• 灵性干扰: 强制耗尽敌人的灵性";

                    case 1:
                        return "[c/FF69B4:序列1：美神]\n" +
                               "----------------\n" +
                               "[c/FF00FF:美之权柄]\n" +
                               "• 魅惑众生: 这种美足以让低位格生物瘫痪\n" +
                               "• 强制驯服: 没有任何生物能拒绝你的命令\n" +
                               "• 性别转换: 获得代表阴性力量的完美形态";
                }
            }

            // --- 猎人途径 (Hunter) ---
            else if (modPlayer.currentHunterSequence <= 9)
            {
                switch (modPlayer.currentHunterSequence)
                {
                    case 9:
                        return "[c/FF4500:序列9：猎人]\n----------------\n[c/00FF00:属性]\n• 移速+15% / 远程伤+10%\n[c/00FFFF:能力]\n• 危险感知 & 生物探测";
                    case 8:
                        return "[c/FF4500:序列8：挑衅者]\n----------------\n[c/00FF00:属性]\n• 防御+8 / 仇恨+300\n[c/FFFF00:技能]\n• 挑衅 (F键): 激怒敌人";
                    case 7:
                        return "[c/FF4500:序列7：纵火家]\n----------------\n[c/00FF00:属性]\n• 全伤+15% / 免疫火焰\n[c/FFFF00:技能]\n• 火球术 (按住G): 蓄力爆炸";
                    case 6:
                        return "[c/FF4500:序列6：阴谋家]\n----------------\n[c/00FF00:属性]\n• 暴击+10%\n[c/FFFF00:技能]\n• 火焰闪现 (V键): 瞬间移动";
                    case 5:
                        return "[c/FF4500:序列5：收割者]\n----------------\n[c/00FF00:属性]\n• 破甲+20 / 暴伤+50%\n[c/FFA500:被动]\n• 弱点收割: 斩杀低血量非Boss";
                    case 4:
                        return "[c/FF4500:序列4：铁血骑士]\n----------------\n[c/00FF00:属性]\n• 防御+40 / 免伤+15%\n[c/FFFF00:技能]\n• 火焰化 (K键): 元素化身\n• 集众 (Z键): 强化召唤物";
                    case 3:
                        return "[c/FF4500:序列3：战争主教]\n----------------\n[c/00FF00:属性]\n• 召唤伤+30%\n[c/00FFFF:能力]\n• 战争光环 & 天气掌控";
                    case 2:
                        return "[c/FF4500:序列2：天气术士]\n----------------\n[c/00FF00:属性]\n• 生命+500 / 全伤+30%\n[c/FFFF00:技能]\n• 灾祸巨人 (K键)\n• 雷击(U) / 冰河(I)";
                    case 1:
                        return "[c/FF00FF:序列1：征服者]\n----------------\n[c/FF0000:征服权柄]\n• 怪物停止刷新 / 绝对仇恨\n[c/00FF00:属性]\n• 攻防大幅提升\n[c/FFFF00:技]\n• 紫焰长枪冲锋";
                }
            }

            // --- 巨人/战士途径 (Warrior) ---
            else if (modPlayer.currentSequence <= 9)
            {
                switch (modPlayer.currentSequence)
                {
                    case 9:
                        return "[c/C0C0C0:序列9：战士]\n----------------\n• 防御+5 / 近战伤+10%";
                    case 8:
                        return "[c/C0C0C0:序列8：格斗家]\n----------------\n• 攻速+10% / 免疫击退";
                    case 7:
                        return "[c/C0C0C0:序列7：武器大师]\n----------------\n• 全伤+10% / 破甲+5";
                    case 6:
                        return "[c/C0C0C0:序列6：黎明骑士]\n----------------\n• 自身发光 / 防御+10\n• 技能: 黎明铠甲 (C键)";
                    case 5:
                        return "[c/C0C0C0:序列5：守护者]\n----------------\n• 防御+15 / 免伤+5%\n• 技能: 守护姿态 (按住Z)";
                    case 4:
                        return "[c/C0C0C0:序列4：猎魔者]\n----------------\n• 免疫诅咒、暗影焰\n• 获得夜视";
                    case 3:
                        return "[c/C0C0C0:序列3：银骑士]\n----------------\n• 获得闪避能力\n• 技能: 水银化 (X键)";
                    case 2:
                        return "[c/C0C0C0:序列2：荣耀者]\n----------------\n• 生命+400 / 防御+30\n• 被动: 黄昏复活 (5分冷却)";
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