using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace zhashi.Content.UI
{
    // 这是一个封装了泰拉瑞亚原生 ItemSlot 的 UI 元素
    // 修复了可能导致空指针崩溃的 Bug
    public class VanillaItemSlotWrapper : UIElement
    {
        internal Item Item;
        private readonly int _context;
        private readonly float _scale;
        public Func<Item, bool> ValidItemFunc;

        public VanillaItemSlotWrapper(int context = ItemSlot.Context.BankItem, float scale = 1f)
        {
            _context = context;
            _scale = scale;
            Item = new Item();
            Item.SetDefaults(0);

            // 设置 UI 元素的宽高，防止点击判定偏移
            Width.Set(TextureAssets.InventoryBack9.Value.Width * scale, 0f);
            Height.Set(TextureAssets.InventoryBack9.Value.Height * scale, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // 1. 确保 Item 绝对不为空，防止崩溃
            if (Item == null)
            {
                Item = new Item();
                Item.SetDefaults(0);
            }

            float oldScale = Main.inventoryScale;
            Main.inventoryScale = _scale;

            // 2. 获取位置
            Rectangle rectangle = GetDimensions().ToRectangle();

            // 3. 处理鼠标交互 (放入/取出物品)
            if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
            {
                Main.LocalPlayer.mouseInterface = true;

                // 处理点击交互
                if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem))
                {
                    // 使用原生方法处理交互，自带音效和逻辑
                    ItemSlot.Handle(ref Item, _context);
                }
            }

            // 4. 绘制背景和物品
            // 使用 try-catch 包裹绘制过程，防止因为贴图缺失导致的闪屏
            try
            {
                ItemSlot.Draw(spriteBatch, ref Item, _context, rectangle.TopLeft());
            }
            catch
            {
                // 如果绘制失败，画一个红叉或者直接跳过，不要崩游戏
            }

            Main.inventoryScale = oldScale;
        }
    }
}