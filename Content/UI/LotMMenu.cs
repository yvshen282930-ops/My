using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;

namespace zhashi.Content.UI
{
    public class LotMMenu : ModMenu
    {
        // 1. 设置这个主题在游戏设置里显示的名字
        public override string DisplayName => "诡秘之主 (Lord of the Mysteries)";

        // 2. 设置主界面 Logo
        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("zhashi/Assets/Textures/MenuLogo");

        // 3. 设置主界面 音乐
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/MenuTheme");

        // 4. 设置背景图片 (绘制一张全屏图)
        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            // 获取背景图片
            Texture2D bgTexture = ModContent.Request<Texture2D>("zhashi/Assets/Textures/MenuBackground").Value;

            // 计算缩放比例，确保图片填满屏幕
            // 无论屏幕多大，图片都会铺满
            Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            Vector2 textureCenter = new Vector2(bgTexture.Width / 2f, bgTexture.Height / 2f);

            // 简单的全屏绘制逻辑
            spriteBatch.Draw(
                bgTexture,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                Color.White
            );

            // 返回 true，让游戏继续绘制 Logo
            // 如果你想自己手动画 Logo，这里可以返回 false
            return true;
        }
    }
}