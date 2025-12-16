using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using zhashi.Content.Buffs;

namespace zhashi.Content.Globals
{
    public class TamedProjectileGlobal : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        // 添加一个标记，用来区分是否是驯服怪物发射的
        public bool isTamedMinionProjectile = false;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 检查弹幕的来源是否是 NPC
            if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC sourceNPC)
            {
                // 如果发射弹幕的 NPC 拥有“驯服”Buff
                if (sourceNPC.HasBuff(ModContent.BuffType<TamedBuff>()))
                {
                    // 标记为驯服物弹幕
                    isTamedMinionProjectile = true;

                    // 将弹幕转化为友好
                    projectile.friendly = true;
                    projectile.hostile = false;

                    // 赋予弹幕更强的属性 (可选)
                    projectile.penetrate = 2; // 增加穿透
                    projectile.timeLeft += 60; // 飞得更远
                }
            }
        }

        // 双重保险：如果已经是友好的弹幕，不要伤害玩家
        public override bool CanHitPlayer(Projectile projectile, Player target)
        {
            // 只有被标记的弹幕才执行此保护逻辑，防止误伤其他逻辑
            if (isTamedMinionProjectile && projectile.friendly)
            {
                return false;
            }
            return base.CanHitPlayer(projectile, target);
        }

        // 允许被驯服的怪物弹幕伤害其他敌人
        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            // 【核心修复】只对被标记为“驯服怪物发射”的弹幕生效
            // 玩家的武器弹幕不会进入这个判断，从而走原版逻辑（保留无敌帧）
            if (isTamedMinionProjectile && projectile.friendly && !target.friendly)
            {
                return true;
            }
            return null;
        }
    }
}