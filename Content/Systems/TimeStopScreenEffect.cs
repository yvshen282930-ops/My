using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace zhashi.Content.Systems
{
    public class TimeStopScreenEffect : ModSystem
    {
        public static bool IsActive = false;
        public static bool LoadedSuccessfully = false;

        public override void Load() { LoadedSuccessfully = false; }
        public override void Unload() { IsActive = false; LoadedSuccessfully = false; }

        private static void AttemptLoadShader()
        {
            if (LoadedSuccessfully) return;

            try
            {
                // 1. 获取模组实例
                Mod myMod = ModLoader.GetMod("zhashi");

                // 2. 尝试加载 (使用相对路径)
                string loadPath = "Content/Effects/ZaWarudoShader";

                // ImmediateLoad 会立即检查文件是否编译成功
                Asset<Effect> shaderAsset = myMod.Assets.Request<Effect>(loadPath, AssetRequestMode.ImmediateLoad);

                if (shaderAsset != null && shaderAsset.Value != null)
                {
                    ScreenShaderData shaderData = new ScreenShaderData(shaderAsset, "ZaWarudoPass");
                    Filter filter = new Filter(shaderData, EffectPriority.VeryHigh);

                    var field = typeof(EffectManager<Filter>).GetField("_effects", BindingFlags.Instance | BindingFlags.NonPublic);
                    var dict = field?.GetValue(Filters.Scene) as Dictionary<string, Filter>;
                    if (dict != null)
                    {
                        dict["ZaWarudoFilter"] = filter;
                        LoadedSuccessfully = true;
                        Main.NewText(">> 蓝屏测试成功！Shader已编译！ <<", 50, 100, 255);
                    }
                }
            }
            catch (System.Exception e)
            {
                // 依然报错说明：文件里肯定还有脏东西，或者 bin/obj 没删干净
                Main.NewText($"[依然失败] TML 拒绝编译此文件: {e.Message}", 255, 50, 50);
            }
        }

        public static void Activate(Vector2 center)
        {
            if (!LoadedSuccessfully) AttemptLoadShader();

            if (!LoadedSuccessfully) return;

            IsActive = true;
            try
            {
                if (Filters.Scene["ZaWarudoFilter"] != null)
                {
                    if (Filters.Scene["ZaWarudoFilter"].IsActive())
                        Filters.Scene["ZaWarudoFilter"].Deactivate();

                    Filters.Scene.Activate("ZaWarudoFilter");
                }
            }
            catch { }
        }

        public override void PostUpdateEverything()
        {
            // 只要激活了，就不用管参数，直接显示蓝色
            if (!IsActive || !LoadedSuccessfully) return;
        }
    }
}