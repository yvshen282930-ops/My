using Terraria.ModLoader;

namespace zhashi.System
{
    public class LotMKeybinds : ModSystem
    {
        // --- 猎人 (红祭司) 途径 ---
        public static ModKeybind RP_Transformation { get; private set; }  // K: 火焰化/巨人
        public static ModKeybind RP_Flash { get; private set; }           // V: 闪现
        public static ModKeybind RP_Bomb { get; private set; }            // X: 火焰炸弹
        public static ModKeybind RP_Cloak { get; private set; }           // Z: 火焰披风
        public static ModKeybind RP_Slash { get; private set; }           // G: 斩击
        public static ModKeybind RP_Enchant { get; private set; }         // T: 附魔
        public static ModKeybind RP_Skill { get; private set; }           // F: 操纵火焰/挑衅
        public static ModKeybind RP_Army { get; private set; }            // J: 集众
        public static ModKeybind RP_Weather { get; private set; }         // Mouse2: 天气
        public static ModKeybind RP_Glacier { get; private set; }         // R: 冰河

        // --- 巨人 途径 ---
        public static ModKeybind Giant_Mercury { get; private set; }      // C: 水银化
        public static ModKeybind Giant_Guardian { get; private set; }     // X: 守护姿态
        public static ModKeybind Giant_Armor { get; private set; }        // Z: 黎明铠甲

        // --- 月亮 途径 ---
        public static ModKeybind Moon_Wings { get; private set; }         // K: 黑暗之翼
        public static ModKeybind Moon_Shackles { get; private set; }      // V: 深渊枷锁/闪现
        public static ModKeybind Moon_Grenade { get; private set; }       // X: 炼金手雷
        public static ModKeybind Moon_Elixir { get; private set; }        // Z: 生命灵液
        public static ModKeybind Moon_Moonlight { get; private set; }     // C: 月光化
        public static ModKeybind Moon_FullMoon { get; private set; }      // G: 满月

        public override void Load()
        {
            // --- 猎人注册 ---
            RP_Transformation = KeybindLoader.RegisterKeybind(Mod, "红祭司: 形态切换 (火焰/巨人)", "K");
            RP_Flash = KeybindLoader.RegisterKeybind(Mod, "红祭司: 闪现", "V");
            RP_Bomb = KeybindLoader.RegisterKeybind(Mod, "红祭司: 火焰炸弹", "X");
            RP_Cloak = KeybindLoader.RegisterKeybind(Mod, "红祭司: 火焰披风", "Z");
            RP_Slash = KeybindLoader.RegisterKeybind(Mod, "红祭司: 收割/斩击", "G");
            RP_Enchant = KeybindLoader.RegisterKeybind(Mod, "红祭司: 武器附魔", "T");
            RP_Skill = KeybindLoader.RegisterKeybind(Mod, "红祭司: 操纵火焰/挑衅", "F");
            RP_Army = KeybindLoader.RegisterKeybind(Mod, "红祭司: 集众", "J");
            RP_Weather = KeybindLoader.RegisterKeybind(Mod, "红祭司: 雷击", "Mouse2");
            RP_Glacier = KeybindLoader.RegisterKeybind(Mod, "红祭司: 冰河", "R");

            // --- 巨人注册 ---
            Giant_Mercury = KeybindLoader.RegisterKeybind(Mod, "巨人: 水银化", "C");
            Giant_Guardian = KeybindLoader.RegisterKeybind(Mod, "巨人: 守护姿态", "X");
            Giant_Armor = KeybindLoader.RegisterKeybind(Mod, "巨人: 黎明铠甲", "Z");

            // --- 月亮注册 ---
            Moon_Wings = KeybindLoader.RegisterKeybind(Mod, "月亮: 黑暗之翼", "K");
            Moon_Shackles = KeybindLoader.RegisterKeybind(Mod, "月亮: 深渊枷锁/闪现", "V");
            Moon_Grenade = KeybindLoader.RegisterKeybind(Mod, "月亮: 炼金手雷", "X");
            Moon_Elixir = KeybindLoader.RegisterKeybind(Mod, "月亮: 生命灵液", "Z");
            Moon_Moonlight = KeybindLoader.RegisterKeybind(Mod, "月亮: 月光化", "C");
            Moon_FullMoon = KeybindLoader.RegisterKeybind(Mod, "月亮: 满月领域", "G");
        }

        public override void Unload()
        {
            RP_Transformation = null; RP_Flash = null; RP_Bomb = null; RP_Cloak = null;
            RP_Slash = null; RP_Enchant = null; RP_Skill = null; RP_Army = null;
            RP_Weather = null; RP_Glacier = null;

            Giant_Mercury = null; Giant_Guardian = null; Giant_Armor = null;

            Moon_Wings = null; Moon_Shackles = null; Moon_Grenade = null;
            Moon_Elixir = null; Moon_Moonlight = null; Moon_FullMoon = null;
        }
    }
}