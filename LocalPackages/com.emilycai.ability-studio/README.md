# Meow Meow Feast · XY 2D 本地工具版（0.3.2-2d.1）

本目录是主项目可直接修改的工具源码，基于 0.3.0。主项目包引用指向本目录；不要修改 Library/PackageCache。独立 Ability-Studio 仓库仍保持原版。

- 所有技能方向、范围、连锁／返回、拉拽／击退使用 XY；Z 仅用于显示深度。朝向约定为局部 +X，围绕 Z 旋转。
- 保留 SkillDefinition 类型、序列化字段和原 .meta GUID，现有 Tongs 可直接读取；加载不会迁移或覆盖技能。旧 castOriginOffset.z 保留在数据中但不参与施法，面板只编辑 XY；命中特效的 Z 偏移仍可用于显示层次。
- 画板“进入测试场景”自动创建／打开 Assets/_Sandbox/Playground2D/AbilityPlayground2D.unity；正交相机面向 XY，WASD 在 XY 移动，木桩使用 CircleCollider2D，预览使用平面图形与无光照材质，适配主项目 URP 2D Renderer。
- 旧 Playground 场景及主项目已有场景不会被覆盖。不要使用旧 XZ 场景验证此 XY 版本。
- 测试场仍用显式目标注册和圆形包围范围计算命中，不代表已接入游戏 Monster 的 Physics2D 查询、障碍、背包或 PlayerAttack。那些游戏脚本和数值面板未修改。
- 蓄力、普通／特殊攻击、状态效果、命中反馈沿用原资产；后仰改为视觉支点在屏幕内倾斜再回正，不旋转目标碰撞体。

## 独立 Playground 预览窗口

- 菜单 `Tools → 技能工坊 → 独立 Playground 预览`，或技能画板顶部“独立 Playground 预览”。可以拖到 Game 面板旁边，同时保留两边可见。
- 无需切换场景或进入 Play Mode；主游戏已经运行或暂停时仍可独立测试。启动／退出主游戏时预览暂时释放资源，再自动重建；技能选择保留，临时战斗状态重置。
- 点击预览获得焦点，WASD 移动；左键普通攻击、右键特殊攻击，按住蓄力、松开释放；空格使用当前预览分支。单攻击资产使用它配置的攻击按键。Esc、失焦、隐藏面板或暂停预览会取消蓄力并清除按键。
- 预览使用 UI Toolkit 窗口事件，不向游戏 Input System 注入事件，也不修改主游戏的时钟、相机、输入设置或目标列表。主项目默认的键鼠 Game View 焦点规则用于将输入交给当前窗口。
- 数据直接读取原 SkillDefinition，画板修改从下一次释放生效；预览的血量、位置、冷却、蓄力和反馈均为临时状态，不写回资产。
- 支持普通／特殊攻击、蓄力范围与前端分区、状态倒计时、移动／添加木桩、暂停和重置。镜头震动和后仰仅影响预览对象。
- 命中特效在私有预览场景中展示渲染与粒子，移除脚本、动画控制器、音频、相机和物理组件，防止预制体运行游戏逻辑；脚本／动画驱动的特效需使用原场景测试。音效只显示触发提示，不播放到主游戏音频。
- 原“进入测试场景”入口保留，适合验证完整场景中的输入、音效或脚本效果。

以下为工具原有说明与历史开发记录；涉及坐标和平面时以上述 2D 约定为准。

# 画板操作

- 模块库分类标题可点击展开／收起，折叠状态在当前 Unity 会话内保留。搜索时展开匹配分类，清空搜索后恢复原来的折叠状态。
- 从画布空白处或连线上按住右键拖动，会显示白色细线轨迹，划到的连接变淡作为待删除预览；松开右键才统一断开。一次手势可划断多条线，Ctrl+Z 一次撤销；Esc、失去鼠标捕获或关闭画板会取消，不删除节点或连接。右键单击仍打开菜单，从节点或参数控件内开始的右键操作保留原行为。连接修改仍需按现有工作流保存技能。
- 中键平移时点阵背景跟随节点移动。

# 命中反馈

模块库新增 **命中反馈** 分类：镜头震动、命中特效、命中音效、后仰。将攻击的“命中目标”（也支持长方形分区目标、拉拽到达目标）接到反馈节点的“目标”。未连接时不触发；反馈不额外造成伤害。

- 镜头震动：强度（米）、持续时间、频率。默认每次攻击首次命中触发，避免范围攻击打中多个敌人连续震动。
- 命中特效：视觉预制体、大小倍率、存活时间、世界偏移。瞬时区域使用目标位置，投射物使用碰撞位置。未指定预制体时不生成特效。
- 命中音效：音效资源、音量、音调。未指定资源时不播放；Playground 使用 2D 声音。
- 后仰：角度（0–60 度）、后仰时间、回正时间。默认 15 度、0.06 秒后仰、0.18 秒回正，按每次命中触发。Playground 模型朝远离施法者的方向倾斜；重合时使用施法者朝向。连续命中从当前姿态重新开始，不叠加角度；结束、重置或禁用时恢复原姿态。只旋转独立的模型支点，目标根节点和碰撞体不移动，不施加眩晕。游戏侧收到 `RecoilFeedback` 后需接自己的模型或动画系统。
- 每个节点可选择“每次命中”或“每次攻击首次命中”。最短触发间隔按施法者、技能资产和节点区分，跨多次释放限频，0 表示不限频；被限频的请求直接跳过，不排队。重置清除计数、间隔、临时特效和音效，并恢复镜头位置。
- 投射物使用释放时的反馈参数与资源快照，穿透、连锁、返回共享该次释放的首次命中计数。命中导致目标死亡时仍有反馈；未命中不触发。

通过 **Tools → 技能工坊 → 创建命中反馈示例** 生成独立示例，再进入测试场景。新建示例包含后仰、自带简单金色命中球和合成短音；再次打开保留已有资产，已有示例需手动拖入后仰并连接目标。可替换为自己的视觉预制体和音效。请勿在反馈预制体里放战斗逻辑。

运行时通过 `SkillExecutor.FeedbackRequested` 发出 `HitFeedbackRequest`（参数快照、施法者、目标、命中位置）；游戏侧订阅后接自己的镜头、特效和音频系统，并按施法者选择本地玩家镜头。`ResetPerformed` 通知清理。`PlaygroundHitFeedback` 是独立预览适配器，不修改主项目镜头。当前不包含顿帧或受击闪色；本次只更新工具源码，未重新打包主项目。

---

# 一体攻击节点

模块库分类与核心节点统一命名为“技能基础”。通过 **Tools → 技能工坊 → 创建一体攻击示例** 打开独立示例，进入测试场景后左键普通攻击、右键特殊攻击。

- 技能基础的普通／特殊攻击直接连接技能范围的“攻击按键”，每个按键对应一个完整攻击节点。
- 在范围节点内选择“瞬发”或“蓄力”，分别显示固定范围或起始／蓄满范围及蓄满时间。切换保留隐藏参数和效果连接。
- 每个攻击独立设置基础伤害、冷却和最大施法距离。圆形、扇形、长方形支持蓄力增长；投射物支持蓄力释放，弹道参数保持固定。
- “命中目标”连接倍率、眩晕等效果；长方形继续支持普通区域和前端区域。
- 旧资产不会自动转换。点击“合并释放方式到攻击节点”可安全转换并撤销。若旧分支有多个范围或不兼容的跨分支蓄力连接，会保留旧结构。
- 独立示例优先复制现有 WeaponAttacks，原技能和参数不会被覆盖。

以下保留旧版工作流说明。

---

范围节点的蓄力最短长度、最小半径、最短距离仅在比例输入接入蓄力节点时可编辑。未连接时灰显并加删除线，断开保留数值。固定／最大范围、宽度、角度和前端区域仍可编辑。直接伤害效果节点现名为“倍率”，计算逻辑不变。

# 武器普通攻击与特殊攻击

菜单 **Tools → 技能工坊 → 创建武器双攻击示例** 打开独立武器画板。点击“进入测试场景”，左键瞬发扇形，右键按住蓄力长方形、松手重击。Esc 取消蓄力。两条分支冷却独立，任一分支蓄力期间另一攻击键被忽略；不会排队补放。

连接结构：
- 技能基础.普通攻击 → 瞬发.攻击入口 → 瞬发.释放 → 技能范围.释放。
- 技能基础.特殊攻击 → 蓄力.攻击入口 → 蓄力.释放 → 技能范围.释放。
- 蓄力比例仍可接长度／半径／距离比例；建议与该范围的释放来源使用同一个蓄力节点。
- 释放节点的“技能终点”接圆心或拉拽目的地。圆形未接圆心时，默认使用所属分支的终点。
- 每个瞬发／蓄力节点单独设置基础伤害、冷却、最大施法距离；武器入口保留名称与公共施法偏移。倍率节点的百分比以所属释放分支的基础伤害为基准。
- 每个武器输出只接一个释放节点，每个释放可接多个范围。没有接释放的范围、没有接攻击入口的释放都不参与攻击。不会自动连线。

旧技能仍按原单技能模式运行。点击旧“技能基础”节点上的“转换为双攻击（保留当前攻击）”，原参数和现有范围迁移到原普通／特殊攻击分支，缺少释放节点时补一个瞬发；保留效果、终点与蓄力连线，支持撤销。现有技能资产不会自动迁移或被示例覆盖。

Playground 默认预览普通攻击（仅配置特殊攻击时预览特殊攻击）；按下某攻击键后显示该分支，HUD 显示两边冷却。空格测试当前预览分支。运行时同一武器与施法者应使用一个 SkillReleaseSession，通过 Press 的可选 SkillKind 参数选择分支；TryCast 是底层立即结算接口，不负责输入互斥。

---

# 圆形与扇形复用蓄力

核心节点统一显示为“技能基础”，效果分类显示为“技能效果”；内部类型和现有资产标识保持不变。

1. 把蓄力的“释放方式”接到技能基础的“释放方式”。
2. 把同一个蓄力的“蓄力比例”接圆形的“半径比例”、扇形的“距离比例”或长方形的“长度比例”，可同时连接多个范围。
3. 圆形设置“蓄力最小半径”和“固定／最大半径”；扇形设置“蓄力最短距离”和“固定／最远距离”。按住时线性增长，松手按当前蓄力范围判定。
4. 圆心遵循原有位置和最大施法距离规则；扇形角度不变。未连蓄力时仍使用原固定范围；连接了未被技能基础选中的蓄力节点时，使用最小范围。

---

# 蓄力长方形与分区伤害

通过 **Tools → 技能工坊 → 创建蓄力长方形示例** 打开独立示例，再点击画板的“进入测试场景”。示例不覆盖已有技能。

- 核心选择普通攻击（左键）或特殊攻击（右键）；空格也可测试当前技能。
- “瞬发／蓄力”的释放方式输出接核心的释放方式。不接仍为瞬发，旧技能仍为普通攻击。
- 蓄力的蓄力比例接长方形的长度比例。按住增长到最长长度，松手释放；满蓄不自动释放。Esc、重置、切换技能或离开游戏输入焦点取消蓄力，不消耗冷却。
- 长方形的“普通区域目标”与“前端区域目标”互斥，跨分界的敌人归前端。前端深度从实际长度的末端向后量；金色预览显示前端区域。“命中目标”仍输出全部命中敌人。
- 长方形伤害结算选“连接倍率节点”，普通区域接 100% 伤害，前端区域接 150% 伤害与眩晕。倍率乘核心基础伤害。旧技能保留“默认基础伤害”，此模式忽略直接倍率节点，已有状态效果保持原行为。
- 示例：特殊攻击，蓄力 2 秒，长 4–14 米、宽 12 米、前端 3 米；普通区域伤害 2，前端伤害 3 并眩晕 2 秒。面向初始木桩蓄满释放可对照效果。未接目标的倍率节点不生效。

游戏侧输入适配器可用 SkillReleaseSession.Press/Tick/Release/Cancel。TryCast 保留为底层立即结算接口（可传归一化蓄力值），用于已有游戏适配器与测试。一个技能资产仍只有一个核心，本次没有实现同时装备两个技能槽。

---

# 技能工坊：命中响应与中文界面

模块库现在分为“技能基础”“释放方式”“技能范围”“技能效果”“命中响应”。菜单入口：**Tools → 技能工坊 → 技能画板**。测试仍使用 Playground Scene；点击“进入测试场景”。

- **穿透**：投射物命中后继续直线飞行。穿透次数表示额外允许穿过的敌人数；2 次最多命中 3 个敌人。
- **连锁**：穿透次数用完后，寻找命中点搜索距离内最近的未命中敌人，继续飞向它。次数表示额外跳转次数，每段飞行距离不超过搜索距离。目标移动时仍可躲开投射物。
- **返回**：去程结束后飞回施法者当前的位置。返回速度倍率乘以投射物原速度，回程可再次命中敌人；同一敌人每程最多一次。回程不会再次连锁或返回。追赶预算为初始射程和返程开始时到施法者距离的较大值的两倍，避免无限追赶。

连接 **投射物.命中响应 → 穿透/连锁/返回.投射物**。这些节点只改变所连接投射物；没有接线时不生效。投射物的“命中目标”仍然接技能效果的“目标”，每次穿透、连锁或返回命中都会触发所连接的效果。

同一投射物可同时连接多个响应，规则固定为 **先穿透 → 再连锁 → 最后返回**。多个穿透的次数相加；多个连锁的次数相加、搜索距离取最大；多个返回取最大速度倍率。各节点参数独立保存，已发射投射物使用施法时的参数快照，后续修改在下一次施法生效。支持撤销、重做和删除连线。

通过 **Tools → 技能工坊 → 创建命中响应示例** 打开独立示例。未连接响应的旧技能仍在首次命中后消失。中文界面覆盖画板、模块库、节点、接口、参数、提示、检查器和测试场景 HUD；既有自定义技能名称、资产路径和内部标识不改写。测试场景从系统中文字体创建临时字体（Windows 优先微软雅黑），不分发系统字体。

当前只在 Ability Studio 开发，未打包或更新到主项目 Meow Meow Feast。

---

# Ability Studio — playable prototype

A designer-oriented skill assembly tool for Unity 6000.6.4f1. This first vertical slice implements reusable Projectile, circular AoE, Rectangle and Cone attacks, base damage, Pull, Knockback, Stun, Bleed and Slow effects. Skills are compositions stored in `SkillDefinition` assets, not execution graphs.

## Play in the host project

1. Open `Assets/_Sandbox/Playground/AbilityPlayground.unity` (or **Tools → Ability Studio → Open Playground**).
2. Choose **Tools → Ability Studio → Skill Board**. Skill Core and the configured modules are shown from the shared asset. Click **Play in Playground**. Dock the board beside Game view to edit and test together.
3. WASD moves, the mouse aims, and left-click or Space casts. The default generated skill has a 6-second cooldown and deals 2 damage before pulling the target toward the caster.
4. **Add Dummy** adds a target. **Reset Arena** restores targets to their spawn points, restores HP, clears projectiles/cooldowns, and resets the player. Added dummies remain until Play Mode ends.
5. Edit cooldown, damage, projectile speed/range/radius, or pull stop distance/duration on a node or in the Inspector; press Enter to commit the field. The next cast reads the new settings. **Save Skill** persists them to disk. ScriptableObject edits can persist after leaving Play Mode; use Undo to revert experiments.
6. Open `Assets/_Sandbox/Gameplay/AbilityGameplay.unity` to test the same asset and executor in a separate scene.

First use: **Open Playground** and **Play in Playground** automatically create the missing Playground scene and sample resources in the host project. Existing scenes, skills and materials are preserved. The Board requires a saved skill; on a fresh install, use Open Playground first to create the sample skill.

No scene needs to be added to Build Settings to play in the Editor. **Create Missing Prototype Assets** creates only missing sample assets; it never overwrites an existing scene or skill.

## Package boundaries

- `Runtime/Model`: versioned composition data, module metadata and prototype validation. No editor or input dependency.
- `Runtime/Skills`: shared executor, swept projectile collision, cooldown, damage and smooth Pull; interfaces for game adapters and effect handlers.
- `Runtime/Playground`: optional Input System player, dummy controls and UI Toolkit feedback.
- `Editor/Core`: UI Toolkit skill Inspector, Undo-aware asset commands, sample scene creation.
- `Tests/Editor`: deterministic runtime, asset persistence, Undo and dependency tests.
- `Tests/Runtime`: PlayMode scene smoke tests.

Games can use Model and Runtime without the Playground. Supply `ISkillCaster`, `ISkillTarget`, optional `IDamageable`/`IDisplaceable`, an `ITargetProvider`, and an `ISkillClock`. Tick `SkillExecutor` directly or use `SkillRuntimeHost`.

## Prototype limits

Projectile, AoE, Rectangle, Cone, Pull, Knockback, Stun, Bleed and Slow execute today. Other attacks/effects and reusable Effect Groups remain future work. Subclassing the module bases, adding validation/snapshot support and implementing handlers extends this slice; unsupported modules fail validation rather than silently doing nothing. Effect order belongs to `IEffectOrderPolicy`, not the asset list.

The small arena uses an explicit target registry with circular XY hit bounds. It has no obstacle collision, navigation, controller collision or object pooling. Pull is a displacement toward the caster's position at impact. An airborne shot keeps its cast-time parameters; edits affect the next cast. A cooldown already running retains its original end time.

The editor Inspector is the configuration/saving surface; a built player has gameplay/HUD controls but cannot save Unity assets. The board edits the existing composition through shared Undo-aware commands. Drag headers to arrange nodes; Save Skill also persists positions. Only manually authored Position and Targets bindings are shown as connections. Dropping a module adds no automatic lines; unconnected Effect Targets are inactive, while position inputs retain their defaults. Hold the left mouse button on an AoE, Rectangle, Cone, Projectile, Knockback, Pull, Stun, Bleed or Slow card in the searchable Module Library, drag it onto the board, and release to add it at that position. A click or a drop outside the board adds nothing. Select module headers and press Delete or use Delete Selected to remove them; Skill Core cannot be deleted. The library groups modules under Skill Core, Skill Area and Skill Effect. The Skill Core card opens the existing Core settings; each skill has one Core. Area and effect cards stay enabled and draggable. Add multiple instances of any module: each has an independent ID, parameters and position. Connect an attack Hit Targets output to each desired Effect Targets input, or connect Pull.Completed Targets for effects after pulling. Parked effects with no Targets connection do not run. Every attack executes once per cast, sharing Skill Core damage and cooldown; a skill without an attack cannot cast. Multiple Pulls use the existing rule: the later displacement replaces the current one. Adding and removing modules supports Undo/Redo, including saved positions. The Inspector refreshes when the composition changes. Embedded preview remains deferred. Playground runs in a separate Game view and previews the saved skill selected on the Board without replacing scene asset references.





## Circular AoE

Drag Circle (AoE) from the library onto the board and edit AoE radius (m). In Playground, aim at the ground: blue circles preview each area, and an orange flash shows activation. All living enemy bounds intersecting the circle take base damage and effects once per AoE instance. Areas may overlap for repeated hits. AoE is instantaneous on the XY plane. Connect Skill Core.Skill Endpoint to Circle.Center to follow the mouse within Core Distance and clamp outside it. An unconnected Circle also defaults to this clamped Core endpoint; its radius is unchanged. There is no obstacle restriction. Delete Projectile/Pull if you want to test a pure area-damage skill. Save Skill persists the composition; Undo restores experiments.


## Knockback

Drag Knockback onto the board and edit Knockback distance (m), then press Enter. The next cast pushes every hit target away from the caster on XZ over 0.3 seconds (default distance 3 m). A target exactly at the caster uses caster facing. Distance must be finite and positive. Each repeated Knockback has independent parameters; later displacement replaces the current displacement. Fixed effect policy applies Knockback before Pull, so Pull overrides Knockback when both are present.

For an AoE test, remove Projectile and Pull, add AoE and Knockback, then Play in Playground. Aim over several dummies and cast; each moves away from the player. Reset Arena restores their positions. Use Undo to restore experiments or Save Skill to keep the assembly.


## Stun

Drag Stun onto the board and edit Stun duration (s), then press Enter. Default is 2 seconds; duration must be finite and positive. Combine AoE + Knockback + Stun: hit dummies display a yellow STUNNED countdown while knockback continues. Expiration removes the status; Reset Arena clears it immediately. Reapplying Stun refreshes to the latest applied duration, without adding durations; repeated Stun nodes keep independent parameters. Runtime policy applies Stun before Knockback and Pull.

Games implement IStatusReceiver on targets to receive stun, and on caster adapters to let SkillExecutor reject casts with CastResult.Stunned. CombatTarget implements the timer and refresh/reset behavior; game movement/AI controllers should read IsStunned to block voluntary actions. Playground dummies are stationary and have no attacks, so the preview shows status/countdown and displacement together. Stun does not block forced displacement or damage.


## Bleed

Drag Bleed onto the board. Edit Bleed damage / s and Bleed duration (s), then press Enter; defaults are 1 damage/s and 3 seconds. Both must be finite and positive. Combine with AoE to apply bleeding to every hit enemy. Playground displays a red BLEEDING countdown and floating damage each second; the final fraction of a second settles proportionally on expiration. Armor reduces tick damage using the existing formula. Reset Arena clears the status and any pending damage.

Reapplication replaces the rate and refreshes duration without stacking. It preserves tick progress and damage already accrued, so rapid hits do not postpone damage indefinitely. Repeated Bleed nodes have independent fields, but the last applied instance refreshes one target status. Projectile snapshots preserve cast-time values. Stun and forced displacement do not pause bleeding. Games implement IBleedReceiver for bleed support; CombatTarget provides the default implementation without requiring changes to existing stun-only IStatusReceiver adapters.


## Slow

Drag Slow onto the board; set Slow reduction (%) and Slow duration (s), then press Enter. Defaults: 50% slower for 3 seconds. Reduction must be greater than 0 and at most 100; duration must be positive; both must be finite. Reapplication replaces the percentage and refreshes duration without stacking. Repeated nodes remain independently editable. AoE and Projectile use the shared runtime and cast snapshots.

In Playground enable Move Dummies (off by default) to see targets patrol horizontally at 2 m/s. Cast AoE + Slow: affected targets show SLOWED with percentage/countdown and move slower, returning to normal on expiry. Stun blocks their voluntary movement; Knockback/Pull retains its distance and duration and takes priority over patrol. Reset Arena clears statuses, restores positions and turns patrol off. Remove Stun when testing Slow alone.

Games implement ISlowReceiver or use CombatTarget.MovementSpeedMultiplier in their own controller. CombatTarget.MoveVoluntarily applies slow/stun and respects forced displacement; it does not provide collision or navigation.


## Rectangle

Drag Rectangle from the library to the board and edit Length (m) and Width (m), then press Enter. Defaults are 8 m long and 3 m wide. This is the LineThrustAttack data type from the architecture, shown as Rectangle in the UI. The rectangle starts at the caster plus rotated cast offset, extends its full length toward the mouse on the ground plane, and is centered sideways across that direction. It activates instantly, hitting all intersecting living enemy circular bounds once per instance with Core damage and every configured effect.

Playground draws blue aim bounds and an orange activation flash using the same geometry. Mouse distance changes direction, not rectangle length. Aim away from the cast origin; a zero direction blocks the whole cast. Length and width must be finite and positive. Repeated rectangles have independent parameters and one shared skill cooldown. Reset clears activation visuals. No obstacle occlusion is applied.


## Cone

Drag Cone onto the board and edit Range (m) and Angle (degrees), then press Enter. Defaults: 6 m reach and a 90-degree total opening angle (45 degrees on each side). Range must be positive; angle must be greater than 0 through 360; both must be finite. The sector starts at the caster plus rotated cast offset and faces the mouse. Mouse distance changes orientation rather than reach. A zero aim direction blocks the cast.

Blue outlines preview each cone; orange outlines flash on activation. All living enemy circular bounds intersecting the sector receive Core damage and configured effects once per instance. Arc, radial edges and apex are included. At 360 degrees the shape is a circle centered at the cast origin. Repeated cones have independent parameters and share one skill cooldown. Reset removes activation visuals. Obstacles do not occlude the attack.


## Position sources, areas and effect bindings

Skill Core now owns Distance (m) and a Skill Endpoint position output. Distance is the maximum XZ cast distance from the cast origin (caster plus rotated cast offset). Inside this range, Skill Endpoint follows the mouse ground point; outside, it clamps to the boundary along the aim direction. Circle.Center and Pull.Destination connected to this output share the same point, and Circle preview uses the same calculation. A Circle at the cast origin is valid; directional attacks still require a nonzero aim direction. Self and Mouse Position are no longer offered in the library; existing serialized sources remain compatible. A green marker shows its location. Position outputs can feed Circle.Center, directional attack Origin, or Pull.Destination. Attack Hit Targets outputs feed effect Targets. Pull.Completed Targets can feed any Effect.Targets (Stun, Bleed, Slow, Knockback or another Pull) after each target finishes that pull. Fan-out is supported; chained Pulls preserve their own destinations and completion effects. Self-links and completion cycles are rejected.

For the gather skill, connect:
- Skill Core.Skill Endpoint → Circle.Center
- Skill Core.Skill Endpoint → Pull.Destination
- Circle.Hit Targets → Pull.Targets
- Pull.Completed Targets → Stun.Targets

Set Skill Core Distance, Circle radius, Pull duration and Destination gap (0 to reach the exact point), and Stun duration. Circle selects all living enemies whose bounds overlap it; there is no two-enemy limit. The hit set is captured at activation. Explicit positions and parameters are captured per cast. Targets already at the destination still complete after Pull duration. A replaced displacement cancels the old completion. Reset and death suppress pending stun.

Tools → Ability Studio → Create Endpoint Pull Example creates/opens a separate EndpointPull asset (never overwrites it), with the four connections ready. Play in Playground previews the selected skill. Edit parameters and press Enter for the next cast; Save Skill persists edits. Selecting an edge and Delete disconnects it; Undo/Redo restores connections. Deleting a source removes its incident connections.

Unconnected inputs preserve existing behavior: Circle centers at the mouse clamped to Core Distance; other attacks start at cast origin; effects with unconnected Targets do not run; Pull uses the caster location at impact. Explicit Self is sampled at cast time. Every effect requires attack hit targets or Pull completion targets. An orphaned Pull chain with no attack source is inactive. Disconnecting affects future casts; projectiles and pulls already in progress retain their cast snapshots. Existing assets are not auto-wired: previously unbound effects now need an explicit Targets connection. Effect policy still controls effects within each phase. This is typed composition with a completion target source, not a general execution graph.

Game displacement adapters must implement ITrackedDisplaceable to signal completion for deferred effects. Existing IDisplaceable adapters retain ordinary Pull but cannot emit completion. CombatTarget implements both through the derived interface.

Node placement and movement snap to a 20-unit canvas grid on release, independent of pan/zoom. Multi-selected nodes each snap to the nearest intersection. Save Skill persists snapped coordinates; Undo restores the prior layout. Existing saved layouts stay unchanged until moved.

The Board background draws subtle dots at the same 20-unit grid intersections used by snapping. Dots follow canvas pan/zoom; the background does not intercept selection or dragging.

Legacy endpoint nodes are merged into Core when opening the Board or Inspector: their distance and position connections are retained, with Undo support. Save Skill persists migration. If an old skill has several different endpoint distances, extra legacy sources are preserved to avoid changing their behavior; new endpoint nodes are no longer offered in the library. Distance does not replace Projectile range, Rectangle length, Cone range or Circle radius; it controls the Core endpoint output only.

Hover over a node's colored title bar for a Chinese help card beside the pointer. It explains the node, parameters and each input/output in designer terms, including required Targets and optional position defaults. The card stays at a readable size while zooming and does not intercept node interactions; leaving the title or dragging hides it.
