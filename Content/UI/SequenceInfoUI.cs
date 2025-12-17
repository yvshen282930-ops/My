using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent; // 用于获取贴图
using Terraria.ModLoader;
using Terraria.UI;
using zhashi.Content; // 引用 Player 类

// 引用魔药命名空间以获取图标 ID
using zhashi.Content.Items.Potions;        // 巨人途径 (Warrior等)
using zhashi.Content.Items.Potions.Hunter; // 猎人途径
using zhashi.Content.Items.Potions.Moon;   // 月亮途径
using zhashi.Content.Items.Potions.Fool; //愚者魔
using zhashi.Content.Items.Potions.Marauder; //错误

namespace zhashi.Content.UI
{
    public class SequenceInfoUI : UIState
    {
        // 定义图标的位置和大小
        private const int ICON_SIZE = 44;
        private const int LEFT_MARGIN = 20;
        private const int TOP_MARGIN = 120; // 您可以调整这个值改变垂直位置

        public override void Draw(SpriteBatch spriteBatch)
        {
            // 获取当前玩家
            Player player = Main.LocalPlayer;
            if (player == null || !player.active || player.dead || player.ghost) return;

            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 只有非凡者才显示
            if (!modPlayer.IsBeyonder) return;

            // 1. 获取当前应该显示的魔药图标 ID
            int itemType = GetCurrentSequencePotionID(modPlayer);
            if (itemType <= 0) return;

            // 2. 加载贴图
            Main.instance.LoadItem(itemType);
            Texture2D texture = TextureAssets.Item[itemType].Value;

            // 3. 计算绘制位置 (屏幕左上角)
            Vector2 drawPos = new Vector2(LEFT_MARGIN, TOP_MARGIN);
            Rectangle iconRect = new Rectangle((int)drawPos.X, (int)drawPos.Y, ICON_SIZE, ICON_SIZE);

            // 4. 绘制图标背景 (可选，增加一个半透明黑底让图标更清楚)
            Utils.DrawInvBG(spriteBatch, iconRect, new Color(20, 20, 40, 150));

            // 5. 绘制图标 (居中绘制)
            Rectangle frame = texture.Frame(); // 获取图片完整帧
            float scale = 1f;
            if (frame.Width > 32 || frame.Height > 32) scale = 0.8f; // 如果图太大就缩一下

            Vector2 origin = frame.Size() / 2f;
            Vector2 center = drawPos + new Vector2(ICON_SIZE / 2f, ICON_SIZE / 2f);

            spriteBatch.Draw(texture, center, frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

            // 6. 鼠标悬停检测
            if (iconRect.Contains(Main.MouseScreen.ToPoint()))
            {
                // 暂时禁止玩家使用物品，防止点穿
                player.mouseInterface = true;

                // 获取详细文本
                string tooltip = GetSequenceDescription(modPlayer);

                // 绘制原版风格的鼠标提示文字
                Main.instance.MouseText(tooltip);
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
            return 0;
        }

        // --- 辅助方法：生成纯文本介绍 ---
        private string GetSequenceDescription(LotMPlayer p)
        {
            string text = "";

            if (p.currentHunterSequence <= 9)
            {
                text += $"[c/FF4500:猎人途径 序列{p.currentHunterSequence}]\n";
                if (p.currentHunterSequence <= 9) text += "- 远程伤害+15% / 生物监测\n";
                if (p.currentHunterSequence <= 8) text += "- 仇恨+300 / 防御+10\n";
                if (p.currentHunterSequence <= 7) text += "- [技能] 蓄力火球 / 火焰披风\n";
                if (p.currentHunterSequence <= 6) text += "- [技能] 火焰闪现 / 武器附魔\n";
                if (p.currentHunterSequence <= 5) text += "- [技能] 收割斩击 / 弱点处决\n";
                if (p.currentHunterSequence <= 4) text += "- [技能] 集众 (召唤栏UP) / 火焰形态\n";
                if (p.currentHunterSequence <= 2) text += "- [技能] 闪电风暴 / 冰河世纪 / 巨人化\n";
                if (p.currentHunterSequence <= 1) text += "- [神性] 征服者：全属性大幅提升\n";
            }
            else if (p.currentSequence <= 9)
            {
                text += $"[c/C0C0C0:巨人途径 序列{p.currentSequence}]\n";
                if (p.currentSequence <= 9) text += "- 基础防御+8 / 近战+12%\n";
                if (p.currentSequence <= 8) text += "- 攻速+15% / 免疫击退\n";
                if (p.currentSequence <= 7) text += "- 全伤害+10% / 穿透+10\n";
                if (p.currentSequence <= 6) text += "- [技能] 晨曦之铠 (高额护盾)\n";
                if (p.currentSequence <= 5) text += "- [技能] 守护姿态 (极高防御)\n";
                if (p.currentSequence <= 4) text += "- 生命+2000 / 夜视\n";
                if (p.currentSequence <= 3) text += "- [技能] 水银化 (隐身加速)\n";
                if (p.currentSequence <= 2) text += "- [神性] 黄昏复活 (濒死回满)\n";
            }
            else if (p.currentMoonSequence <= 9)
            {
                text += $"[c/EE82EE:月亮途径 序列{p.currentMoonSequence}]\n";
                if (p.currentMoonSequence <= 9) text += "- 免疫中毒 / 炼药双倍\n";
                if (p.currentMoonSequence <= 8) text += "- [技能] 驯兽/ 召唤栏+2 / 移速+30%\n";
                if (p.currentMoonSequence <= 7) text += "- [技能] 黑暗之翼 / 深渊枷锁\n";
                if (p.currentMoonSequence <= 6) text += "- [技能] 生命灵液 / 炼金手雷\n";
                if (p.currentMoonSequence <= 5) text += "- [技能] 满月领域 / 月光化\n";
                if (p.currentMoonSequence <= 4) text += "- [技能] 蝙蝠化身 / 黑暗凝视\n";
                if (p.currentMoonSequence <= 3) text += "- [技能] 召唤之门 (召唤BOSS/强力生物)\n";
                if (p.currentMoonSequence <= 2) text += "- [技能] 创生领域 (主宰战场)\n";
                if (p.currentMoonSequence <= 1) text += "- [神性] 美神：魅惑万物\n";
            }

            else if (p.currentFoolSequence <= 9)
            {
                text += $"[c/8A2BE2:愚者途径 序列{p.currentFoolSequence}]\n"; // 紫色标题

                if (p.currentFoolSequence <= 9)
                {
                    text += "- [被动] 魔法伤害+10% / 危险感知\n";
                    text += "- [能力] 灵视 (C键开关, 探宝/见灵)\n";
                    text += "- [技能] 占卜术 (J键, 随机启示/Buff)\n";
                }
                if (p.currentFoolSequence <= 8)
                {
                    text += "- [被动] 身体掌控：移速/跳跃/攻速大幅提升\n";
                    text += "- [被动] 直觉预感：暴击+10% / 闪避几率提升\n";
                    text += "- [能力] 化纸为刀：可以使用[纸人飞刀]武器\n";
                }
                if (p.currentFoolSequence <= 7)
                {
                    text += "- [能力] 火焰跳跃 (F键, 30米瞬移)\n";
                    text += "- [被动] 伤害转移 (抵挡致命伤)\n";
                    text += "- [被动] 水下呼吸 & 免疫束缚\n";
                    text += "- [技能] 空气弹 (获得魔法武器)\n";
                    text += "- [能力] 制作纸人替身（抵挡伤害）\n";
                }
                if (p.currentFoolSequence <= 6)
                {
                    text += "- [技能] 无面伪装 (V键, 隐身/高防)\n";
                    text += "- [技能] 干扰直觉 (G键, 混乱敌人)\n";
                    text += "- [被动] 观察者：显示生物信息/探矿\n";
                    text += "- [强化] 空气弹威力翻倍 / 技能冷却减半\n";
                }
                if (p.currentFoolSequence <= 5)
                {
                    text += "- [技能] 操纵灵体之线 (按住Z键, 强控/转化)\n";
                    text += "- [能力] 秘偶化 (转化敌人为友军, 上限+3)\n";
                    text += "- [被动] 灵体视野 (全图探敌/探宝)\n";
                    text += "- [强化] 空气弹威力 (三倍伤害)\n";
                }
                if (p.currentFoolSequence <= 4)
                {
                    text += "- [被动] 灵之虫 (免死/重生)\n";
                    text += "- [技能] 秘偶互换 (T键, 瞬间换位)\n";
                    text += "- [技能] 灵体震慑 (R键, 群体麻痹)\n";
                    text += "- [强化] 占卜术 (包含战斗/生存/运气)\n";
                    text += "- [强化] 灵体之线 (5倍速转化 / 10个秘偶)\n";
                }
                if (p.currentFoolSequence <= 3)
                {
                    text += "- [技能] 历史投影 (Y键, 召唤败在你手中的生物)\n";
                    text += "- [技能] 昨日重现 (U键, 全属性爆发)\n";
                    text += "- [被动] 灵之虫++ (600条, 自动回复)\n";
                    text += "- [强化] 空气炮 (岸防炮级威力)\n";
                    text += "- [强化] 灵体之线 (瞬控 / 全图互换)\n";
                }
                if (p.currentFoolSequence <= 2)
                {
                    text += "- 奇迹愿望: 按 V 切换愿望，长按 V 实现\n";
                    text += "- 命运干扰: 按 G 开启光环\n";
                    text += "- 历史场景: Shift + Y 召唤\n";
                }
                if (p.currentFoolSequence <= 1)
                {
                    text += "- 灵肉转化: 按 V (物理免疫/穿墙)\n";
                    text += "- 奇迹愿望: Shift + V (切换/长按实现)\n";
                    text += "- 概念嫁接: 按 G (空间反弹/攻击必杀)\n";
                    text += "- 命运干扰: Shift + G (开启/关闭光环)\n";
                    text += "- 诡秘之境: 被动 (吞噬掉落物/压制敌人)\n";
                }
            }

            else if (p.currentMarauderSequence <= 9)
            {
                text += $"[c/3232C8:错误途径 序列{p.currentMarauderSequence}]\n";

                if (p.currentMarauderSequence <= 9)
                {
                    text += "- 卓越观察: 常驻探宝药水效果\n";
                    text += "- 窃取: 拾取范围扩大，攻击几率偷钱\n";
                    text += "- 敏捷之手: 挖掘/放置速度提升\n";
                    text += "- 短兵精通: 短剑类武器大幅增强\n";
                }
                if (p.currentMarauderSequence <= 8) // 诈骗师
                {
                    text += "- [被动] 魅力与口才: 商店购物打折\n";
                    text += "- [被动] 欺诈大师: 移动速度大幅提升\n";
                    text += "- [被动] 精神干扰: 攻击有概率使敌人混乱\n";
                }
                if (p.currentMarauderSequence <= 7) // 解密学者
                {
                    text += "- [被动] 灵性直觉: 获得狩猎与危险感知视野\n";
                    text += "- [被动] 弱点解析: 护甲穿透+10 / 暴击+5%\n";
                }
                if (p.currentMarauderSequence <= 6) // 盗火人
                {
                    text += "- [被动] 窃取: 攻击概率直接获得怪物掉落物\n";
                    text += "- [被动] 窃取能力: 攻击吸取生命与魔力\n";
                    text += "- [被动] 精神抗性: 免疫大部分精神干扰Debuff\n";
                }
                if (p.currentMarauderSequence <= 5) // 窃梦家
                {
                    text += "- [被动] 窃取意图: 攻击使敌人呆滞/混乱\n";
                    text += "- [被动] 梦魇光环: 附近敌人减速且减防\n";
                    text += "- [特效] 记忆窃取: 攻击削弱敌人并回血\n";
                    text += "- [被动] 盗天机: 战斗中随机窃取强力Buff\n";
                    text += "- [能力] 梦境主宰: 拾取范围极大提升\n";
                    if (p.currentMarauderSequence == 5)
                    {
                        // 这里的 9 与 LotMPlayer.PARASITE_RITUAL_TARGET 对应
                        string status = p.parasiteRitualProgress >= 9 ? "[已完成]" : $"[{p.parasiteRitualProgress}/9]";
                        // 进度未满显示橙色，满了显示绿色
                        string colorHex = p.parasiteRitualProgress >= 9 ? "00FF00" : "FFA500";
                        text += $"[c/{colorHex}:[晋升仪式] 窃取供养: {status}]\n";
                    }
                }
                if (p.currentMarauderSequence <= 4) // 寄生者 (Parasite)
                {
                    text += "- [被动] 半虫化: 除非命中要害，否则免疫一次致死伤害(长冷却)\n";
                    text += "- [主动] 寄生 (按P键): \n";
                    text += "    > 对城镇NPC: 浅层寄生，随身隐形并快速回血\n";
                    text += "    > 对敌人: 深层寄生，持续造成伤害并吸血/控制\n";
                    text += "- [主动] 概念窃取 (K键): \n";
                    text += "    > 窃取“距离”: 瞬间移动到鼠标位置\n";
                    text += "    > 窃取“位置”: 与目标互换位置\n";
                }
                if (p.currentMarauderSequence == 4)
                {
                    // 显示序列4晋升序列3的仪式进度
                    string status = p.mentorRitualProgress >= 9 ? "[已完成]" : $"[{p.mentorRitualProgress}/9]";
                    string colorHex = p.mentorRitualProgress >= 9 ? "00FF00" : "FFA500";
                    text += $"[c/{colorHex}:[晋升仪式] 误导冤魂: {status}]\n";
                    text += "  (提示: 让敌人在[混乱]状态下死亡)\n";
                }
                if (p.currentMarauderSequence <= 3) // 欺瞒导师
                {
                    text += "- [被动] 欺诈权柄: 窃取技能判定次数 x3\n";
                    text += "- [主动] 欺瞒领域 (Shift+P): \n";
                    text += "    > 扭曲周围规则，使敌人强制混乱并减防\n";
                    text += "    > 误导飞行道具，使其偏离甚至倒戈\n";
                }
                if (p.currentMarauderSequence == 3)
                {
                    // 显示序列3 -> 序列2 仪式进度
                    float percent = (float)p.trojanRitualTimer / LotMPlayer.TROJAN_RITUAL_TARGET * 100;
                    string status = percent >= 100 ? "[已完成]" : $"[{percent:F1}%]";
                    string colorHex = percent >= 100 ? "00FF00" : "FFA500";
                    text += $"[c/{colorHex}:[晋升仪式] 顶替身份: {status}]\n";
                    text += "  (提示: 寄生城镇NPC并保持一段时间)\n";
                }
                if (p.currentMarauderSequence <= 2) // 命运木马
                {
                    text += "- [被动] 命运权柄: 窃取判定次数 x6\n";
                    text += "- [被动] 命运预见: 受到致命伤时由分身替死 (冷却)\n";
                    text += "- [主动] 命运窃取 (Shift+O): \n";
                    text += "    > 嫁接命运：若敌人血量比你高，互换生命比例\n";
                    text += "    > 对Boss造成巨额真实伤害并剥夺其攻击能力\n";
                }
                if (p.currentMarauderSequence == 2)
                {
                    // 显示序列2 -> 序列1 仪式进度
                    float percent = (float)p.wormRitualTimer / LotMPlayer.WORM_RITUAL_TARGET * 100;
                    string status = percent >= 100 ? "[已完成]" : $"[{percent:F1}%]";
                    string colorHex = percent >= 100 ? "00FF00" : "FFA500";
                    text += $"[c/{colorHex}:[晋升仪式] 时光混乱: {status}]\n";
                    text += "  (提示: 在城镇中开启欺瞒领域并维持)\n";
                }

                if (p.currentMarauderSequence <= 1) // 时之虫
                {
                    text += "- [被动] 时间权柄: 免疫大部分时间控制/减速效果\n";
                    text += "- [主动] 时之虫领域 (Shift+P): \n";
                    text += "    > 召唤壁钟虚影，范围内敌人极度减速并持续衰老(掉血)\n";
                    text += "- [主动] 窃取时间 (Shift+O): \n";
                    text += "    > 瞬间剥夺目标生命(老化)，并赋予自身极速\n";
                    text += "- [能力] 错误化身: 死亡时由序列2分身通过窃取命运来复活(强化版)\n";
                }
            }

            return text;
        }
    }
}