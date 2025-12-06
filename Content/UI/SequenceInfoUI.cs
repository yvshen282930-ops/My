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
        private string currentTexturePath = "";

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

        private string GetTexturePath(LotMPlayer modPlayer)
        {
            string baseHunter = "zhashi/Content/Items/Potions/Hunter/";
            string baseWarrior = "zhashi/Content/Items/Potions/";
            string baseMoon = "zhashi/Content/Items/Potions/Moon/";

            // --- 月亮途径 ---
            if (modPlayer.currentMoonSequence <= 9)
            {
                switch (modPlayer.currentMoonSequence)
                {
                    case 9: return baseMoon + "ApothecaryPotion";
                    case 8: return baseMoon + "BeastTamerPotion";
                    case 7: return baseMoon + "VampirePotion";
                    case 6: return baseMoon + "PotionsProfessorPotion";
                    case 5: return baseMoon + "ScarletScholarPotion";
                    // 【核心修复】这里之前写成了 ShamanKingPotion，现已更正为 WitchKingPotion
                    case 4: return baseMoon + "WitchKingPotion";
                    case 3: return baseMoon + "SummoningMasterPotion";
                    case 2: return baseMoon + "LifeGiverPotion";
                    case 1: return baseMoon + "BeautyGoddessPotion";
                    default: return "";
                }
            }
            // --- 猎人途径 ---
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
            // --- 巨人途径 ---
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
                    default: return "";
                }
            }
            return "";
        }

        private string GetTooltipText(LotMPlayer modPlayer)
        {
            if (modPlayer.currentMoonSequence <= 9)
            {
                switch (modPlayer.currentMoonSequence)
                {
                    case 9:
                        return "[c/00FF7F:序列9：药师]\n----------------\n[c/00FF00:属性]\n• 生命上限 +20\n• 回复 +2/s\n[c/00FFFF:被动]\n• 抗毒 & 灵视";
                    case 8:
                        return "[c/00FF7F:序列8：驯兽师]\n----------------\n[c/00FF00:属性]\n• 力量敏捷提升\n[c/FFFF00:技能]\n• 驯化: 鞭打残血生物";
                    case 7:
                        return "[c/FF0000:序列7：吸血鬼]\n----------------\n[c/00FF00:属性]\n• 超强自愈与速度\n[c/FFFF00:技能]\n• K键: 黑暗之翼\n• V键: 深渊枷锁\n[c/FFA500:被动]\n• 腐蚀之爪 & 厌恶阳光";
                    case 6:
                        return "[c/00FF7F:序列6：魔药教授]\n----------------\n[c/00FF00:属性]\n• 魔法伤害 +15%\n[c/FFFF00:技能]\n• X键: 炼金手雷\n• Z键: 生命灵液\n[c/00FFFF:被动]\n• 药剂双倍 & 炼金石";
                    case 5:
                        return "[c/DC143C:序列5：深红学者]\n----------------\n[c/00FF00:属性]\n• 免疫精神控制\n[c/FFFF00:满月技]\n• G键: 开启满月领域\n• V键(满月): 闪现\n• C键: 月光化(无敌)";
                    case 4:
                        return "[c/8B0000:序列4：巫王]\n----------------\n[c/FF0000:半神]\n• 寿命极长\n• 召唤/魔法伤害 +20%\n[c/FFFF00:技能]\n• T键: 蝙蝠化身 (无敌飞行)\n• J键: 月亮纸人 (替身)\n• F键: 黑暗凝视 (高伤)";
                        // ... (序列3-1 预留)
                }
            }
            else if (modPlayer.currentHunterSequence <= 9)
            {
                switch (modPlayer.currentHunterSequence)
                {
                    case 9: return "[c/FF4500:序列9：猎人]\n----------------\n[c/00FF00:属性]\n• 移速+15% / 远程伤+10%\n[c/00FFFF:能力]\n• 危险感知 & 生物探测";
                    case 8: return "[c/FF4500:序列8：挑衅者]\n----------------\n[c/00FF00:属性]\n• 防御+8 / 仇恨+300\n[c/FFFF00:技能]\n• 挑衅 (F键)";
                    case 7: return "[c/FF4500:序列7：纵火家]\n----------------\n[c/00FF00:属性]\n• 全伤+15% / 免疫火焰\n[c/FFFF00:技能]\n• 火球术 (按住G)";
                    case 6: return "[c/FF4500:序列6：阴谋家]\n----------------\n[c/00FF00:属性]\n• 暴击+10%\n[c/FFFF00:技能]\n• 闪现 (V键)";
                    case 5: return "[c/FF4500:序列5：收割者]\n----------------\n[c/00FF00:属性]\n• 破甲+20 / 暴伤+50%\n[c/FFA500:被动]\n• 弱点收割";
                    case 4: return "[c/FF4500:序列4：铁血骑士]\n----------------\n[c/00FF00:属性]\n• 防御+40 / 免伤+15%\n[c/FFFF00:技能]\n• 火焰化 (K键)\n• 集众 (Z键)";
                    case 3: return "[c/FF4500:序列3：战争主教]\n----------------\n[c/00FF00:属性]\n• 召唤伤+30%\n[c/00FFFF:能力]\n• 战争光环";
                    case 2: return "[c/FF4500:序列2：天气术士]\n----------------\n[c/00FF00:属性]\n• 生命+500 / 全伤+30%\n[c/FFFF00:技能]\n• 灾祸巨人 (K键)\n• 雷击(U) / 冰河(I)";
                    case 1: return "[c/FF00FF:序列1：征服者]\n----------------\n[c/FF0000:征服权柄]\n• 怪物停止刷新\n[c/00FF00:属性]\n• 攻防大幅提升\n[c/FFFF00:技]\n• 紫焰长枪冲锋";
                }
            }
            else if (modPlayer.currentSequence <= 9)
            {
                switch (modPlayer.currentSequence)
                {
                    case 9: return "[c/C0C0C0:序列9：战士]\n----------------\n• 防御+5 / 近战伤+10%";
                    case 8: return "[c/C0C0C0:序列8：格斗家]\n----------------\n• 攻速+10% / 免疫击退";
                    case 7: return "[c/C0C0C0:序列7：武器大师]\n----------------\n• 全伤+10% / 破甲+5";
                    case 6: return "[c/C0C0C0:序列6：黎明骑士]\n----------------\n• 自身发光 / 防御+10\n• 技能: 黎明铠甲 (C键)";
                    case 5: return "[c/C0C0C0:序列5：守护者]\n----------------\n• 防御+15 / 免伤+5%\n• 技能: 守护姿态 (按住Z)";
                    case 4: return "[c/C0C0C0:序列4：猎魔者]\n----------------\n• 免疫诅咒、暗影焰\n• 获得夜视";
                    case 3: return "[c/C0C0C0:序列3：银骑士]\n----------------\n• 获得闪避能力\n• 技能: 水银化 (C键)";
                    case 2: return "[c/C0C0C0:序列2：荣耀者]\n----------------\n• 生命+400 / 防御+30\n• 被动: 黄昏复活 (5分冷却)";
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