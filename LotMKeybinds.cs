using Terraria.ModLoader;

namespace zhashi.System
{
    public class LotMKeybinds : ModSystem
    {
        // ... (原有变量保持不变) ...
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
        public static ModKeybind Ability1 { get; private set; }
        public static ModKeybind GuardianSkill { get; private set; }

        // 巨人
        public static ModKeybind Giant_Mercury { get; private set; }
        public static ModKeybind Giant_Guardian { get; private set; }
        public static ModKeybind Giant_Armor { get; private set; }

        // 月亮 (原有)
        public static ModKeybind Moon_Wings { get; private set; }
        public static ModKeybind Moon_Shackles { get; private set; }
        public static ModKeybind Moon_Grenade { get; private set; }
        public static ModKeybind Moon_Elixir { get; private set; }
        public static ModKeybind Moon_Moonlight { get; private set; }
        public static ModKeybind Moon_FullMoon { get; private set; }

        // 【新增】月亮 (巫王)
        public static ModKeybind Moon_BatSwarm { get; private set; }     // T键
        public static ModKeybind Moon_PaperFigurine { get; private set; } // J键
        public static ModKeybind Moon_Gaze { get; private set; }          // F键

        // 猎人 (红祭司)
        public static ModKeybind RP_Transformation { get; private set; }
        public static ModKeybind RP_Flash { get; private set; }
        public static ModKeybind RP_Bomb { get; private set; }
        public static ModKeybind RP_Cloak { get; private set; }
        public static ModKeybind RP_Slash { get; private set; }
        public static ModKeybind RP_Enchant { get; private set; }
        public static ModKeybind RP_Skill { get; private set; }
        public static ModKeybind RP_Army { get; private set; }
        public static ModKeybind RP_Weather { get; private set; }
        public static ModKeybind RP_Glacier { get; private set; }

        public override void Load()
        {
            // ... (旧按键注册保持不变) ...
            Giant_Mercury = KeybindLoader.RegisterKeybind(Mod, "巨人: 水银化", "C");
            Giant_Guardian = KeybindLoader.RegisterKeybind(Mod, "巨人: 守护姿态", "X");
            Giant_Armor = KeybindLoader.RegisterKeybind(Mod, "巨人: 黎明铠甲", "Z");
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

            // 月亮注册
            Moon_Wings = KeybindLoader.RegisterKeybind(Mod, "月亮: 黑暗之翼", "K");
            Moon_Shackles = KeybindLoader.RegisterKeybind(Mod, "月亮: 深渊枷锁/闪现", "V");
            Moon_Grenade = KeybindLoader.RegisterKeybind(Mod, "月亮: 炼金手雷", "X");
            Moon_Elixir = KeybindLoader.RegisterKeybind(Mod, "月亮: 生命灵液", "Z");
            Moon_Moonlight = KeybindLoader.RegisterKeybind(Mod, "月亮: 月光化", "C");
            Moon_FullMoon = KeybindLoader.RegisterKeybind(Mod, "月亮: 满月领域", "G");

            // 【新增注册】
            Moon_BatSwarm = KeybindLoader.RegisterKeybind(Mod, "月亮: 蝙蝠化身", "T");
            Moon_PaperFigurine = KeybindLoader.RegisterKeybind(Mod, "月亮: 月亮纸人", "J");
            Moon_Gaze = KeybindLoader.RegisterKeybind(Mod, "月亮: 黑暗凝视", "F");
        }

        public override void Unload()
        {
            FireForm = null; ConspiratorMove = null; WeatherStrike = null; GlacierFreeze = null; ArmySkill = null; ReaperSlash = null; PyroBomb = null; PyroCloak = null; PyroEnchant = null; PyroSkill = null; SilverSkill = null; Ability1 = null; GuardianSkill = null;
            RP_Transformation = null; RP_Flash = null; RP_Bomb = null; RP_Cloak = null; RP_Slash = null; RP_Enchant = null; RP_Skill = null; RP_Army = null; RP_Weather = null; RP_Glacier = null;
            Giant_Mercury = null; Giant_Guardian = null; Giant_Armor = null;

            Moon_Wings = null; Moon_Shackles = null; Moon_Grenade = null; Moon_Elixir = null; Moon_Moonlight = null; Moon_FullMoon = null;

            Moon_BatSwarm = null; Moon_PaperFigurine = null; Moon_Gaze = null;
        }
    }
}