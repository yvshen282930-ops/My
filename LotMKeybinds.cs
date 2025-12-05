using Terraria.ModLoader;

namespace zhashi
{
    public class LotMKeybinds : ModSystem
    {
        // 巨人
        public static ModKeybind Ability1;      // Z
        public static ModKeybind GuardianSkill; // X
        public static ModKeybind SilverSkill;   // C

        // 猎人
        public static ModKeybind PyroSkill;     // F
        public static ModKeybind PyroCloak;     // G
        public static ModKeybind PyroBomb;      // H
        public static ModKeybind ConspiratorMove; // V
        public static ModKeybind ReaperSlash;     // J
        public static ModKeybind PyroEnchant;     // T
        public static ModKeybind FireForm;        // K
        public static ModKeybind ArmySkill;       // L
        public static ModKeybind WeatherStrike;   // U
        public static ModKeybind GlacierFreeze;   // I
        public static ModKeybind ConquerorWill;   // Y (征服 - 新增)

        public override void Load()
        {
            Ability1 = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 黎明铠甲", "Z");
            GuardianSkill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 守护姿态", "X");
            SilverSkill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 水银化", "C");

            PyroSkill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 操纵火焰", "F");
            PyroCloak = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 火焰披风", "G");
            PyroBomb = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 火焰炸弹", "H");
            ConspiratorMove = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 火焰闪现", "V");
            PyroEnchant = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 武器附火", "T");
            ReaperSlash = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 屠杀", "J");
            FireForm = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 火焰化", "K");
            ArmySkill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 集众", "L");
            WeatherStrike = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 天气操控", "U");
            GlacierFreeze = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 冰河", "I");

            // 【新增】
            ConquerorWill = KeybindLoader.RegisterKeybind(Mod, "非凡能力: 征服", "Y");
        }

        public override void Unload()
        {
            Ability1 = null; GuardianSkill = null; SilverSkill = null;
            PyroSkill = null; PyroCloak = null; PyroBomb = null; ConspiratorMove = null;
            PyroEnchant = null; ReaperSlash = null; FireForm = null; ArmySkill = null;
            WeatherStrike = null; GlacierFreeze = null; ConquerorWill = null;
        }
    }
}