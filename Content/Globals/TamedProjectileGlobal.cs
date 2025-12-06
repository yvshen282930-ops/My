using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using zhashi.Content.Buffs;

namespace zhashi.Content.Globals
{
    public class TamedProjectileGlobal : GlobalProjectile
    {
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 检查弹幕的来源是否是 NPC
            if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC sourceNPC)
            {
                // 如果发射弹幕的 NPC 拥有“驯服”Buff
                if (sourceNPC.HasBuff(ModContent.BuffType<TamedBuff>()))
                {
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
            if (projectile.friendly && !projectile.hostile)
            {
                return false;
            }
            return true;
        }

        // 允许被驯服怪物的弹幕伤害其他敌人
        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            // 如果弹幕是友好的（被上面的 OnSpawn 修改过），且目标不是友好的
            if (projectile.friendly && !target.friendly)
            {
                return true;
            }
            return null;
        }
    }
}