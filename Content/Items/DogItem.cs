using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using zhashi.Content.NPCs;

namespace zhashi.Content.Items
{
    public class DogItem : ModItem
    {
        // 这里的变量充当“存档”的作用
        public string DogName = "旺财";
        public int DogPathway = 0;
        public int DogSequence = 10;
        public int BonusMaxLife = 0;
        public Item[] Equipment;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.maxStack = 1; // 唯一
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false; // 不消耗
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddCondition(new Condition(
                    Language.GetOrRegister("Mods.zhashi.Conditions.DogNotOwned", () => "未拥有/未召唤"),
                    () => !Main.LocalPlayer.HasItem(Type) && !IsDogActive(Main.LocalPlayer)
                ))
                .Register();
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;

            // 检查狗是否活着
            NPC existingDog = GetActiveDog(player);

            // ------------------------------------------------
            // 情况 A：狗还活着 -> 传送
            // ------------------------------------------------
            if (existingDog != null)
            {
                existingDog.Center = player.Center;
                existingDog.velocity = Vector2.Zero;
                if (existingDog.ModNPC is DogNPC dogScript)
                {
                    dogScript.attackState = 0;
                    dogScript.isStaying = false;
                }
                Main.NewText($"{GetDogNameDisplay()} 回到了你身边！", 100, 255, 100);
                return true;
            }

            // ------------------------------------------------
            // 情况 B：狗死了 -> 消耗材料复活
            // ------------------------------------------------

            // 检查材料
            int boneCount = player.CountItem(ItemID.Bone);
            int fleshCount = player.CountItem(ItemID.RottenChunk);

            if (boneCount < 100 || fleshCount < 100)
            {
                Main.NewText($"无法复活！缺少复活材料：", Color.Red);
                if (boneCount < 100) Main.NewText($"- 骨头: {boneCount}/100", Color.Red);
                if (fleshCount < 100) Main.NewText($"- 腐肉: {fleshCount}/100", Color.Red);
                return true;
            }

            // 消耗材料
            for (int i = 0; i < 100; i++) player.ConsumeItem(ItemID.Bone);
            for (int i = 0; i < 100; i++) player.ConsumeItem(ItemID.RottenChunk);

            // 召唤新狗
            int index = NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X, (int)player.Center.Y, ModContent.NPCType<DogNPC>());

            if (Main.npc[index].ModNPC is DogNPC newDog)
            {
                // 【核心修复】读取物品自身 (this) 的数据注入给新狗
                // 之前如果这里读取的是 player.GetModPlayer<LotMPlayer>()，名字可能没更新

                newDog.MyName = this.DogName; // 确保名字传递过去！
                newDog.currentPathway = this.DogPathway;
                newDog.currentSequence = this.DogSequence;
                newDog.BonusMaxHP = this.BonusMaxLife;
                newDog.OwnerName = player.name;

                // 恢复装备
                if (this.Equipment != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (this.Equipment[i] != null) newDog.DogInventory[i] = this.Equipment[i].Clone();
                    }
                }

                // 初始化血量
                newDog.NPC.lifeMax = 300 + newDog.BonusMaxHP + (newDog.currentSequence < 10 ? (10 - newDog.currentSequence) * 30 : 0);
                newDog.NPC.life = newDog.NPC.lifeMax;

                // 标记为已初始化
                newDog.isInitialized = true;

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);

                Main.NewText($"{newDog.MyName} 在血肉与白骨的献祭中重生了！", 200, 50, 255);
            }

            return true;
        }

        // --- 信息显示 ---
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            NPC liveDog = GetActiveDog(Main.LocalPlayer);
            DogNPC dogScript = liveDog?.ModNPC as DogNPC;

            // 优先显示活狗数据，否则显示物品存档数据
            string displayName = (dogScript != null) ? dogScript.MyName : this.DogName;
            int pathway = (dogScript != null) ? dogScript.currentPathway : this.DogPathway;
            int sequence = (dogScript != null) ? dogScript.currentSequence : this.DogSequence;

            string status = (dogScript != null)
                ? "[c/00FF00:存活 (点击传送)]"
                : "[c/FF0000:死亡 (需100骨头+100腐肉复活)]";

            string pathwayName = "凡狗";
            switch (pathway) { case 1: pathwayName = "巨人"; break; case 2: pathwayName = "猎人"; break; case 3: pathwayName = "月亮"; break; case 4: pathwayName = "愚者"; break; case 5: pathwayName = "错误"; break; }
            string seqText = sequence < 10 ? $"[{pathwayName} 序列{sequence}]" : "(未开启途径)";

            tooltips.Add(new TooltipLine(Mod, "DogStatus", $"状态: {status}"));
            tooltips.Add(new TooltipLine(Mod, "DogInfo", $"[c/FFA500:{displayName}] {seqText}"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "据说这是霍尔伯爵买猎狗时的附赠品。") { OverrideColor = Color.Gray });
        }

        private string GetDogNameDisplay()
        {
            NPC liveDog = GetActiveDog(Main.LocalPlayer);
            if (liveDog != null && liveDog.ModNPC is DogNPC script) return script.MyName;
            return DogName;
        }

        private NPC GetActiveDog(Player player)
        {
            foreach (var n in Main.npc)
            {
                if (n.active && n.type == ModContent.NPCType<DogNPC>() && n.ModNPC is DogNPC dog)
                {
                    if (dog.OwnerName == player.name) return n;
                }
            }
            return null;
        }
        private bool IsDogActive(Player player) => GetActiveDog(player) != null;

        // --- 存档读写 (保证退出游戏名字不丢) ---
        public override void SaveData(TagCompound tag)
        {
            tag["DogName"] = DogName; // 保存名字
            tag["DogPathway"] = DogPathway;
            tag["DogSequence"] = DogSequence;
            tag["BonusMaxLife"] = BonusMaxLife;
            if (Equipment != null)
            {
                for (int i = 0; i < Equipment.Length; i++)
                {
                    if (Equipment[i] == null) Equipment[i] = new Item();
                    tag[$"Equip_{i}"] = ItemIO.Save(Equipment[i]);
                }
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("DogName")) DogName = tag.GetString("DogName");
            if (tag.ContainsKey("DogPathway")) DogPathway = tag.GetInt("DogPathway");
            if (tag.ContainsKey("DogSequence")) DogSequence = tag.GetInt("DogSequence");
            if (tag.ContainsKey("BonusMaxLife")) BonusMaxLife = tag.GetInt("BonusMaxLife");
            Equipment = new Item[3];
            for (int i = 0; i < 3; i++)
            {
                if (tag.ContainsKey($"Equip_{i}")) Equipment[i] = ItemIO.Load(tag.GetCompound($"Equip_{i}"));
                else { Equipment[i] = new Item(); Equipment[i].SetDefaults(0); }
            }
        }
    }
}