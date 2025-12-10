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
using zhashi.Content.Items.Potions.Fool;   // 愚者途径

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
            // 如果你想让它随波浪移动或其它特效，可以在这里修改 vector
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
            // 这里逻辑是：显示你当前最高等级的那条途径的图标

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
            if (p.currentSequence <= 9) // 巨人
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
            if (p.currentMoonSequence <= 9) // 月亮
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
            if (p.currentFoolSequence <= 9) // 愚者途径
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

            return 0;
        }

        // --- 辅助方法：生成纯文本介绍 (去掉了之前的 [i:ID] 图标代码，保持整洁) ---
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
                if (p.currentMoonSequence <= 8) text += "- 召唤栏+2 / 移速+30%\n";
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
                    text += "- [技能] 奇迹愿望 (V键, 短按切换/长按实现)\n";
                    text += "- [技能] 干扰命运 (G键, 开启光环/满幸运)\n";
                    text += "- [强化] 历史投影 (上限9 / Shift+Y 召唤历史场景)\n";
                    text += "- [被动] 复活奇迹 (1200灵虫, 死亡清屏)\n";
                }
                if (p.currentFoolSequence <= 1)
                {
                    text += "- [技能] 灵肉转化 (V键, 穿墙/物免)\n";
                    text += "- [技能] 概念嫁接 (G键, 空间反弹/必杀)\n";
                    text += "- [被动] 诡秘之境 (吞噬掉落物/压制敌人)\n";
                    text += "- [被动] 概念不死 (致死伤害自动嫁接)\n";
                }
            }

            return text;
        }
    }
}