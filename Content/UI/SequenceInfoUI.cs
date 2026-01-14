using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI.Chat;
using Terraria.ModLoader;
using Terraria.UI;
using System;
using Terraria.ID;
using zhashi.Content;

// 引用魔药命名空间
using zhashi.Content.Items.Potions;
using zhashi.Content.Items.Potions.Hunter;
using zhashi.Content.Items.Potions.Moon;
using zhashi.Content.Items.Potions.Fool;
using zhashi.Content.Items.Potions.Marauder;
using zhashi.Content.Items.Potions.Sun;

namespace zhashi.Content.UI
{
    public class SequenceInfoUI : UIState
    {
        private const int ICON_SIZE = 44;
        private const int LEFT_MARGIN = 20;
        private const int TOP_MARGIN = 120;

        private UIPanel _infoPanel;
        private UIList _textList;
        private UIScrollbar _scrollbar;

        private string _cachedText = "";
        private int _lastPotionId = -1;
        private int _hoverTimer = 0; // 悬停缓冲计时器

        public override void OnInitialize()
        {
            _infoPanel = new UIPanel();
            _infoPanel.SetPadding(15);
            // 透明黑色背景，无边框
            _infoPanel.BackgroundColor = new Color(0, 0, 0) * 0.6f;
            _infoPanel.BorderColor = Color.Transparent;

            _infoPanel.Left.Set(-9999, 0f); // 默认隐藏
            _infoPanel.Top.Set(TOP_MARGIN, 0f);
            _infoPanel.Width.Set(450, 0f);
            _infoPanel.Height.Set(400, 0f);

            _scrollbar = new UIScrollbar();
            _scrollbar.Height.Set(0, 1f);
            _scrollbar.Left.Set(-10, 1f);
            _scrollbar.Top.Set(0, 0f);

            _textList = new UIList();
            _textList.Width.Set(-25, 1f);
            _textList.Height.Set(0, 1f);
            _textList.ListPadding = 5f;
            _textList.SetScrollbar(_scrollbar);

            _infoPanel.Append(_textList);
            _infoPanel.Append(_scrollbar);

            Append(_infoPanel);
        }

        private class UIColoredText : UIElement
        {
            private string _text;
            public UIColoredText(string text) { SetText(text); }
            public void SetText(string text)
            {
                _text = text;
                Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, _text, Vector2.One);
                this.Width.Set(size.X, 0f);
                this.Height.Set(size.Y, 0f);
            }
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (string.IsNullOrEmpty(_text)) return;
                CalculatedStyle dimensions = GetInnerDimensions();
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, _text, dimensions.Position(), Color.White, 0f, Vector2.Zero, Vector2.One);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active || player.dead || player.ghost) return;

            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            if (!modPlayer.IsBeyonder) return;

            int itemType = GetCurrentSequencePotionID(modPlayer);
            if (itemType <= 0) return;

            // 1. 绘制图标
            Main.instance.LoadItem(itemType);
            Texture2D texture = TextureAssets.Item[itemType].Value;
            Vector2 drawPos = new Vector2(LEFT_MARGIN, TOP_MARGIN);
            Rectangle iconRect = new Rectangle((int)drawPos.X, (int)drawPos.Y, ICON_SIZE, ICON_SIZE);

            Utils.DrawInvBG(spriteBatch, iconRect, new Color(20, 20, 40, 100));

            Rectangle frame = texture.Frame();
            float scale = 1f;
            if (frame.Width > 32 || frame.Height > 32) scale = 0.8f;
            Vector2 origin = frame.Size() / 2f;
            Vector2 center = drawPos + new Vector2(ICON_SIZE / 2f, ICON_SIZE / 2f);
            spriteBatch.Draw(texture, center, frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

            // --- 2. 悬停检测逻辑 ---
            Point mousePoint = Main.MouseScreen.ToPoint();
            bool isHoveringIcon = iconRect.Contains(mousePoint);

            // 如果鼠标在图标上，给计时器充能，并定位面板
            if (isHoveringIcon)
            {
                _infoPanel.Left.Set(LEFT_MARGIN + ICON_SIZE + 2, 0f);
                _infoPanel.Recalculate();
                _hoverTimer = 15; // 续命 0.25秒
            }

            // 如果鼠标在面板上，持续充能
            if (_infoPanel.IsMouseHovering)
            {
                _hoverTimer = 15;
                player.mouseInterface = true;
            }

            // --- 3. 面板内容更新逻辑 (修复Bug的核心) ---
            if (_hoverTimer > 0)
            {
                _hoverTimer--;

                // [修复]：每一帧都必须获取最新文本，无论鼠标在哪里
                // 这样才能实时显示冷却时间变化 (5s -> 4s) 和技能开启状态
                string fullText = GetSequenceDescription(modPlayer);

                // 只有当文本内容真的变了才去操作UI，节省性能
                if (fullText != _cachedText)
                {
                    bool isSamePotion = (itemType == _lastPotionId);

                    // 如果是同一个魔药（只是倒计时变了），记录当前滚动条位置
                    float previousScroll = _scrollbar.ViewPosition;

                    _cachedText = fullText;
                    _lastPotionId = itemType;

                    _textList.Clear();
                    _textList.Add(new UIColoredText(_cachedText));

                    // 强制刷新列表布局，让滚动条知道新内容有多高
                    _textList.Recalculate();

                    // 如果魔药没变，恢复之前的滚动位置（防止倒计时刷新时画面跳动）
                    // 如果换了魔药，就自动回到顶部
                    if (isSamePotion)
                    {
                        _scrollbar.ViewPosition = previousScroll;
                    }
                    else
                    {
                        _scrollbar.ViewPosition = 0f;
                    }
                }

                _infoPanel.Recalculate();
                base.Draw(spriteBatch);
            }
            else
            {
                _infoPanel.Left.Set(-9999, 0f); // 隐藏
            }
        }

        // --- 辅助方法：获取当前序列对应的魔药 ID ---
        private int GetCurrentSequencePotionID(LotMPlayer p)
        {
            // 优先检测低序列 (数值越小代表序列越高)

            // 1. 猎人途径
            if (p.currentHunterSequence <= 9)
            {
                switch (p.currentHunterSequence)
                {
                    case 9: return ModContent.ItemType<HunterPotion>();
                    case 8: return ModContent.ItemType<ProvokerPotion>();
                    case 7: return ModContent.ItemType<PyromaniacPotion>();
                    case 6: return ModContent.ItemType<ConspiratorPotion>();
                    case 5: return ModContent.ItemType<ReaperPotion>();
                    case 4: return ModContent.ItemType<IronBloodedKnightPotion>();
                    case 3: return ModContent.ItemType<WarBishopPotion>();
                    case 2: return ModContent.ItemType<WeatherWarlockPotion>();
                    case 1: return ModContent.ItemType<ConquerorPotion>();
                    default: return ModContent.ItemType<HunterPotion>();
                }
            }
            // 2. 巨人途径
            if (p.currentSequence <= 9)
            {
                switch (p.currentSequence)
                {
                    case 9: return ModContent.ItemType<WarriorPotion>();
                    case 8: return ModContent.ItemType<PugilistPotion>();
                    case 7: return ModContent.ItemType<WeaponMasterPotion>();
                    case 6: return ModContent.ItemType<DawnKnightPotion>();
                    case 5: return ModContent.ItemType<GuardianPotion>();
                    case 4: return ModContent.ItemType<DemonHunterPotion>();
                    case 3: return ModContent.ItemType<SilverKnightPotion>();
                    case 2: return ModContent.ItemType<GloryPotion>();
                    case 1: return ModContent.ItemType<HandOfGodPotion>();
                    default: return ModContent.ItemType<WarriorPotion>();
                }
            }
            // 3. 月亮途径
            if (p.currentMoonSequence <= 9)
            {
                switch (p.currentMoonSequence)
                {
                    case 9: return ModContent.ItemType<ApothecaryPotion>();
                    case 8: return ModContent.ItemType<BeastTamerPotion>();
                    case 7: return ModContent.ItemType<VampirePotion>();
                    case 6: return ModContent.ItemType<PotionsProfessorPotion>();
                    case 5: return ModContent.ItemType<ScarletScholarPotion>();
                    case 4: return ModContent.ItemType<WitchKingPotion>();
                    case 3: return ModContent.ItemType<SummoningMasterPotion>();
                    case 2: return ModContent.ItemType<LifeGiverPotion>();
                    case 1: return ModContent.ItemType<BeautyGoddessPotion>();
                    default: return ModContent.ItemType<ApothecaryPotion>();
                }
            }
            // 4. 愚者途径
            if (p.currentFoolSequence <= 9)
            {
                switch (p.currentFoolSequence)
                {
                    case 9: return ModContent.ItemType<SeerPotion>();
                    case 8: return ModContent.ItemType<ClownPotion>();
                    case 7: return ModContent.ItemType<MagicianPotion>();
                    case 6: return ModContent.ItemType<FacelessPotion>();
                    case 5: return ModContent.ItemType<MarionettistPotion>();
                    case 4: return ModContent.ItemType<BizarroSorcererPotion>();
                    case 3: return ModContent.ItemType<ScholarOfYorePotion>();
                    case 2: return ModContent.ItemType<MiracleInvokerPotion>();
                    case 1: return ModContent.ItemType<AttendantPotion>();
                    default: return ModContent.ItemType<SeerPotion>();
                }
            }
            // 5.错误途径
            if (p.currentMarauderSequence <= 9)
            {
                switch (p.currentMarauderSequence)
                {
                    case 9: return ModContent.ItemType<MarauderPotion>(); 
                    case 8: return ModContent.ItemType<SwindlerPotion>();
                    case 7: return ModContent.ItemType<CryptologistPotion>();
                    case 6: return ModContent.ItemType<PrometheusPotion>(); // [新增]
                    case 5: return ModContent.ItemType<DreamStealerPotion>();
                    case 4: return ModContent.ItemType<ParasitePotion>();
                    case 3: return ModContent.ItemType<MentorPotion>();
                    case 2: return ModContent.ItemType<TrojanHorsePotion>();
                    case 1: return ModContent.ItemType<WormOfTimePotion>();
                    default: return ModContent.ItemType<MarauderPotion>();
                }
            }
            if (p.currentSunSequence <= 9)
            {
                switch (p.currentSunSequence)
                {
                    case 9: return ModContent.ItemType<Content.Items.Potions.Sun.BardPotion>();
                    case 8: return ModContent.ItemType<LightSupplicantPotion>();   // 祈光人
                    case 7: return ModContent.ItemType<SolarHighPriestPotion>();   // 太阳神官
                    case 6: return ModContent.ItemType<NotaryPotion>();            // 公证人
                    case 5: return ModContent.ItemType<PriestPotion>();     // 光之祭司
                    case 4: return ModContent.ItemType<UnshadowedPotion>();        // 无暗者
                    case 3: return ModContent.ItemType<JusticeMentorPotion>();     // 正义导师
                    case 2: return ModContent.ItemType<LightSeekerPotion>();       // 逐光者
                    case 1: return ModContent.ItemType<WhiteAngelPotion>();        // 白天使
                    default: return ModContent.ItemType<Content.Items.Potions.Sun.BardPotion>();
                }
            }
            return 0;
        }

        // --- 辅助方法：生成纯文本介绍 ---
        private string GetSequenceDescription(LotMPlayer p)
        {
            string text = "";

            if (p.currentHunterSequence <= 9)
            {
                text += $"[c/FF4500:猎人途径 序列{p.currentHunterSequence}]\n";
                if (p.currentHunterSequence <= 9) // 猎人
                {
                    text += $"序列九: [c/FFA07A:猎人]\n";
                    text += "- [被动] 远程伤害+15% / 近战伤害+5%\n";
                    text += "- [被动] 生物监测 / 危险感知\n";
                }
                if (p.currentHunterSequence <= 8) // 挑衅者
                {
                    text += $"序列八: [c/FA8072:挑衅者]\n";
                    text += "- [被动] 仇恨值+300 / 防御+10 / 生命回复+2\n";
                }
                if (p.currentHunterSequence <= 7) // 纵火家
                {
                    text += $"序列七: [c/FF6347:纵火家]\n";

                    string cloakStatus = p.isFlameCloakActive ? " [c/00FF00:(开启)]" : "";
                    string chargeStatus = p.isChargingFireball ? " [c/FF0000:(蓄力中...)]" : "";

                    text += $"- [技能] 蓄力火球 (按住技能键){chargeStatus}: 发射高伤火球\n";
                    text += "- [技能] 纵火者炸弹 (按键投掷): 造成范围火焰伤害\n";
                    text += $"- [技能] 火焰披风 (按键切换){cloakStatus}: 免疫寒冷/近身灼烧\n";
                    text += "- [被动] 免疫火焰/燃烧/岩浆\n";
                }
                if (p.currentHunterSequence <= 6) // 阴谋家
                {
                    text += $"序列六: [c/FF4500:阴谋家]\n";

                    string cdFlash = p.fireTeleportCooldown > 0 ? $" [c/FF0000:({p.fireTeleportCooldown / 60}s)]" : "";
                    string enchantStatus = p.isFireEnchanted ? " [c/00FF00:(已附魔)]" : "";

                    text += $"- [技能] 火焰闪现 (按键触发){cdFlash}: 瞬间传送并留下诱饵\n";
                    text += $"- [技能] 武器附魔 (按键切换){enchantStatus}: 赋予武器火焰特效\n";
                    text += "- [被动] 暴击率+15% / 魔力消耗-20%\n";
                }
                if (p.currentHunterSequence <= 5) // 收割者
                {
                    text += $"序列五: [c/DC143C:收割者]\n";

                    text += "- [技能] 收割斩击 (按键释放): 释放大范围火焰斩\n";
                    text += "- [被动] 弱点处决: 对低血量(20%)非Boss敌人造成致死打击\n";
                    text += "- [被动] 护甲穿透+30 / 暴击率+20%\n";
                }
                if (p.currentHunterSequence <= 4) // 铁血骑士
                {
                    text += $"序列四: [c/B22222:铁血骑士]\n";

                    string armyStatus = p.isArmyOfOne ? " [c/00FF00:(集结)]" : "";
                    string formStatus = p.isFireForm ? " [c/00FF00:(元素化)]" : "";

                    text += $"- [技能] 战争集众 (按键切换){armyStatus}: 召唤栏位大幅增加\n";
                    text += $"- [技能] 火焰形态 (按键切换){formStatus}: \n";
                    text += "    > 免疫击退/无限飞行/大幅提升机动性与防御\n";
                    text += "    > 冲刺可对敌人造成毁灭性伤害\n";
                }
                if (p.currentHunterSequence <= 3) // 战争主教
                {
                    text += $"序列三: [c/8B0000:战争主教]\n";
                    text += "- [被动] 召唤物+10 / 哨兵+3 / 召唤伤害+40%\n";
                    text += "- [权柄] 战争领域: 极大增强军队作战能力\n";
                }
                if (p.currentHunterSequence <= 2) // 天气术士
                {
                    text += $"序列二: [c/800000:天气术士]\n";

                    string cdGlacier = p.glacierCooldown > 0 ? $" [c/FF0000:({p.glacierCooldown / 60}s)]" : "";
                    string giantStatus = p.isCalamityGiant ? " [c/00FF00:(化身中)]" : "";

                    text += "- [技能] 闪电风暴 (按键释放): 召唤大范围雷击\n";
                    text += $"- [技能] 冰河世纪 (按键释放){cdGlacier}: 全屏冻结与减速\n";
                    text += $"- [技能] 灾祸巨人 (按键切换){giantStatus}: \n";
                    text += "    > 化身百米高的雷火巨人，防御与生命值质变\n";

                    if (p.currentHunterSequence == 2)
                    {
                        bool isClear = p.conquerorRitualComplete;
                        string status = isClear ? "[已完成]" : "[进行中]";
                        string colorHex = isClear ? "00FF00" : "FFA500";

                        text += $"[c/{colorHex}:[晋升仪式] 征服者: {status}]\n";
                        text += "  (提示: 肃清视野内所有敌人，确立无敌之姿)\n";
                    }
                }
                if (p.currentHunterSequence <= 1) // 征服者
                {
                    text += $"序列一: [c/FF0000:征服者]\n";
                    text += "- [神性] 战争主宰: 暴击率+40% / 最终伤害+100%\n";
                    text += "- [被动] 威慑: 仇恨值极大提升，免疫大部分削弱效果\n";
                }
            }
            else if (p.currentSequence <= 9)
            {
                text += $"[c/C0C0C0:巨人途径 序列{p.currentSequence}]\n";

                // --- 序列9: 战士 ---
                if (p.currentSequence <= 9)
                {
                    text += $"序列九: [c/D3D3D3:战士]\n";
                    text += "- [被动] 基础防御+8 / 近战伤害+12%\n";
                    text += "- [被动] 生命上限+100 / 近战暴击+5%\n";
                    text += "- [能力] 战斗精通：熟练使用各类近战武器\n";
                }

                // --- 序列8: 格斗学者 ---
                if (p.currentSequence <= 8)
                {
                    text += $"序列八: [c/A9A9A9:格斗学者]\n";
                    text += "- [被动] 近战攻速+15% / 伤害减免+5%\n";
                    text += "- [被动] 钢铁意志：免疫击退\n";
                }

                // --- 序列7: 武器大师 ---
                if (p.currentSequence <= 7)
                {
                    text += $"序列七: [c/808080:武器大师]\n";
                    text += "- [被动] 全伤害+10% / 全暴击+5%\n";
                    text += "- [被动] 弱点洞悉：护甲穿透+10\n";
                }

                // --- 序列6: 黎明骑士 ---
                if (p.currentSequence <= 6)
                {
                    text += $"序列六: [c/FFFFE0:黎明骑士]\n";

                    // 铠甲状态显示
                    string armorStatus;
                    if (p.dawnArmorBroken)
                        armorStatus = $" [c/FF0000:(破碎: {p.dawnArmorCooldownTimer / 60 + 1}s)]";
                    else if (p.dawnArmorActive)
                        armorStatus = $" [c/00FF00:(护盾: {(int)p.dawnArmorCurrentHP}/{(int)p.MaxDawnArmorHP})]";
                    else
                        armorStatus = " [关闭]";

                    text += $"- [技能] 晨曦之铠 (按键切换){armorStatus}: \n";
                    text += "  > 召唤圣光铠甲，提供额外防御与独立护盾值\n";
                    text += "- [被动] 自带圣洁光照 / 生命回复+3\n";
                    if (p.currentSequence == 6)
                    {
                        int target = LotMPlayer.GUARDIAN_RITUAL_TARGET;
                        int current = p.guardianRitualProgress;
                        if (current > target) current = target;

                        string statusText = current >= target ? "[已完成]" : $"[{current}/{target}]";
                        string colorHex = current >= target ? "00FF00" : "FFA500";

                        text += $"[c/{colorHex}:[晋升仪式] 守护之心: {statusText}]\n";
                        text += "  (提示: 在城镇NPC附近承受伤害)\n";
                    }
                }

                // --- 序列5: 守护者 ---
                if (p.currentSequence <= 5)
                {
                    text += $"序列五: [c/708090:守护者]\n";
                    string guardStatus = p.isGuardianStance ? " [c/00FF00:(坚守中)]" : "";

                    text += $"- [技能] 守护姿态 (按住按键){guardStatus}: \n";
                    text += "    > 牺牲移动能力，换取防御+80与30%免伤\n";
                    text += "- [被动] 基础防御+20 / 免疫混乱\n";
                }

                // --- 序列4: 猎魔人 ---
                if (p.currentSequence <= 4)
                {
                    text += $"序列四: [c/696969:猎魔人]\n";
                    text += "- [半神] 猎魔体质：生命+500 / 全伤害+20%\n";
                    text += "- [被动] 超凡感官：获得夜视与生物探测\n";
                    text += "- [被动] 诅咒抗性：免疫咒火与暗影焰\n";
                }

                // --- 序列3: 银骑士 ---
                if (p.currentSequence <= 3)
                {
                    text += $"序列三: [c/C0C0C0:银骑士]\n";
                    string mercuryStatus = p.isMercuryForm ? " [c/00FF00:(化身中)]" : "";

                    text += $"- [技能] 水银化 (按键切换){mercuryStatus}: \n";
                    text += "    > 隐身并极大提升移速，对接触的敌人造成伤害\n";
                    text += "- [被动] 银白闪避：获得黑带闪避效果\n";
                    text += "- [强化] 近战攻速+20% / 生命回复+5\n";
                }

                // --- 序列2: 荣耀战神 ---
                if (p.currentSequence <= 2)
                {
                    text += $"序列二: [c/FFD700:荣耀战神]\n";
                    string cdRevive = p.twilightResurrectionCooldown > 0
                        ? $" [c/FF0000:({p.twilightResurrectionCooldown / 60}s)]"
                        : " [c/00FF00:(就绪)]";

                    text += $"- [神性] 黄昏复活{cdRevive}: \n";
                    text += "    > 受到致命伤时免疫死亡并瞬间回满生命\n";
                    text += "- [被动] 巨人神躯：生命+2000 / 防御+50 / 免伤+15%\n";
                }
                if (p.currentSequence <= 1)
                {
                    text += $"序列一: [c/FF4500:神明之手]\n";
                    text += "- [位格] 黄昏神躯：生命上限+5000 / 基础防御+100\n";
                    text += "- [被动] 衰败权柄：伤害减免+20% / 生命回复+20\n";
                    text += "- [升华] 技能神性化：\n";
                    text += "    > 守护姿态：额外获得10%免伤与强力荆棘反伤\n";
                    text += "    > 晨曦之铠：激活时额外提供50点防御力\n";
                    text += "- [特效] 神性显化：周身散发黄昏光辉\n";
                } 
            }
            else if (p.currentMoonSequence <= 9)
            {
                text += $"[c/EE82EE:月亮途径 序列{p.currentMoonSequence}]\n";
                if (p.currentMoonSequence <= 9) // 药师
                {
                    text += $"序列九: [c/D8BFD8:药师]\n";
                    text += "- [被动] 免疫中毒/剧毒，生命回复提升\n";
                    text += "- [被动] 药物精通：制作药水类物品翻倍(概率)\n";
                    text += "- [能力] 怪物感知：能看到周围的生物\n";
                }
                if (p.currentMoonSequence <= 8) // 驯兽师
                {
                    text += $"序列八: [c/DA70D6:驯兽师]\n";

                    string tameStatus = p.isTamingActive ? " [c/00FF00:(开启)]" : "";

                    text += $"- [能力] 驯兽模式 (按键切换){tameStatus}: \n";
                    text += "    > 使用鞭子攻击虚弱生物(25%血)可将其驯服\n";
                    text += "- [被动] 召唤栏位增加，移动速度提升\n";
                }
                if (p.currentMoonSequence <= 7) // 吸血鬼
                {
                    text += $"序列七: [c/BA55D3:吸血鬼]\n";

                    string wingsStatus = p.isVampireWings ? " [c/00FF00:(展开)]" : "";
                    string cdShackle = p.abyssShackleCooldown > 0 ? $" [c/FF0000:({p.abyssShackleCooldown / 60}s)]" : "";

                    text += $"- [技能] 黑暗之翼 (按键切换){wingsStatus}: 飞行/无视摔伤\n";
                    text += $"- [技能] 深渊枷锁 (按键释放){cdShackle}: 释放黑暗锁链束缚敌人\n";
                    text += "- [被动] 生命回复大幅提升 / 免疫寒冷与冰冻\n";
                }
                if (p.currentMoonSequence <= 6) // 魔药教授
                {
                    text += $"序列六: [c/9932CC:魔药教授]\n";

                    string cdElixir = p.elixirCooldown > 0 ? $" [c/FF0000:({p.elixirCooldown / 60}s)]" : "";

                    text += $"- [技能] 生命灵液 (按键服用){cdElixir}: 瞬间回复大量生命\n";
                    text += "- [技能] 炼金手雷 (按键投掷): 造成范围化学伤害\n";
                    text += "- [被动] 魔法伤害提升 / 免疫火焰\n";
                }
                if (p.currentMoonSequence <= 5) // 深红学者
                {
                    text += $"序列五: [c/8B008B:深红学者]\n";

                    string moonStatus = p.isFullMoonActive ? " [c/00FF00:(开启)]" : "";
                    string lightStatus = p.isMoonlightized ? " [c/00FF00:(化身中)]" : "";

                    text += $"- [技能] 满月领域 (按键切换){moonStatus}: \n";
                    text += "    > 制造满月环境，大幅提升魔法与回蓝\n";
                    text += $"- [技能] 月光化 (按键切换){lightStatus}: \n";
                    text += "    > 化为不可选中的月光，极速移动并免疫伤害\n";
                    text += "- [被动] 免疫黑暗/混乱/沉默\n";
                }
                if (p.currentMoonSequence <= 4) // 巫王
                {
                    text += $"序列四: [c/800080:巫王]\n";

                    string batStatus = p.isBatSwarm ? " [c/00FF00:(化身中)]" : "";
                    string cdGaze = p.darknessGazeCooldown > 0 ? $" [c/FF0000:({p.darknessGazeCooldown / 60}s)]" : "";
                    string cdPaper = p.paperFigurineCooldown > 0 ? $" [c/FF0000:({p.paperFigurineCooldown / 60}s)]" : "";

                    text += $"- [技能] 蝙蝠化身 (按键切换){batStatus}: 分散成蝙蝠群移动\n";
                    text += $"- [技能] 黑暗凝视 (按键释放){cdGaze}: 造成大范围伤害与致盲\n";
                    text += $"- [技能] 纸人替身 (按键触发){cdPaper}: 紧急回避伤害\n";
                }
                if (p.currentMoonSequence <= 3) // 召唤大师
                {
                    text += $"序列三: [c/4B0082:召唤大师]\n";

                    string cdGate = p.summonGateCooldown > 0 ? $" [c/FF0000:({p.summonGateCooldown / 60}s)]" : "";

                    text += $"- [技能] 召唤之门 (按键开启){cdGate}: \n";
                    text += "    > 洞开灵界大门，召唤未知生物助战\n";
                    text += "- [被动] 召唤栏大幅增加 / 召唤伤害提升\n";
                }
                if (p.currentMoonSequence <= 2) // 创生者
                {
                    text += $"序列二: [c/2E0854:创生者]\n";

                    string domainStatus = p.isCreationDomain ? " [c/00FF00:(开启)]" : "";
                    string cdPurify = p.purifyCooldown > 0 ? $" [c/FF0000:({p.purifyCooldown / 60}s)]" : "";
                    string cdLife = p.paperFigurineCooldown > 0 ? $" [c/FF0000:({p.paperFigurineCooldown / 60}s)]" : "";

                    text += $"- [技能] 创生领域 (按键切换){domainStatus}: \n";
                    text += "    > 持续治疗周围队友，并汲取敌人生命\n";
                    text += $"- [技能] 净化大地 (按键释放){cdPurify}: 净化周围的腐化/猩红\n";
                    text += $"- [强化] 纸人 -> 生命剥夺{cdLife}: 强制剥夺范围内敌人生命\n";
                }
                if (p.currentMoonSequence <= 1) // 美神
                {
                    text += $"序列一: [c/FFFFFF:美神]\n";

                    text += "- [被动] 魅惑光环: 周围敌人攻击减半并陷入混乱/爱慕\n";
                    text += "- [被动] 完美闪避: 25%几率完全免疫伤害\n";
                    text += "- [被动] 绯红之主: 全伤害大幅提升，免疫绝大部分负面状态\n";
                }
            }

            else if (p.currentFoolSequence <= 9)
            {
                text += $"[c/8A2BE2:愚者途径 序列{p.currentFoolSequence}]\n"; // 紫色标题

                if (p.currentFoolSequence <= 9) // 占卜家
                {
                    text += $"序列九: [c/9370DB:占卜家]\n";

                    string cdDiv = p.divinationCooldown > 0 ? $" [c/FF0000:({p.divinationCooldown / 60}s)]" : "";
                    string visionStatus = p.isSpiritVisionActive ? " [c/00FF00:(开启)]" : "";

                    text += "- [被动] 魔法伤害+10% / 危险感知\n";
                    text += $"- [能力] 灵视 (C键){visionStatus}: 探宝/见灵\n";
                    text += $"- [技能] 占卜术 (J键){cdDiv}: 随机获得一种启示或Buff\n";
                }
                if (p.currentFoolSequence <= 8) // 小丑
                {
                    text += $"序列八: [c/BA55D3:小丑]\n";

                    text += "- [被动] 身体掌控：移速/跳跃/攻速大幅提升\n";
                    text += "- [被动] 直觉预感：暴击+10% / 10%几率自动闪避\n";
                    text += "- [能力] 纸人飞刀：可以使用特制投掷武器\n";
                }
                if (p.currentFoolSequence <= 7) // 魔术师
                {
                    text += $"序列七: [c/9932CC:魔术师]\n";

                    string cdJump = p.flameJumpCooldown > 0 ? $" [c/FF0000:({p.flameJumpCooldown / 60}s)]" : "";
                    string cdTransfer = p.damageTransferCooldown > 0 ? $" [c/FF0000:({p.damageTransferCooldown / 60}s)]" : "";

                    text += $"- [能力] 火焰跳跃 (F键){cdJump}: 瞬间传送至鼠标指向的任何火焰\n";
                    text += $"- [被动] 伤害转移{cdTransfer}: 受到致命伤时由纸人替死\n";
                    text += "- [被动] 水下呼吸 & 免疫束缚/石化\n";
                    text += "- [技能] 空气弹: 获得专属魔法武器\n";
                    text += "- [能力] 纸人替身: 消耗纸人道具免疫伤害\n";
                }
                if (p.currentFoolSequence <= 6) // 无面人
                {
                    text += $"序列六: [c/8B008B:无面人]\n";

                    string faceStatus = p.isFacelessActive ? " [c/00FF00:(开启)]" : "";
                    string cdDistort = p.distortCooldown > 0 ? $" [c/FF0000:({p.distortCooldown / 60}s)]" : "";

                    text += $"- [技能] 无面伪装 (V键){faceStatus}: 隐身/降低仇恨/增加防御\n";
                    text += $"- [技能] 干扰直觉 (G键){cdDistort}: 使周围敌人混乱并削弱攻击\n";
                    text += "- [被动] 观察者：显示稀有生物/矿物/当前DPS\n";
                    text += "- [被动] 反占卜：40%几率闪避攻击\n";
                }
                if (p.currentFoolSequence <= 5) // 秘偶大师
                {
                    text += $"序列五: [c/800080:秘偶大师]\n";

                    string threadStatus = "";
                    if (p.spiritThreadTargetIndex != -1)
                    {
                        int progress = (int)((float)p.spiritThreadTimer / LotMPlayer.CONTROL_TIME_REQUIRED * 100);
                        threadStatus = $" [c/00FF00:(控制中: {progress}%)]";
                    }

                    text += $"- [技能] 操纵灵体之线 (按住Z键){threadStatus}: \n";
                    text += "    > 持续控制目标，满进度后将其转化为秘偶\n";
                    text += "- [能力] 秘偶化: 转化敌人为友军 (上限+3)\n";
                    text += "- [被动] 灵体视野: 全图探敌/危险感知\n";
                }
                if (p.currentFoolSequence <= 4) // 诡法师
                {
                    text += $"序列四: [c/4B0082:诡法师]\n";

                    string cdSwap = p.swapCooldown > 0 ? $" [c/FF0000:({p.swapCooldown / 60}s)]" : "";
                    string cdControl = p.spiritControlCooldown > 0 ? $" [c/FF0000:({p.spiritControlCooldown / 60}s)]" : "";

                    text += $"- [被动] 灵之虫: 拥有 {p.spiritWorms} 条灵之虫 (用于复活/施法)\n";
                    text += $"- [技能] 秘偶互换 (T键){cdSwap}: 与最近的秘偶瞬间换位\n";
                    text += $"- [技能] 灵体震慑 (R键){cdControl}: 大范围群体麻痹/混乱\n";
                    text += "- [强化] 占卜术: 效果增强，甚至可以预知未来\n";
                    text += "- [强化] 灵体之线: 转化速度提升 (上限10个秘偶)\n";
                }
                if (p.currentFoolSequence <= 3) // 古代学者
                {
                    text += $"序列三: [c/483D8B:古代学者]\n";

                    string borrowStatus = p.isBorrowingPower ? " [c/00FF00:(生效中)]" : "";

                    text += "- [技能] 历史投影 (Y键): 随机召唤曾击败过的Boss投影助战\n";
                    text += $"- [技能] 昨日重现 (U键){borrowStatus}: \n";
                    text += $"    > 向过去的自己借取力量 (剩余次数: {10 - p.borrowUsesDaily})\n";
                    text += "- [被动] 灵之虫++: 上限600条，自动快速回复\n";
                    text += "- [强化] 空气炮: 威力提升至岸防炮级别\n";
                }
                if (p.currentFoolSequence <= 2) // 奇迹师
                {
                    text += $"序列二: [c/2E0854:奇迹师]\n";

                    string cdMiracle = p.miracleCooldown > 0 ? $" [c/FF0000:({p.miracleCooldown / 60}s)]" : "";
                    string fateStatus = p.fateDisturbanceActive ? " [c/00FF00:(开启)]" : "";

                    string wishName = p.selectedWish == 0 ? "生命复苏" :
                                      p.selectedWish == 1 ? "毁灭天灾" :
                                      p.selectedWish == 2 ? "空间传送" : "昼夜更替";

                    text += $"- [技能] 奇迹愿望 (V键){cdMiracle}: \n";
                    text += $"    > 单按切换，长按实现: [c/00FFFF:{wishName}]\n";
                    text += $"- [技能] 命运干扰 (G键){fateStatus}: 开启光环，极大提升暴击与运气\n";
                    text += "- [技能] 历史场景 (Shift+Y): 召唤大型历史投影降临\n";
                    text += "- [被动] 复活奇迹: 死亡时引发全屏毁灭打击并复活\n";

                    if (p.currentFoolSequence == 2)
                    {
                        int target = LotMPlayer.ATTENDANT_RITUAL_TARGET; // 10
                        int current = p.attendantRitualProgress;
                        string status = current >= target ? "[已完成]" : $"[{current}/{target}]";
                        string colorHex = current >= target ? "00FF00" : "FFA500";

                        text += $"[c/{colorHex}:[晋升仪式] 诡秘侍者: {status}]\n";
                        text += "  (提示: 将10个城镇NPC转化为秘偶)\n";
                    }
                }
                if (p.currentFoolSequence <= 1) // 诡秘侍者
                {
                    text += $"序列一: [c/FFFFFF:诡秘侍者]\n";

                    string spiritFormStatus = p.isSpiritForm ? " [c/00FF00:(灵体化)]" : "";
                    string graftMode = p.graftingMode == 0 ? "关闭" : (p.graftingMode == 1 ? "空间反弹" : "概念必杀");
                    string cdGraft = p.graftingCooldown > 0 ? $" [c/FF0000:({p.graftingCooldown / 60}s)]" : "";

                    text += $"- [技能] 灵肉转化 (V键){spiritFormStatus}: 物理免疫/穿墙/高耗蓝\n";
                    text += "- [技能] 奇迹愿望 (Shift+V): 功能同序列2\n";
                    text += $"- [技能] 概念嫁接 (G键){cdGraft}: 当前模式: [c/00FFFF:{graftMode}]\n";
                    text += "- [技能] 命运干扰 (Shift+G): 开启/关闭光环\n";
                    text += "- [被动] 诡秘之境: 自动吞噬周围掉落物回血，压制敌人\n";
                    text += "- [被动] 概念防御: 受到致命伤时自动消耗灵性嫁接死亡\n";
                }
            }

            else if (p.currentMarauderSequence <= 9)
            {
                text += $"[c/3232C8:错误途径 序列{p.currentMarauderSequence}]\n";

                if (p.currentMarauderSequence <= 9)
                {
                    text += $"序列九: [c/E6E6FA:偷盗者]\n";
                    text += "- 卓越观察: 常驻探宝药水效果\n";
                    text += "- [主动]窃取: 拾取范围扩大，攻击几率偷钱\n";
                    text += "- 敏捷之手: 挖掘/放置速度提升\n";
                    text += "- 短兵精通: 短剑类武器大幅增强\n";
                }
                if (p.currentMarauderSequence <= 8) // 诈骗师
                {
                    text += $"序列八: [c/D8BFD8:诈骗师]\n";
                    text += "- [被动] 魅力与口才: 商店购物打折\n";
                    text += "- [被动] 欺诈大师: 移动速度大幅提升\n";
                    text += "- [被动] 精神干扰: 攻击有概率使敌人混乱\n";
                }
                if (p.currentMarauderSequence <= 7) // 解密学者
                {
                    text += $"序列七: [c/DA70D6:解密学者]\n";
                    text += "- [被动] 灵性直觉: 获得狩猎与危险感知视野\n";
                    text += "- [被动] 弱点解析: 护甲穿透+10 / 暴击+5%\n";
                }
                if (p.currentMarauderSequence <= 6) // 盗火人
                {
                    text += $"序列六: [c/BA55D3:盗火人]\n";

                    text += "- [主动] 窃取: 攻击概率直接获得怪物掉落物\n";
                    text += "- [被动] 窃取能力: 攻击吸取生命与魔力\n";
                    text += "- [被动] 精神抗性: 免疫大部分精神干扰Debuff\n";
                }
                if (p.currentMarauderSequence <= 5) // 窃梦家
                {
                    text += $"序列五: [c/9932CC:窃梦家]\n";

                    text += "- [被动] 窃取意图: 攻击使敌人呆滞/混乱\n";
                    text += "- [被动] 梦魇光环: 附近敌人减速且减防\n";
                    text += "- [特效] 记忆窃取: 攻击削弱敌人并回血\n";
                    text += "- [被动] 盗天机: 战斗中随机窃取强力Buff\n";
                    text += "- [能力] 梦境主宰: 拾取范围极大提升\n";

                    if (p.currentMarauderSequence == 5)
                    {
                        string status = p.parasiteRitualProgress >= 9 ? "[已完成]" : $"[{p.parasiteRitualProgress}/9]";
                        string colorHex = p.parasiteRitualProgress >= 9 ? "00FF00" : "FFA500";
                        text += $"[c/{colorHex}:[晋升仪式] 窃取供养: {status}]\n";
                    }
                }
                if (p.currentMarauderSequence <= 4) // 寄生者
                {
                    text += $"序列四: [c/8A2BE2:寄生者 (半神)]\n";

                    text += "- [被动] 半虫化: 除非命中要害，否则免疫一次致死伤害(长冷却)\n";
                    text += "- [主动] 寄生 (按P键): \n";
                    text += "    > 对城镇NPC: 浅层寄生，随身隐形并快速回血\n";
                    text += "    > 对敌人: 深层寄生，持续造成伤害并吸血/控制\n";
                    text += "- [主动] 概念窃取 (K键): \n";
                    text += "    > 窃取“距离”: 瞬间移动到鼠标位置\n";
                    text += "    > 窃取“位置”: 与目标互换位置\n";

                    if (p.currentMarauderSequence == 4)
                    {
                        string status = p.mentorRitualProgress >= 9 ? "[已完成]" : $"[{p.mentorRitualProgress}/9]";
                        string colorHex = p.mentorRitualProgress >= 9 ? "00FF00" : "FFA500";
                        text += $"[c/{colorHex}:[晋升仪式] 误导冤魂: {status}]\n";
                        text += "  (提示: 让敌人在[混乱]状态下死亡)\n";
                    }
                }
                if (p.currentMarauderSequence <= 3) // 欺瞒导师
                {
                    text += $"序列三: [c/800080:欺瞒导师 (圣者)]\n";

                    text += "- [被动] 欺诈权柄: 窃取技能判定次数 x3\n";
                    text += "- [主动] 欺瞒领域 (Shift+P): \n";
                    text += "    > 扭曲周围规则，使敌人强制混乱并减防\n";
                    text += "    > 序列二后，误导飞行道具，使其偏离甚至倒戈\n";

                    if (p.currentMarauderSequence == 3)
                    {
                        float percent = (float)p.trojanRitualTimer / LotMPlayer.TROJAN_RITUAL_TARGET * 100;
                        string status = percent >= 100 ? "[已完成]" : $"[{percent:F1}%]";
                        string colorHex = percent >= 100 ? "00FF00" : "FFA500";
                        text += $"[c/{colorHex}:[晋升仪式] 顶替身份: {status}]\n";
                        text += "  (提示: 寄生城镇NPC并保持一段时间)\n";
                    }
                }
                if (p.currentMarauderSequence <= 2) // 命运木马
                {
                    text += $"序列二: [c/4B0082:命运木马 (天使)]\n";

                    text += "- [被动] 命运权柄: 窃取判定次数 x6\n";
                    text += "- [被动] 命运预见: 受到致命伤时由分身替死 (冷却)\n";
                    text += "- [主动] 命运窃取 (Shift+O): \n";
                    text += "    > 嫁接命运：若敌人血量比你高，互换生命比例\n";
                    text += "    > 对Boss造成巨额真实伤害并剥夺其攻击能力\n";

                    if (p.currentMarauderSequence == 2)
                    {
                        float percent = (float)p.wormRitualTimer / LotMPlayer.WORM_RITUAL_TARGET * 100;
                        string status = percent >= 100 ? "[已完成]" : $"[{percent:F1}%]";
                        string colorHex = percent >= 100 ? "00FF00" : "FFA500";
                        text += $"[c/{colorHex}:[晋升仪式] 时光混乱: {status}]\n";
                        text += "  (提示: 在城镇中开启欺瞒领域并维持)\n";
                    }
                }
                if (p.currentMarauderSequence <= 1) // 时之虫
                {
                    text += $"序列一: [c/FFFFFF:时之虫 (天使之王)]\n";

                    text += "- [被动] 时间权柄: 免疫大部分时间控制/减速效果\n";
                    text += "- [主动] 时之虫领域 (Shift+P): \n";
                    text += "    > 召唤壁钟虚影，范围内敌人极度减速并持续衰老(掉血)\n";
                    text += "- [主动] 窃取时间 (Shift+O): \n";
                    text += "    > 瞬间剥夺目标生命(老化)，并赋予自身极速\n";
                    text += "- [能力] 错误化身: 死亡时由序列2分身通过窃取命运来复活(强化版)\n";
                }
            }
            // 6. 太阳途径 (Sun)
            else if (p.currentSunSequence <= 9)
            {
                // 标题颜色：金色
                text += $"[c/FFD700:太阳途径 序列{p.currentSunSequence}]\n";

                if (p.currentSunSequence <= 9) // 歌颂者
                {
                    string timeSing = p.isSinging && p.singTimer > 0 ? $" [c/00FF00:(剩余{p.singTimer / 60}s)]" : "";

                    text += $"序列九: [c/FFFFE0:歌颂者]\n";
                    text += "- [被动] 体质增强: 生命上限/防御力/生命回复提升\n";
                    text += $"- [主动] 赞美太阳 (Z键){timeSing}: \n"; // 这里加上了时间变量
                    text += "    > 吟唱圣歌，净化周围队友的负面状态\n";
                    text += "    > 赋予勇气(免疫恐惧)、力量(增伤)、敏捷(加速)\n";
                    text += "    > 持续回复灵性，对太阳信徒有额外加成\n";
                }
                if (p.currentSunSequence <= 8) // 祈光人
                {
                    text += $"序列八: [c/FFFACD:祈光人]\n";
                    string cdRadiance = p.sunRadianceCooldown > 0 ? $" [c/FF0000:({p.sunRadianceCooldown / 60}s)]" : "";

                    text += "- [被动] 白昼: 自带大范围圣洁光照\n";
                    text += "- [被动] 驱魔: 你的歌声现在能赋予队友寒冷免疫与亡灵克制\n";
                    text += $"- [主动] 日照 (X键){cdRadiance}: \n"; // 这里加上了冷却变量
                    text += "    > 召唤如正午烈阳般的光辉，对周围造成伤害\n";
                    text += "    > 对死尸/鬼魂造成3倍暴击，并致盲生者\n";
                    text += "- [仪式] 圣水制造: 可在水源旁廉价转化圣水\n";
                }
                if (p.currentSunSequence <= 7) // 太阳神官
                {

                    text += $"序列七: [c/FFD700:太阳神官]\n";
                    string cdLight = p.holyLightCooldown > 0 ? $" [c/FF0000:({p.holyLightCooldown / 60}s)]" : "";
                    string cdOath = p.holyOathCooldown > 0 ? $" [c/FF0000:({p.holyOathCooldown / 60}s)]" : "";
                    string cdFire = p.fireOceanCooldown > 0 ? $" [c/FF0000:({p.fireOceanCooldown / 60}s)]" : "";

                    text += "- [被动] 神圣体质: 免疫各类恶劣环境与负面状态\n";
                    text += "- [被动] 太阳光环: 持续净化并强化周围队友\n";
                    text += $"- [技能] 召唤圣光 (C键){cdLight}: 降下毁灭光柱\n";
                    text += $"- [技能] 神圣誓约 (V键){cdOath}: 获得随机强力增益\n";
                    text += $"- [技能] 光明之火 (G键){cdFire}: 召唤熔化大地的火海\n";
                }
                if (p.currentSunSequence <= 6) // 公证人
                {
                    text += $"序列六: [c/FFA500:公证人]\n";
                    string cdNotarize = p.notarizeCooldown > 0 ? $" [c/FF0000:({p.notarizeCooldown / 60}s)]" : "";

                    text += "- [被动] 商业公证: 商店购物半价，拾取范围扩大\n";
                    text += "- [被动] 规则体质: 生命防御大幅提升，免疫流血/中毒\n";
                    text += $"- [技能] 公证 (Z键){cdNotarize}: \n";
                    text += "    > 队友: 驱散Debuff并赋予增益\n";
                    text += "    > 敌人: 造成神圣伤害并削弱\n";

                    if (p.currentSunSequence == 6)
                    {
                        int target = 100; // LotMPlayer.PURIFICATION_RITUAL_TARGET
                        int current = p.purificationProgress;

                        string status = current >= target ? "[已完成]" : $"[{current}/{target}]";
                        string colorHex = current >= target ? "00FF00" : "FFA500";

                        text += $"\n[c/{colorHex}:[晋升仪式] 净化之路: {status}]\n";
                        text += "  (提示: 需净化100个不死生物以晋升光之祭司)\n";
                    }
                }

                if (p.currentSunSequence <= 5) // 光之祭司
                {
                    text += $"序列五: [c/FF8C00:光之祭司]\n";
                    string cdHoly = p.holyLightCooldown > 0 ? $" [c/FF0000:({p.holyLightCooldown / 60}s)]" : "";

                    text += "- [被动] 光之眼: 获得夜视与危险感知\n";
                    text += "- [被动] 净化光环: 自动灼烧周围死灵\n";
                    text += $"- [质变] 神圣之光 (C键){cdHoly}: 召唤光柱净化污秽\n";

                    // 【修正】序列 5 显示晋升序列 4 的条件
                    // 条件：审判进度 + 无暗十字
                    if (p.currentSunSequence == 5)
                    {
                        // 1. 击杀进度
                        int target = 20; // LotMPlayer.JUDGMENT_RITUAL_TARGET
                        int current = p.judgmentProgress;
                        string killStatus = current >= target ? "[审判完成]" : $"[审判: {current}/{target}]";

                        // 2. 物品要求
                        bool hasItem = p.Player.HasItem(ModContent.ItemType<Items.UnshadowedCross>());
                        string itemStatus = hasItem ? "[十字就绪]" : "[缺无暗十字]";

                        // 综合判定
                        bool ready = (current >= target) && hasItem;
                        string colorHex = ready ? "00FF00" : "FFA500";

                        text += $"\n[c/{colorHex}:[晋升仪式] 无暗者]\n";
                        text += $"  1. {killStatus}\n";
                        text += $"  2. 条件: {itemStatus}\n";
                        text += "  (提示: 审判20个强敌，并持有无暗十字)\n";
                    }
                }

                if (p.currentSunSequence <= 4) // 无暗者
                {
                    text += $"序列四: [c/FF4500:无暗者 (半神)]\n";
                    string cdNotarize = p.notarizeCooldown > 0 ? $" [c/FF0000:({p.notarizeCooldown / 60}s)]" : "";
                    string cdSpear = p.sunRadianceCooldown > 0 ? $" [c/FF0000:({p.sunRadianceCooldown / 60}s)]" : "";
                    string cdSun = p.fireOceanCooldown > 0 ? $" [c/FF0000:({p.fireOceanCooldown / 60}s)]" : "";

                    text += "- [半神] 无暗之域: 免疫黑暗，看破隐形\n";
                    text += "- [被动] 神圣盔甲: 极高防御与减伤\n";
                    text += $"- [技能] 无暗之枪 (X键){cdSpear}\n";
                    text += $"- [大招] 阳炎 (G键){cdSun}\n";

                    // 【修正】序列 4 显示晋升序列 3 的条件
                    // 条件：白天 + 徽章 + 纯净
                    if (p.currentSunSequence == 4)
                    {
                        bool isDay = Main.dayTime;
                        bool hasEmblem = p.Player.HasItem(ItemID.AvengerEmblem);
                        bool isClean = !p.Player.HasBuff(BuffID.Bleeding) && !p.Player.HasBuff(BuffID.Poisoned);

                        string timeStatus = isDay ? "[白天]" : "[等待白天]";
                        string itemStatus = hasEmblem ? "[徽章就绪]" : "[缺复仇者徽章]";
                        string cleanStatus = isClean ? "[身体纯净]" : "[有不洁状态]";

                        string colorHex = (isDay && hasEmblem && isClean) ? "00FF00" : "FFA500";

                        text += $"\n[c/{colorHex}:[晋升仪式] 正义导师]\n";
                        text += $"  条件: {timeStatus} {itemStatus} {cleanStatus}\n";
                        text += "  (提示: 白天，持有复仇者徽章，且无流血/中毒状态)\n";
                    }
                }

                if (p.currentSunSequence <= 3) // 正义导师
                {
                    text += $"序列三: [c/FF6347:正义导师 (圣者)]\n";
                    string cdJudge = p.notarizeCooldown > 0 ? $" [c/FF0000:({p.notarizeCooldown / 60}s)]" : "";
                    string cdContract = p.holyOathCooldown > 0 ? $" [c/FF0000:({p.holyOathCooldown / 60}s)]" : "";

                    text += "- [被动] 圣者体质 / 召唤上限+3\n";
                    text += $"- [质变] 正义审判 (Z键){cdJudge}: 巨额单体伤害\n";
                    text += $"- [技能] 神圣契约 (V键){cdContract}: 提升全属性\n";

                    if (p.currentSunSequence == 3)
                    {
                        // 这里沿用你的逻辑：需击败光之女皇
                        bool condition = NPC.downedEmpressOfLight;
                        // 补上序列2魔药里写的条件：需持有日耀圣物
                        bool hasArtifact = p.Player.HasItem(ItemID.DayBreak) || p.Player.HasItem(ItemID.SolarEruption);

                        string bossStatus = condition ? "[光女已败]" : "[需击败光之女皇]";
                        string itemStatus = hasArtifact ? "[圣物就绪]" : "[缺日耀武器]";

                        bool ready = condition && hasArtifact;
                        string colorHex = ready ? "00FF00" : "FFA500";

                        text += $"\n[c/{colorHex}:[晋升仪式] 逐光者]\n";
                        text += $"  条件: {bossStatus} {itemStatus}\n";
                        text += "  (提示: 击败光之女皇，并持有日耀圣物)\n";
                    }
                }

                if (p.currentSunSequence <= 2) // 逐光者
                {
                    text += $"序列二: [c/FF0000:逐光者 (天使)]\n";
                    text += "- [被动] 光化重组: 闪避瞬移\n";
                    text += "- [技能] 太阳使者 (P键): 无限飞行与狱火光环\n";

                    if (p.currentSunSequence == 2)
                    {
                        bool isNoon = Main.dayTime && Main.time >= 27000 && Main.time <= 36000;
                        int npcCount = 0;
                        for (int i = 0; i < Main.maxNPCs; i++) if (Main.npc[i].active && Main.npc[i].townNPC) npcCount++;

                        string countStatus = npcCount >= 15 ? "[信徒充足]" : $"[信徒: {npcCount}/15]";
                        string timeStatus = isNoon ? "[正午]" : "[等待正午]";

                        bool ready = isNoon && npcCount >= 15;
                        string colorHex = ready ? "00FF00" : "FFA500";

                        text += $"\n[c/{colorHex}:[晋升仪式] 纯白国度]\n";
                        text += $"  条件: {timeStatus} {countStatus}\n";
                        text += "  (提示: 正午时分，至少15位城镇NPC存活)\n";
                    }
                }

                if (p.currentSunSequence <= 1)
                {
                    text += $"[c/FFFFFF:序列1: 纯白天使 (天使之王)]\n";
                    text += "- [被动] 神圣之国: 压制全场，斩杀亡灵\n";
                    text += "- [技能] 纯白之光 / 信仰之仆\n";
                    text += "  (已达当前版本顶峰)";
                }
            }

            return text;
        }
    }
}