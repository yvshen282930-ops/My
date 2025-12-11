using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using zhashi.Content;

namespace zhashi.Content.UI
{
    public class SpiritWormBar : UIState
    {
        private UIText text;
        private UIElement area;

        public override void OnInitialize()
        {
            // 1. 设定区域
            area = new UIElement();

            // 设定位置：屏幕底部向上 50 像素 (原来是120)
            area.Left.Set(Main.screenWidth / 2 - 100, 0f);
            area.Top.Set(Main.screenHeight - 50, 0f);
            area.Width.Set(200, 0f);
            area.Height.Set(30, 0f);

            // 2. 添加文本
            text = new UIText("0/0", 0.8f);
            text.Width.Set(200, 0f);
            text.Height.Set(30, 0f);
            text.Top.Set(0, 0f); // 直接紧贴区域顶部
            text.HAlign = 0.5f; // 居中

            // 3. 这里的颜色是文字颜色，默认是白色
            // 如果觉得看不清，可以在 Draw 里动态改

            area.Append(text);
            Append(area);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 判断是否显示 (序列4及以上)
            if (modPlayer.currentFoolSequence > 4 || player.dead || !player.active)
                return;

            // 计算数值
            int maxWorms = 50;
            if (modPlayer.currentFoolSequence <= 2) maxWorms = 1200;
            else if (modPlayer.currentFoolSequence <= 3) maxWorms = 600;

            int currentWorms = modPlayer.spiritWorms;

            // 设置文本
            text.SetText($"灵之虫: {currentWorms} / {maxWorms}");

            // 根据虫子数量变色 (可选，保留一点视觉反馈)
            // 虫子少于 20% 时文字变红，否则为白色
            if (currentWorms < maxWorms * 0.2f)
                text.TextColor = Color.Red;
            else
                text.TextColor = Color.White;

            // 只绘制子元素(也就是文本)，不绘制任何背景条
            base.Draw(spriteBatch);
        }

        // 实时更新位置 (防止调整分辨率后位置不对)
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (area != null)
            {
                area.Left.Set(Main.screenWidth / 2 - 100, 0f);
                // 保持在底部 -50 的位置
                area.Top.Set(Main.screenHeight - 50, 0f);
            }
        }
    }
}