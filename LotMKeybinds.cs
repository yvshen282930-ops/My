using Terraria.ModLoader;

namespace zhashi.System
{
    public class LotMKeybinds : ModSystem
    {
        //原有按键
        public static ModKeybind FireForm { get; private set; }
        public static ModKeybind ConspiratorMove { get; private set; }
        public static ModKeybind WeatherStrike { get; private set; }
        public static ModKeybind GlacierFreeze { get; private set; }
        public static ModKeybind ArmySkill { get; private set; }
        public static ModKeybind ReaperSlash { get; private set; }
        public static ModKeybind PyroBomb { get; private set; }
        public static ModKeybind PyroCloak { get; private set; }
        public static ModKeybind PyroEnchant { get; private set; }
        public static ModKeybind PyroSkill { get; private set; }
        public static ModKeybind SilverSkill { get; private set; }
        public static ModKeybind Ability1 { get; private set; } // 黎明铠甲
        public static ModKeybind GuardianSkill { get; private set; }

        // 【新增】月亮途径专属按键
        public static ModKeybind DarknessWings { get; private set; }
        public static ModKeybind AbyssShackles { get; private set; }

        public override void Load()
        {
            // 注册原有按键 (保持你原有的键位设置)
            FireForm = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 火焰化/巨人形态", "K");
            ConspiratorMove = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 闪现", "V");
            WeatherStrike = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 天气操控", "Mouse2");
            GlacierFreeze = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 冰河", "R");
            ArmySkill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 集众", "J");
            ReaperSlash = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 斩击", "G");
            PyroBomb = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 火焰炸弹", "X");
            PyroCloak = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 火焰披风", "Z");
            PyroEnchant = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 武器附火", "T");
            PyroSkill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 操纵火焰", "F");
            SilverSkill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 水银化", "C");
            Ability1 = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 黎明铠甲", "Z");
            GuardianSkill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 守护姿态", "X");

            // 【新增】注册新按键
            // 默认键位设为 K 和 V，玩家可以在游戏设置里自己改
            DarknessWings = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 黑暗之翼", "K");
            AbyssShackles = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 深渊枷锁", "V");
        }

        public override void Unload()
        {
            FireForm = null;
            ConspiratorMove = null;
            WeatherStrike = null;
            GlacierFreeze = null;
            ArmySkill = null;
            ReaperSlash = null;
            PyroBomb = null;
            PyroCloak = null;
            PyroEnchant = null;
            PyroSkill = null;
            SilverSkill = null;
            Ability1 = null;
            GuardianSkill = null;

            DarknessWings = null;
            AbyssShackles = null;
        }
    }
}