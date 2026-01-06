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

            // 如果还没加载出贴图，先用默认尺寸算，或者跳过
            // 这里我们用你提供的 321x100 作为兜底，防止第一帧跳过导致逻辑断层
            float baseW = textureWidth > 0 ? textureWidth : 321f;
            float baseH = textureHeight > 0 ? textureHeight : 100f;

            // === 3. 【核心】坐标系统一 ===
            Vector2 mouseUISpace = Main.MouseScreen / Main.UIScale;

            // === 4. 【修复】精细化判定区域 ===
            // 既然贴图是 321x100，我们定义一个缩减量（Padding）
            // 假设上下左右各缩进 10-20 像素，只保留中间核心区域
            // 你可以根据实际手感调整这两个数字
            float shrinkX = 40f; // 左右总共缩减 40 像素 (左20 右20)
            float shrinkY = 30f; // 上下总共缩减 30 像素 (上15 下15)

            // 计算实际绘制尺寸
            float drawW = baseW * currentScale;
            float drawH = baseH * currentScale;

            // 计算缩减后的判定尺寸 (不能小于 10x10)
            float hitW = drawW - (shrinkX * currentScale);
            float hitH = drawH - (shrinkY * currentScale);
            if (hitW < 10) hitW = 10;
            if (hitH < 10) hitH = 10;

            // 计算判定框的偏移 (让它居中)
            float offsetX = (drawW - hitW) / 2f;
            float offsetY = (drawH - hitH) / 2f;

            // 生成最终的判定矩形 (HitBox)
            Rectangle hitBox = new Rectangle(
                (int)(currentPos.X + offsetX),
                (int)(currentPos.Y + offsetY),
                (int)hitW,
                (int)hitH
            );

            // 检查鼠标是否在缩小后的矩形内
            isHovering = hitBox.Contains(mouseUISpace.ToPoint());

            // 交互锁定：防止拖拽时挥剑
            // 只有当真正悬停在“核心区域”时才阻断操作
            if (isHovering || dragging)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (!dragging) Main.instance.MouseText("");
            }

            // === 5. 拖拽逻辑 (保持不变) ===
            bool currentMouseLeft = Main.mouseLeft;
            bool justPressed = currentMouseLeft && !oldMouseLeft;

            if (dragging)
            {
                if (currentMouseLeft)
                {
                    // 核心修正：currentPos 是绘制的左上角，不是 HitBox 的左上角
                    // 拖拽逻辑需要保持 currentPos 的相对位置不变
                    // 这里的 dragOffset 是基于 (鼠标 - 绘制左上角) 计算的，所以逻辑不用变
                    currentPos = mouseUISpace - dragOffset;

                    float maxW = Main.screenWidth / Main.UIScale;
                    float maxH = Main.screenHeight / Main.UIScale;
                    // 限制范围时，还是用绘制尺寸来限制比较自然
                    currentPos.X = MathHelper.Clamp(currentPos.X, 0, maxW - drawW);
                    currentPos.Y = MathHelper.Clamp(currentPos.Y, 0, maxH - drawH);
                }
                else
                {
                    dragging = false;
                    SaveConfig();
                }
            }
            else
            {
                if (isHovering && justPressed)
                {
                    dragging = true;
                    // 记录偏移量：鼠标相对于“绘制左上角”的位置
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