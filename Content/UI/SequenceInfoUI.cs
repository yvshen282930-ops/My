using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using zhashi.Content;
using System;

namespace zhashi.Content.UI
{
    public class SequenceStatusUI : UIState
    {
        private UIImage sequenceIcon;
        private UIText statusText; // 用于显示简略状态（可选）

        // 上次检测的序列，用于减少Update频率
        private int lastSeq = -100;
        private int lastHunter = -100;
        private int lastMoon = -100;

        public override void OnInitialize()
        {
            // 初始化图标位置 (左上角，向下偏移一点以免遮挡原版条)
            // 使用一个原版贴图作为默认占位符，防止加载失败
            sequenceIcon = new UIImage(Main.Assets.Request<Texture2D>("Images/Item_" + ItemID.FallenStar));
            sequenceIcon.Left.Set(20, 0f);
            sequenceIcon.Top.Set(120, 0f); // 放在血条/魔力条下面
            sequenceIcon.Width.Set(44, 0f);
            sequenceIcon.Height.Set(44, 0f);

            Append(sequenceIcon);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Player player = Main.LocalPlayer;
            if (player == null || !player.active) return;
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 只有当序列发生变化时才尝试切换图标，节省性能
            if (modPlayer.currentSequence != lastSeq ||
                modPlayer.currentHunterSequence != lastHunter ||
                modPlayer.currentMoonSequence != lastMoon)
            {
                UpdateIconTexture(modPlayer);
                lastSeq = modPlayer.currentSequence;
                lastHunter = modPlayer.currentHunterSequence;
                lastMoon = modPlayer.currentMoonSequence;
            }
        }

        private void UpdateIconTexture(LotMPlayer modPlayer)
        {
            string texturePath = "";
            int itemId = 0; // 如果找不到模组贴图，使用原版物品ID作为备选

            // 优先级：月亮 > 猎人 > 巨人
            if (modPlayer.currentMoonSequence <= 9)
            {
                // 这里尝试加载模组魔药贴图，你需要确保路径完全正确
                // 如果不想折腾路径，可以直接用 ItemID.MoonLordTrophy 之类的代替
                texturePath = "zhashi/Content/Items/Potions/Moon/BeautyGoddessPotion"; // 示例：最高级
                itemId = ItemID.MoonStone; // 备选：月亮石
            }
            else if (modPlayer.currentHunterSequence <= 9)
            {
                texturePath = "zhashi/Content/Items/Potions/Hunter/ConquerorPotion";
                itemId = ItemID.RangerEmblem; // 备选：游侠徽章
            }
            else if (modPlayer.currentSequence <= 9)
            {
                texturePath = "zhashi/Content/Items/Potions/GloryPotion";
                itemId = ItemID.WarriorEmblem; // 备选：战士徽章
            }
            else
            {
                // 凡人状态
                sequenceIcon.Color = Color.Transparent; // 隐藏
                return;
            }

            // 尝试加载
            if (ModContent.HasAsset(texturePath))
            {
                sequenceIcon.SetImage(ModContent.Request<Texture2D>(texturePath));
            }
            else
            {
                // 加载备选原版贴图，保证UI不消失
                sequenceIcon.SetImage(Main.Assets.Request<Texture2D>("Images/Item_" + itemId));
            }
            sequenceIcon.Color = Color.White;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // 鼠标悬停显示详细信息
            if (sequenceIcon.IsMouseHovering && sequenceIcon.Color.A > 0)
            {
                Player player = Main.LocalPlayer;
                var modPlayer = player.GetModPlayer<LotMPlayer>();

                string info = GenerateFullStatusText(player, modPlayer);
                Main.instance.MouseText(info);
            }
        }

        // --- 生成超级详细的数据面板 ---
        private string GenerateFullStatusText(Player player, LotMPlayer modPlayer)
        {
            string title = "";
            string colorHex = "";
            string content = "";

            // 1. 确定称号和颜色
            if (modPlayer.currentMoonSequence <= 9)
            {
                int s = modPlayer.currentMoonSequence;
                string[] names = { "", "美神", "创生者", "召唤大师", "巫王", "深红学者", "魔药教授", "吸血鬼", "驯兽师", "药师" };
                title = (s >= 1 && s <= 9) ? names[s] : "月亮途径";
                colorHex = "DC143C"; // 深红

                // 月亮专属数据
                content += $"\n[c/AAAAAA:灵性]: {modPlayer.spiritualityCurrent:F0} / {modPlayer.spiritualityMax}";
                content += $"\n[c/AAAAAA:状态]: {(modPlayer.isBatSwarm ? "蝙蝠化身" : (modPlayer.isMoonlightized ? "月光化" : "正常"))}";

                // 技能列表
                content += "\n\n[c/FFD700:--- 主动技能 ---]";
                if (s <= 9) content += "\n(无主动技, 被动生效)";
                if (s <= 7) content += "\n[K] 黑暗之翼 (飞行)\n[V] 深渊枷锁 (束缚)";
                if (s <= 6) content += "\n[X] 炼金手雷\n[Z] 生命灵液 (治疗)";
                if (s <= 5) content += "\n[G] 满月领域 (强化)\n[C] 月光化 (无敌移动)";
                if (s <= 4) content += "\n[T] 蝙蝠化身 (隐身/飞行)\n[J] 月亮纸人 (替身)\n[F] 黑暗凝视 (单体爆发)";
                if (s <= 3) content += "\n[R] 召唤之门 (召唤大军)";
                if (s <= 2) content += "\n[G] 创生领域 (群体回血/花靴)\n[J] 群体转化 (强控/秒杀)\n[X] 净化大地\n[Z] 生命奇迹 (满血)";
            }
            else if (modPlayer.currentHunterSequence <= 9)
            {
                int s = modPlayer.currentHunterSequence;
                string[] names = { "", "征服者", "天气术士", "战争主教", "铁血骑士", "收割者", "阴谋家", "纵火家", "挑衅者", "猎人" };
                title = (s >= 1 && s <= 9) ? names[s] : "猎人途径";
                colorHex = "FF4500"; // 橙红

                content += $"\n[c/AAAAAA:灵性]: {modPlayer.spiritualityCurrent:F0} / {modPlayer.spiritualityMax}";

                content += "\n\n[c/FFD700:--- 主动技能 ---]";
                if (s <= 8) content += "\n[F] 挑衅";
                if (s <= 7) content += "\n[X] 火焰炸弹\n[Z] 火焰披风\n[F] 操纵火焰";
                if (s <= 6) content += "\n[V] 闪现\n[T] 武器附火";
                if (s <= 5) content += "\n[G] 致命斩击";
                if (s <= 4) content += "\n[K] 火焰形态\n[J] 集众";
                if (s <= 2) content += "\n[K] 灾祸巨人\n[右键] 天气雷击\n[R] 冰河世纪";
            }
            else if (modPlayer.currentSequence <= 9)
            {
                int s = modPlayer.currentSequence;
                string[] names = { "", "未知", "荣耀者", "银骑士", "猎魔者", "守护者", "黎明骑士", "武器大师", "格斗家", "战士" };
                title = (s >= 1 && s <= 9) ? names[s] : "巨人途径";
                colorHex = "C0C0C0"; // 银色

                content += $"\n[c/AAAAAA:灵性]: {modPlayer.spiritualityCurrent:F0} / {modPlayer.spiritualityMax}";

                content += "\n\n[c/FFD700:--- 主动技能 ---]";
                if (s <= 6) content += "\n[Z] 黎明铠甲 (护盾)";
                if (s <= 5) content += "\n[X] 守护姿态 (减伤)";
                if (s <= 3) content += "\n[C] 水银化 (潜行)";
            }

            // 通用属性面板
            string stats = "\n\n[c/FFD700:--- 当前属性加成 ---]";
            stats += $"\n防御力: {player.statDefense}";
            stats += $"\n生命再生: {player.lifeRegen / 2}/s";
            stats += $"\n移动速度: +{(int)((player.moveSpeed - 1f) * 100)}%";
            stats += $"\n伤害倍率: {modPlayer.GetSequenceMultiplier(Math.Min(modPlayer.currentSequence, Math.Min(modPlayer.currentHunterSequence, modPlayer.currentMoonSequence))):F1}x";

            if (modPlayer.currentMoonSequence <= 3 || modPlayer.currentHunterSequence <= 4)
            {
                stats += $"\n召唤栏位: {player.maxMinions}";
                stats += $"\n召唤伤害: +{(int)((player.GetDamage(DamageClass.Summon).Additive - 1f) * 100)}%";
            }

            return $"[c/{colorHex}:{title} (序列{Math.Min(modPlayer.currentSequence, Math.Min(modPlayer.currentHunterSequence, modPlayer.currentMoonSequence))})]{content}{stats}";
        }
    }
}