using Terraria.ModLoader;

namespace zhashi
{
    public class LotMKeybinds : ModSystem
    {
        // ===================================================
        // 1. 变量声明 (在这里定义所有按键)
        // ===================================================

        // --- 月亮途径 (Moon) ---
        public static ModKeybind Moon_Wings { get; private set; }
        public static ModKeybind Moon_BatSwarm { get; private set; }
        public static ModKeybind Moon_PaperFigurine { get; private set; }
        public static ModKeybind Moon_Gaze { get; private set; }
        public static ModKeybind Moon_Shackles { get; private set; }
        public static ModKeybind Moon_Grenade { get; private set; }
        public static ModKeybind Moon_Elixir { get; private set; }
        public static ModKeybind Moon_Moonlight { get; private set; }
        public static ModKeybind Moon_FullMoon { get; private set; }
        public static ModKeybind Moon_SummonGate { get; private set; }

        // --- 猎人途径 (Hunter/Red Priest) ---
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

        // --- 巨人/战士途径 (Giant/Warrior) ---
        public static ModKeybind Giant_Mercury { get; private set; }
        public static ModKeybind Giant_Armor { get; private set; }
        public static ModKeybind Giant_Guardian { get; private set; }

        // --- 愚者途径 (Fool) [新增] ---
        public static ModKeybind Fool_SpiritVision { get; private set; } // 灵视
        public static ModKeybind Fool_Divination { get; private set; }   // 占卜
        public static ModKeybind Fool_FlameJump { get; private set; }    // 火焰跳跃
        public static ModKeybind Fool_Faceless { get; private set; }     // 无面伪装
        public static ModKeybind Fool_Distort { get; private set; }      // 干扰直觉
        public static ModKeybind Fool_Threads { get; private set; }      // 灵体之线
        public static ModKeybind Fool_Swap { get; private set; }    // 秘偶互换
        public static ModKeybind Fool_Control { get; private set; } // 控灵
        public static ModKeybind Fool_History { get; private set; } // 历史投影
        public static ModKeybind Fool_Borrow { get; private set; }  // 昨日重现
        public static ModKeybind Fool_Miracle { get; private set; }     // 奇迹/愿望
        public static ModKeybind Fool_Grafting { get; private set; }   // 嫁接
        public static ModKeybind Fool_SpiritForm { get; private set; } // 灵肉转化

        // ===================================================
        // 2. 注册按键 (Load)
        // ===================================================
        public override void Load()
        {
            // Moon
            Moon_Wings = KeybindLoader.RegisterKeybind(Mod, "月亮: 黑暗之翼", "F");
            Moon_BatSwarm = KeybindLoader.RegisterKeybind(Mod, "月亮: 蝙蝠化身", "F");
            Moon_PaperFigurine = KeybindLoader.RegisterKeybind(Mod, "月亮: 纸人替身", "J");
            Moon_Gaze = KeybindLoader.RegisterKeybind(Mod, "月亮: 黑暗凝视", "J");
            Moon_Shackles = KeybindLoader.RegisterKeybind(Mod, "月亮: 深渊枷锁", "G");
            Moon_Grenade = KeybindLoader.RegisterKeybind(Mod, "月亮: 炼金手雷", "X");
            Moon_Elixir = KeybindLoader.RegisterKeybind(Mod, "月亮: 生命灵液", "V");
            Moon_Moonlight = KeybindLoader.RegisterKeybind(Mod, "月亮: 月光化", "Z");
            Moon_FullMoon = KeybindLoader.RegisterKeybind(Mod, "月亮: 满月/创生", "C");
            Moon_SummonGate = KeybindLoader.RegisterKeybind(Mod, "月亮: 召唤之门", "K");

            // Hunter
            RP_Transformation = KeybindLoader.RegisterKeybind(Mod, "猎人: 形态切换", "Z");
            RP_Flash = KeybindLoader.RegisterKeybind(Mod, "猎人: 火焰闪现", "F");
            RP_Bomb = KeybindLoader.RegisterKeybind(Mod, "猎人: 炸弹", "X");
            RP_Cloak = KeybindLoader.RegisterKeybind(Mod, "猎人: 火焰披风", "C");
            RP_Slash = KeybindLoader.RegisterKeybind(Mod, "猎人: 收割斩击", "G");
            RP_Enchant = KeybindLoader.RegisterKeybind(Mod, "猎人: 武器附魔", "V");
            RP_Skill = KeybindLoader.RegisterKeybind(Mod, "猎人: 蓄力火球", "Q");
            RP_Army = KeybindLoader.RegisterKeybind(Mod, "猎人: 集众", "Z");
            RP_Weather = KeybindLoader.RegisterKeybind(Mod, "猎人: 天气操控", "P");
            RP_Glacier = KeybindLoader.RegisterKeybind(Mod, "猎人: 冰河世纪", "X");

            // Giant
            Giant_Mercury = KeybindLoader.RegisterKeybind(Mod, "巨人: 水银化", "C");
            Giant_Armor = KeybindLoader.RegisterKeybind(Mod, "巨人: 晨曦之铠", "X");
            Giant_Guardian = KeybindLoader.RegisterKeybind(Mod, "巨人: 守护姿态", "Z");

            // Fool [新增]
            Fool_SpiritVision = KeybindLoader.RegisterKeybind(Mod, "愚者: 灵视开关", "C");
            Fool_Divination = KeybindLoader.RegisterKeybind(Mod, "愚者: 占卜术", "J");
            Fool_FlameJump = KeybindLoader.RegisterKeybind(Mod, "愚者: 火焰跳跃", "F");
            Fool_Faceless = KeybindLoader.RegisterKeybind(Mod, "愚者: 无面伪装", "V");
            Fool_Distort = KeybindLoader.RegisterKeybind(Mod, "愚者: 干扰直觉", "G");
            Fool_Threads = KeybindLoader.RegisterKeybind(Mod, "愚者: 灵体之线", "Z");
            Fool_Swap = KeybindLoader.RegisterKeybind(Mod, "愚者: 秘偶互换", "T");
            Fool_Control = KeybindLoader.RegisterKeybind(Mod, "愚者: 控灵/麻痹", "R");
            Fool_History = KeybindLoader.RegisterKeybind(Mod, "愚者: 历史投影", "Y");
            Fool_Borrow = KeybindLoader.RegisterKeybind(Mod, "愚者: 昨日重现", "U");
            Fool_Miracle = KeybindLoader.RegisterKeybind(Mod, "愚者: 奇迹愿望", "Z");
            Fool_Grafting = KeybindLoader.RegisterKeybind(Mod, "愚者: 嫁接", "G");
            Fool_SpiritForm = KeybindLoader.RegisterKeybind(Mod, "愚者: 灵肉转化", "V");

        }

        // ===================================================
        // 3. 卸载按键 (Unload)
        // ===================================================
        public override void Unload()
        {
            // Moon
            Moon_Wings = null;
            Moon_BatSwarm = null;
            Moon_PaperFigurine = null;
            Moon_Gaze = null;
            Moon_Shackles = null;
            Moon_Grenade = null;
            Moon_Elixir = null;
            Moon_Moonlight = null;
            Moon_FullMoon = null;
            Moon_SummonGate = null;

            // Hunter
            RP_Transformation = null;
            RP_Flash = null;
            RP_Bomb = null;
            RP_Cloak = null;
            RP_Slash = null;
            RP_Enchant = null;
            RP_Skill = null;
            RP_Army = null;
            RP_Weather = null;
            RP_Glacier = null;

            // Giant
            Giant_Mercury = null;
            Giant_Armor = null;
            Giant_Guardian = null;

            // Fool [新增]
            Fool_SpiritVision = null;
            Fool_Divination = null;
            Fool_FlameJump = null;
            Fool_Faceless = null;
            Fool_Distort = null;
            Fool_Threads = null;
            Fool_Swap = null;
            Fool_Control = null;
            Fool_History = null;
            Fool_Borrow = null;
            Fool_Miracle = null;
            Fool_Grafting = null;
            Fool_SpiritForm = null;
        }
    }
}