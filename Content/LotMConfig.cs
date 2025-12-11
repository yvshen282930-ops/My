using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace zhashi.Content
{
    public class LotMConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("平衡性设置")]

        [Label("启用动态世界等级")]
        [Tooltip("根据当前世界击败的Boss（包括灾厄/瑟银等）动态调整属性。\n建议开启以获得最佳体验。")]
        [DefaultValue(true)]
        public bool DynamicProgression;

        [Label("机制软化")]
        [Tooltip("将“绝对无敌/秒杀”替换为“高额减伤/百分比伤害”。\n关闭此项将回归变态强度。")]
        [DefaultValue(true)]
        public bool NerfDivineAbilities;

        [Label("属性强度倍率")]
        [Tooltip("全局调整模组增加的生命值、伤害和防御。\n1.0 为标准，0.5 为减半。")]
        [Range(0.1f, 5.0f)]
        [DefaultValue(1.0f)]
        [Slider]
        public float GlobalPowerMultiplier;
    }
}