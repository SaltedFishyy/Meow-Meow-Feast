using System;
using AbilityStudio.Model;

namespace AbilityStudio.Editor
{
    /// <summary>Short designer-facing descriptions for node title hover cards.</summary>
    public static class NodeHelpContent
    {
        /// <summary>Explains a node and its ports. Null represents Skill Core.</summary>
        public static string For(Type type, bool integrated = false)
        {
            if (type != null && typeof(HitFeedbackEffect).IsAssignableFrom(type))
            {
                string description = type == typeof(RecoilFeedback) ? "目标模型朝远离施法者的方向短暂后仰，再恢复原姿态。角度控制幅度，后仰时间和回正时间分别控制两个阶段。只影响外观，不移动根节点、碰撞体，不附加眩晕。连续命中从当前姿态重新后仰，不累积旋转。游戏侧需接自己的模型或动画系统。" :
                    type == typeof(CameraShakeFeedback) ? "命中时请求镜头震动。强度为位移幅度，时间控制衰减时长，频率控制震动快慢。默认每次攻击只震一次。" :
                    type == typeof(HitVfxFeedback) ? "在命中位置加世界偏移处生成特效预制体，按大小倍率缩放，到存活时间后移除。未指定资源时不生成特效。" :
                    "命中时播放所选音效，可调音量和音调。未指定资源时不播放声音。测试场景使用 2D 声音。";
                return description + "\n\n目标 输入（必接）\n接攻击的命中目标、分区目标或拉拽到达目标。不连接不生效，不造成额外伤害。\n\n触发方式\n每次命中：逐个目标触发，穿透和回程命中也可触发。每次攻击首次命中：同一节点每次释放最多触发一次。\n\n最短触发间隔\n同一施法者、同一技能、同一节点跨释放限频；0 表示不限频。间隔内的请求跳过，不排队。\n\nPlayground 自带预览；游戏侧订阅反馈请求，接自己的镜头、特效和音效系统。";
            }
            if (integrated && type == null) return "技能基础提供两个按键入口：普通攻击用左键，特殊攻击用右键。每个入口接一个攻击节点，两边冷却独立。蓄力期间另一攻击键被忽略，Esc 取消。";
            if (integrated && type != null && typeof(AttackModule).IsAssignableFrom(type))
            {
                string shape = type == typeof(CircleAttack) ? "圆形在技能终点附近命中敌人。" : type == typeof(ConeAttack) ? "扇形朝鼠标方向展开，角度固定。" : type == typeof(LineThrustAttack) ? "长方形朝鼠标方向展开，宽度固定。" : "投射物朝鼠标方向飞行。蓄力决定等待时间，不改变投射物速度与射程。";
                return shape + "\n\n攻击设置\n瞬发：按下立即攻击。蓄力：按住后松开攻击，蓄满后保持；范围从起始值增长到蓄满值。切换方式保留隐藏数值。冷却在成功释放后开始。\n\n攻击按键 输入\n接技能基础的普通攻击或特殊攻击，未接时不生效。\n\n起点／圆心 输入（可选）\n不接时，扇形、长方形和投射物从角色出发，圆形位于鼠标附近的技能终点。\n\n技能终点 输出\n受本节点最大施法距离限制的位置，可接圆心或拉拽目的地。\n\n命中目标 输出\n接倍率或其他技能效果的目标。选择连接倍率节点后，由倍率节点计算伤害。" +
                    (type == typeof(LineThrustAttack) ? "\n\n普通区域目标／前端区域目标 输出\n将主体与前端敌人分开，前端区域长度控制强化区深度。跨界敌人只归前端。" : "") +
                    (type == typeof(ProjectileAttack) ? "\n\n命中响应 输出\n接穿透、连锁、返回改变飞行行为。" : "");
            }
            if (type == typeof(InstantRelease)) return "按下攻击键立即释放。\n\n左侧释放方式输入：连接技能基础的释放方式（或普通／特殊攻击释放方式）。\n右侧释放输出：连接技能范围的释放输入。\n伤害、冷却、最大施法距离和施法偏移都在技能范围节点设置。";
            if (type == typeof(ChargeRelease)) return "按住蓄力，松开释放；蓄满后保持，取消不消耗冷却。\n\n左侧释放方式输入接技能基础，右侧释放输出接技能范围。\n蓄力比例输出可连接范围的长度／半径／距离比例，让范围随蓄力增长。伤害、冷却、最大施法距离和施法偏移在范围节点设置。";
            const string targets = "\n\n目标 输入（必接）\n接攻击的 命中目标：命中时生效。\n接 拉拽 的 到达目标：拉拽完成后生效。\n不接线，这个效果不生效。";
            if (type == null) return "选择单攻击或双攻击，设置名称与键位。默认普通攻击为鼠标左键，特殊攻击为鼠标右键。\n\n右侧释放方式接口连接瞬发／蓄力节点的左侧释放方式，再将其释放输出接到技能范围。\n技能终点由当前范围的最大施法距离和施法偏移计算，可接圆心或拉拽目的地。\n切换单攻击会隐藏并停用特殊分支，但保留它的节点、连线和参数。";
            const string area = "\n\n释放 输入（必接）\n接瞬发／蓄力节点右侧的释放；释放方式节点左侧接技能基础。断开后此范围不释放。\n\n释放位置 输入（可选）\n连接位置来源；不连接时使用玩家位置，再叠加本范围的施法偏移。圆心接口仍独立控制圆形的生效位置。\n\n范围数值\n基础伤害、冷却、最大施法距离和施法偏移由本范围独立设置。施法偏移为角色朝向下的本地 XY 偏移。\n\n命中目标 输出\n连接技能效果的目标。";
            if (type == typeof(CircleAttack)) return "圆形默认在释放位置生效；释放位置和圆心都不连接时使用玩家位置，并叠加施法偏移。圆心连接时使用指定圆心；可接技能基础的技能终点或位置来源。\n半径比例可接蓄力比例，从最小半径增长到最大半径。" + area;
            if (type == typeof(ProjectileAttack)) return "向鼠标方向发射投射物。默认击中第一个敌人后消失；可调速度、飞行距离和大小。\n\n命中响应输出\n连接穿透、连锁、返回的“投射物”输入改变飞行行为，可同时连接。先穿透，再连锁，最后返回。" + area;
            if (type == typeof(LineThrustAttack)) return "长方形从本范围的施法起点朝鼠标展开。长度比例可接蓄力比例，让长度从最短增长到最长。\n普通区域目标／前端区域目标可分别连接效果；跨界目标只算前端。" + area;
            if (type == typeof(HealEffect)) return "恢复范围内存活友方（含自身）的生命值，不受护甲影响，不超过生命上限，也不会复活。\n治疗方式可选固定治疗量、基础数值百分比、目标最大生命值百分比；百分比填 100 表示 100%。基础数值读取所属技能范围的基础伤害。\n连接技能范围的命中目标；未接线不生效。治疗不改变范围节点原有的敌方伤害设置；纯治疗时将伤害结算选为“连接倍率节点”且不连接倍率。" + targets;
            if (type == typeof(DamageEffect)) return "造成所属技能范围的基础伤害乘以倍率的直接伤害。100% 为基础伤害，150% 为 1.5 倍；护甲只结算一次，不影响流血。\n技能范围的伤害结算必须选择“连接倍率节点”。未接目标不生效；多个倍率节点会分别结算。" + targets;
            if (type == typeof(ConeAttack)) return "扇形从本范围的施法起点朝鼠标展开。距离比例可接蓄力比例，让距离从最短增长到最长。" + area;
            if (type == typeof(PullEffect)) return "把敌人拉向指定位置。持续时间 是拉拽时间；目的地间距 是到达时与目的地保留的距离，填 0 可拉到该点。" + targets + "\n\n目的地 输入（可选）\n要拉到哪里。接 技能终点 可聚拢到技能终点；不接时拉向命中时的玩家位置。\n\n到达目标 输出\n每个敌人完成拉拽后，把它交给下一个效果。例如接 眩晕 的 目标，实现拉到后眩晕。拉拽被替换或取消时不触发。";
            if (type == typeof(StunEffect)) return "让敌人眩晕，停止主动移动和施法；仍可被拉动或击退。持续时间 控制眩晕秒数。" + targets;
            if (type == typeof(BleedEffect)) return "让敌人持续流血。每秒伤害 控制每秒伤害，持续时间 控制持续秒数。重复施加会刷新，不叠加。" + targets;
            if (type == typeof(SlowEffect)) return "降低敌人的主动移动速度。减速比例 控制减速百分比，持续时间 控制持续秒数。重复施加会刷新，不叠加。" + targets;
            if (type == typeof(KnockbackEffect)) return "把敌人朝远离玩家的方向推开。击退距离 控制推开的距离。" + targets;
            if (type == typeof(SelfPosition)) return "旧版位置节点。\n\n位置输出\n提供施法时的玩家位置。";
            if (type == typeof(MousePosition)) return "旧版位置节点。\n\n位置输出\n提供施法时的鼠标地面位置。";
            if (type == typeof(SkillEndpointPosition)) return "旧版终点节点。\n\n位置输出\n提供从施法位置朝鼠标方向、固定距离处的位置。";
            const string response = "\n\n投射物输入（必接）\n连接投射物的“命中响应”输出；未连接时不生效。每次命中仍触发投射物连接的技能效果。";
            if (type == typeof(PierceResponse)) return "穿过敌人继续直线飞行。穿透次数是额外允许穿过的敌人数，例如 2 可命中最多 3 个。去程同一敌人只命中一次。多个穿透连接到同一投射物时次数相加。" + response;
            if (type == typeof(ChainResponse)) return "穿透用完后，转向命中点附近最近的未命中敌人。连锁次数控制额外跳转次数，搜索距离控制每次跳转范围。多个连锁的次数相加，搜索距离取最大值。" + response;
            if (type == typeof(ReturnResponse)) return "飞行到最远距离或无法继续命中后，返回施法者当前的位置。返回速度倍率乘以投射物原速度。回程可再次命中敌人，每个敌人最多一次，不再连锁。多个返回取最大速度倍率；追赶过远会自动结束。" + response;
            return "此节点暂无说明。";
        }
    }
}
