using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using zhashi.Content; // 引用 LotMPlayer

namespace zhashi.Content.UI
{
    // 1. UI 状态类：负责画图
    public class SpiritualityBarState : UIState
    {
        // 每一帧都会调用这个方法来绘制
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 如果不是非凡者，或者是主菜单，就不显示
            if (!modPlayer.IsBeyonder || player.dead || player.ghost)
                return;

            // === 1. 加载贴图 ===
            // 确保你已经把图片放在 zhashi/Content/UI/ 文件夹下，且名字完全一致
            // Request<Texture2D> 会自动加载图片
            Texture2D frameTexture = ModContent.Request<Texture2D>("zhashi/Content/UI/SpiritualityBar_Frame").Value;
            Texture2D fillTexture = ModContent.Request<Texture2D>("zhashi/Content/UI/SpiritualityBar_Fill").Value;

            // === 2. 计算位置 (调整到你红圈的位置) ===
            // 泰拉瑞亚原版血条大概在右边 300-400 像素的位置
            // 我们设置在 屏幕宽度 - 500，这样就在血条左边了
            int screenX = Main.screenWidth - 700;
            int screenY = 20; // 距离顶部 20 像素

            // 获取图片的大小
            int width = frameTexture.Width;
            int height = frameTexture.Height;

            // 定义整个条的矩形位置
            Rectangle barRect = new Rectangle(screenX, screenY, width, height);

            // === 3. 计算灵性比例 ===
            float quotient = modPlayer.spiritualityCurrent / (float)modPlayer.spiritualityMax;
            // 限制在 0 到 1 之间
            if (quotient < 0) quotient = 0;
            if (quotient > 1) quotient = 1;

            // === 4. 绘制填充条 (Fill) ===
            // 核心技巧：使用 sourceRectangle (源矩形) 来裁剪图片
            // 我们只画图片左边的 "quotient" 比例部分
            int fillWidth = (int)(width * quotient);

            // sourceRect: 从图片(0,0)开始，切多宽
            Rectangle sourceRect = new Rectangle(0, 0, fillWidth, height);
            // destRect: 画在屏幕上的什么位置，画多宽
            Rectangle destRect = new Rectangle(screenX, screenY, fillWidth, height);

            // 绘制填充层 (颜色设为白色，表示使用图片原色)
            spriteBatch.Draw(fillTexture, destRect, sourceRect, Color.White);

            // === 5. 绘制边框 (Frame) ===
            // 边框画在上面，盖住边缘
            spriteBatch.Draw(frameTexture, barRect, Color.White);

            // === 6. 绘制文字数值 (可选) ===
            // 如果你想在条中间显示数字
            string text = $"{(int)modPlayer.spiritualityCurrent}/{modPlayer.spiritualityMax}";
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
            // 文字居中计算
            Vector2 textPos = new Vector2(screenX + width / 2 - textSize.X / 2, screenY + height / 2 - textSize.Y / 2);

            // 绘制文字 (白色，带黑边)
            Utils.DrawBorderString(spriteBatch, text, textPos, Color.White);

            // 鼠标悬停显示提示 (可选)
            if (barRect.Contains(Main.MouseScreen.ToPoint()))
            {
                Main.instance.MouseText($"灵性: {(int)modPlayer.spiritualityCurrent}");
            }
        }
    }

    // 2. ModSystem 类：负责管理 UI 何时加载、何时显示
    public class SpiritualityUISystem : ModSystem
    {
        private UserInterface _interface;
        private SpiritualityBarState _barState;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _interface = new UserInterface();
                _barState = new SpiritualityBarState();
                _barState.Activate();
                _interface.SetState(_barState);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (_interface != null)
            {
                _interface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "zhashi: Spirituality Bar",
                    delegate {
                        _interface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}