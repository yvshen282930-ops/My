using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using zhashi.Content;
using zhashi.Content.NPCs.Town;

namespace zhashi.Content.UI.Profiles
{
    public class OldNeilProfile : GalgameProfile
    {
        // ★★★ 调试步骤 1：先强制用邓恩的图片 ★★★
        // 如果这时候能显示立绘，说明你的 OldNeil_Head.png 路径或文件名绝对有错！
        public override string HeadTexture => "zhashi/Content/UI/Portraits/Dunn_Head";
        public override string StandingTexture => "zhashi/Content/UI/Portraits/Dunn_Standing";

        public override float OffsetX => 0f;
        public override float OffsetY => 0f;

        public override string GetDialogue(NPC npc)
        {
            // 打印调试信息
            Main.NewText("调试：正在读取对话...", 100, 200, 255);
            return "这是调试模式。如果你能看到邓恩的脸，说明 UI 系统没坏，是你之前的老尼尔图片路径填错了，或者图片格式不对。";
        }

        public override void SetupButtons(NPC npc, GalgameDialogueUI ui)
        {
            Main.NewText("调试：正在生成按钮...", 100, 200, 255);

            // ★★★ 调试步骤 2：删除所有 if 判断，强制显示按钮 ★★★
            // 即使你之前领过魔药，这个按钮也必须显示出来。

            ui.AddButton("【测试】给我一瓶药水", () => {
                Main.LocalPlayer.QuickSpawnItem(npc.GetSource_GiftOrReward(), ItemID.LesserHealingPotion);
                ui.SetDialogueText("测试成功！功能是正常的。");
            });

            ui.AddButton("离开", () => {
                Main.LocalPlayer.SetTalkNPC(-1);
                ui.Close();
            });
        }
    }
}