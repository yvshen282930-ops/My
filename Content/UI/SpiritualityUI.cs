using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.GameInput;
using zhashi.Content;
using zhashi.Content.Configs;

namespace zhashi.Content.UI
{
    public class SpiritualityBarState : UIState
    {
        // === 运行时变量 ===
        private Vector2 currentPos;
        private float currentScale;
        private bool initialized = false;

        // === 拖拽变量 ===
        private bool dragging = false;
        private Vector2 dragOffset; // 记录抓取点

        // === 缓存 ===
        private int textureWidth = 0;
        private int textureHeight = 0;
        private bool isHovering = false;

        // 记录上一帧的鼠标左键状态，用于手写“刚按下”判定
        private bool oldMouseLeft = false;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 1. 状态检查
            if (!modPlayer.IsBeyonder || player.dead || player.ghost)
            {
                dragging = false;
                return;
            }

            // 2. 初始化配置
            if (!initialized)
            {
                currentPos = ZhashiConfig.Instance.BarPosition;
                currentScale = ZhashiConfig.Instance.BarScale;
                initialized = true;
            }

            if (textureWidth == 0) return;

            // === 3. 【核心】坐标系统一 ===
            // 泰拉瑞亚的 UI 坐标 = 屏幕像素 / UIScale
            // 我们统一把鼠标转换到 UI 坐标系下，这样就没有缩放误差了
            Vector2 mouseUISpace = Main.MouseScreen / Main.UIScale;

            // === 4. 判定区域计算 ===
            // 在 UI 坐标系下，HitBox 就是简单的矩形
            Rectangle hitBox = new Rectangle(
                (int)currentPos.X,
                (int)currentPos.Y,
                (int)(textureWidth * currentScale),
                (int)(textureHeight * currentScale)
            );

            // 检查鼠标是否在矩形内
            isHovering = hitBox.Contains(mouseUISpace.ToPoint());

            // 交互锁定：防止拖拽时挥剑
            if (isHovering || dragging)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (!dragging) Main.instance.MouseText("");
            }

            // === 5. 【修复】拖拽逻辑 (偏移锁定法) ===
            bool currentMouseLeft = Main.mouseLeft;
            bool justPressed = currentMouseLeft && !oldMouseLeft; // 这一帧按了 + 上一帧没按 = 刚按下

            if (dragging)
            {
                // A. 正在拖拽
                if (currentMouseLeft)
                {
                    // 新位置 = 当前鼠标UI坐标 - 刚开始抓的偏移量
                    currentPos = mouseUISpace - dragOffset;

                    // 简单的边界限制 (防止拖出屏幕)
                    float maxW = Main.screenWidth / Main.UIScale;
                    float maxH = Main.screenHeight / Main.UIScale;
                    currentPos.X = MathHelper.Clamp(currentPos.X, 0, maxW - hitBox.Width);
                    currentPos.Y = MathHelper.Clamp(currentPos.Y, 0, maxH - hitBox.Height);
                }
                else
                {
                    // B. 松开鼠标
                    dragging = false;
                    SaveConfig();
                }
            }
            else
            {
                // C. 开始拖拽检测
                if (isHovering && justPressed)
                {
                    dragging = true;
                    // 记录“抓哪里了”：偏移量 = 鼠标位置 - UI左上角
                    dragOffset = mouseUISpace - currentPos;
                }
            }

            // === 6. 滚轮缩放 ===
            if (isHovering)
            {
                int scrollDelta = PlayerInput.ScrollWheelDelta;
                if (scrollDelta != 0)
                {
                    if (scrollDelta > 0) currentScale += 0.05f;
                    else currentScale -= 0.05f;

                    if (currentScale < 0.5f) currentScale = 0.5f;
                    if (currentScale > 3.0f) currentScale = 3.0f;

                    SaveConfig();
                }
            }

            // 更新鼠标状态缓存
            oldMouseLeft = currentMouseLeft;
        }

        private void SaveConfig()
        {
            ZhashiConfig.Instance.BarPosition = currentPos;
            ZhashiConfig.Instance.BarScale = currentScale;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (!modPlayer.IsBeyonder || player.dead || player.ghost) return;

            // 加载资源
            Texture2D frameTexture = ModContent.Request<Texture2D>("zhashi/Content/UI/SpiritualityBar_Frame").Value;
            Texture2D fillTexture = ModContent.Request<Texture2D>("zhashi/Content/UI/SpiritualityBar_Fill").Value;

            textureWidth = frameTexture.Width;
            textureHeight = frameTexture.Height;

            // 视觉反馈
            Color drawColor = (isHovering || dragging) ? Color.White : new Color(200, 200, 200, 255);

            // === 绘制 ===
            spriteBatch.End();

            // 构建绘制矩阵
            Matrix transformMatrix =
                Matrix.CreateScale(currentScale) * Matrix.CreateTranslation(currentPos.X, currentPos.Y, 0f) * Main.UIScaleMatrix;

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                transformMatrix
            );

            // 绘制内容 (坐标0,0)
            float quotient = modPlayer.spiritualityCurrent / (float)modPlayer.spiritualityMax;
            quotient = MathHelper.Clamp(quotient, 0f, 1f);

            int fillWidth = (int)(frameTexture.Width * quotient);
            Rectangle sourceRect = new Rectangle(0, 0, fillWidth, frameTexture.Height);
            Rectangle destRect = new Rectangle(0, 0, fillWidth, frameTexture.Height);

            spriteBatch.Draw(fillTexture, destRect, sourceRect, drawColor);
            spriteBatch.Draw(frameTexture, Vector2.Zero, drawColor);

            // 绘制文字
            string text = $"{(int)modPlayer.spiritualityCurrent}/{modPlayer.spiritualityMax}";
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
            Vector2 textPos = new Vector2(frameTexture.Width / 2 - textSize.X / 2, frameTexture.Height / 2 - textSize.Y / 2);
            Color textColor = (isHovering || dragging) ? Color.Gold : Color.White;

            Utils.DrawBorderString(spriteBatch, text, textPos, textColor);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            // 悬停提示
            if (isHovering)
            {
                Main.instance.MouseText($"灵性: {(int)modPlayer.spiritualityCurrent}");
            }
        }
    }

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
            if (_interface != null) _interface.Update(gameTime);
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