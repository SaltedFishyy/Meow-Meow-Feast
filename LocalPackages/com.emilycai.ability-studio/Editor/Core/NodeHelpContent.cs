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
            if (type == typeof(InstantRelease)) return "按下攻击键立即释放。\n\n释放方式输出\n武器模式下：攻击入口接技能基础的普通／特殊攻击；释放输出接技能范围的释放输入；技能终点提供受本分支距离限制的位置。伤害、冷却和距离在此独立配置。未接攻击入口时不生效。旧版模式接技能基础的释放方式。";
            if (type == typeof(ChargeRelease)) return "按住蓄力，松开释放。最长蓄力时间决定多久达到 100%，满蓄后继续保持直到松手。取消不攻击、不消耗冷却。\n\n释放方式输出\n武器模式下：攻击入口接技能基础的普通／特殊攻击，释放输出接技能范围的释放输入，技能终点提供本分支的位置。伤害、冷却与距离独立。旧版模式接技能基础的释放方式，启用蓄力。\n\n蓄力比例输出\n从 0 到 100%。接长方形的长度比例、圆形的半径比例或扇形的距离比例，让范围随蓄力增长；未被核心选中的蓄力节点输出 0。";
            const string targets = "\n\n目标 输入（必接）\n接攻击的 命中目标：命中时生效。\n接 拉拽 的 到达目标：拉拽完成后生效。\n不接线，这个效果不生效。";
            if (type == null) return "武器模式：普通攻击与特殊攻击分别连接一个瞬发或蓄力节点。左键触发普通攻击，右键触发特殊攻击。两条分支冷却独立；蓄力期间忽略另一攻击键。旧技能可点击转换为双攻击，保留原参数与连线。\n\n旧版单技能设置：技能的基础设置：普通攻击／特殊攻击、伤害、冷却和最大施法距离。\n普通攻击用左键，特殊攻击用右键；空格测试当前技能。\n\n释放方式输入（可选）\n连接瞬发或蓄力；不接时默认瞬发。\n最大施法距离：鼠标在范围内时终点跟随鼠标，超出时停在边界。\n\n技能终点 输出\n提供技能终点位置。接 圆形范围 的 圆心 可移动圆心；接 拉拽 的 目的地 可把敌人拉到这里。";
            if (type == typeof(CircleAttack)) return "圆形攻击：武器模式下须将释放输入接瞬发或蓄力的释放输出，未接则不生效。命中圆内或碰到边缘的敌人。\n\n半径比例 输入（可选）\n接蓄力比例后，从最小半径增长到最大半径；该蓄力还须接技能基础的释放方式。未连接时使用固定半径，圆心位置规则不变。\n\n圆心 输入（可选）\n决定圆心。不接时使用 技能基础 的技能终点：范围内跟随鼠标，超出 最大施法距离 时停在边界。也可连接位置来源指定圆心。\n\n命中目标 输出\n把本次命中的敌人交给 效果 的 目标。";
            const string area = "\n\n释放 输入（武器模式必接）\n接瞬发或蓄力的释放输出，决定此范围属于哪个攻击分支；未接时不生效。\n\n起点 输入（可选）\n决定攻击起点。不接时使用玩家施法位置（含 施法偏移）。\n\n命中目标 输出\n把本次命中的敌人交给 效果 的 目标。";
            if (type == typeof(ProjectileAttack)) return "向鼠标方向发射投射物。默认击中第一个敌人后消失；可调速度、飞行距离和大小。\n\n命中响应输出\n连接穿透、连锁、返回的“投射物”输入改变飞行行为，可同时连接。先穿透，再连锁，最后返回。" + area;
            if (type == typeof(LineThrustAttack)) return "从起点朝鼠标方向展开长方形。\n长度比例未连接时使用固定／最长长度；连接蓄力比例后，从最短长度增长到最长长度。宽度保持不变。\n\n长度比例输入\n接蓄力的蓄力比例；该蓄力还须接到核心的释放方式。\n\n普通区域目标输出\n只包含未接触前端强化区的敌人。\n\n前端区域目标输出\n包含与本次长方形最前方区域相交的敌人。前端区域长度决定强化区深度。跨越分界的敌人只归前端。\n\n伤害结算\n默认基础伤害：自动造成核心伤害，倍率节点不参与。连接倍率节点：关闭自动伤害，由连接的倍率节点结算。" + area;
            if (type == typeof(DamageEffect)) return "造成核心基础伤害乘以倍率的直接伤害。100% 为基础伤害，150% 为 1.5 倍；护甲只结算一次，不影响流血。\n技能范围的伤害结算必须选择“连接倍率节点”。未接目标不生效；多个倍率节点会分别结算。" + targets;
            if (type == typeof(ConeAttack)) return "从起点朝鼠标方向展开扇形，命中范围内敌人。可调距离和张开角度。\n\n距离比例 输入（可选）\n接蓄力比例后，从最短距离增长到最远距离；该蓄力还须接技能基础的释放方式。未连接时使用固定距离，角度保持不变。" + area;
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
