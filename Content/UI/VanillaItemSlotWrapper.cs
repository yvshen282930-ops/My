using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent; // 【关键引用】
using Terraria.GameInput;
using Terraria.UI;

namespace zhashi.Content.UI
{
    // 这是一个封装了原版物品槽的 UI 控件
    public class VanillaItemSlotWrapper : UIElement
    {
        internal Item Item; // 当前槽位内的物品
        private readonly int _context; // 物品槽上下文 (例如 BankItem, InventoryItem 等)
        private readonly float _scale; // 缩放比例
        internal Func<Item, bool> ValidItemFunc; // 可选：限制能放入的物品规则

        public VanillaItemSlotWrapper(int context = ItemSlot.Context.BankItem, float scale = 1f)
        {
            _context = context;
            _scale = scale;
            Item = new Item();
            Item.SetDefaults(0);

            // 【核心修复】Main.inventoryBackTexture 已废弃，改为 TextureAssets.InventoryBack.Value
            Width.Set(TextureAssets.InventoryBack.Value.Width * scale, 0f);
            Height.Set(TextureAssets.InventoryBack.Value.Height * scale, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // 1. 【核心修复】备份原版缩放 (防止污染全局 UI)
            float oldScale = Main.inventoryScale;
            Main.inventoryScale = _scale;

            // 2. 计算绘制位置
            Rectangle rectangle = GetDimensions().ToRectangle();

            // 3. 处理交互 (点击/放入/取出)
            if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
            {
                Main.LocalPlayer.mouseInterface = true; // 告诉游戏鼠标在 UI 上

                if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem))
                {
                    // ItemSlot.Handle 会处理所有的交换、放入逻辑
                    ItemSlot.Handle(ref Item, _context);
                }
            }

            // 4. 绘制槽位
            ItemSlot.Draw(spriteBatch, ref Item, _context, rectangle.TopLeft());

            // 5. 绘制悬停提示
            if (ContainsPoint(Main.MouseScreen))
            {
                Main.HoverItem = Item.Clone();
                Main.hoverItemName = Item.Name;
            }

            // 6. 【核心修复】还原缩放！
            Main.inventoryScale = oldScale;
        }
    }
}