## 0.3.2-2d.1 — 独立 Playground 预览

- 可停靠的预览窗口，主游戏 Play Mode 中保持独立运行。
- 私有预览场景、手动时钟、目标列表、双攻击与蓄力输入；失焦取消与资源释放。
- 原技能资产共享读取，XY 范围、分区、状态、镜头震动、后仰与隔离的视觉特效预览。
- 音效仅显示触发，保留原场景测试入口进行完整音效／脚本试玩。
- 增加场景隔离、输入取消、运行中共存与离屏渲染测试。

## 0.3.1-2d.1 — MMF 主项目本地 XY 适配

- 改用 XY 技能几何与位移，保留资产类型和 GUID。
- 2D 正交测试场景、鼠标投影、WASD、范围预览、CircleCollider2D 木桩与屏幕内后仰。
- 施法偏移提供 XY 编辑；旧场景和游戏脚本不修改。
- 新增 XY 几何／组合／资产兼容测试与场景运行测试。

## Unreleased — 画板交互

- 按住右键划线时显示轨迹、预览待删除连接，松手统一断开；Esc 或失去捕获取消。一次手势支持撤销多条断连，普通右键菜单保留。
- 模块库分类可展开／收起，当前会话记住状态；搜索自动展开，清空后恢复。

## Unreleased — 后仰反馈

- 命中反馈分类新增“后仰”，配置角度、后仰时间和回正时间，复用目标连接与触发间隔。
- Playground 使用独立模型支点朝远离施法者方向后仰，连续命中平滑重启，结束、重置或禁用恢复姿态；碰撞体留在目标根节点。
- 新建反馈示例包含后仰，已有示例与用户技能保留不变。主项目安装包未更新。

## Unreleased — 分类名称统一

- 攻击基础 → 技能基础；攻击范围 → 技能范围；攻击效果 → 技能效果；受击反馈 → 命中反馈。
- 同步画板、模块库、Inspector、节点帮助、验证提示与示例菜单；保留内部类型、资产和连接标识。

## Unreleased — 受击反馈

- 新增受击反馈分类及镜头震动、命中特效、命中音效节点，复用目标连接和撤销保存。
- 共用运行时提供反馈请求、释放快照、首次命中计数和跨释放触发间隔；空资源静默跳过播放。
- Playground 提供播放、限量实例、自动清理、重置和镜头位置恢复，新增独立示例菜单。
- 只修改工具源码，主项目仍使用已发布的 0.2.1。

## 一体攻击节点
- 释放方式和参数合并到攻击范围，支持瞬发／蓄力切换及隐藏参数保留。
- 普通／特殊攻击直接连接完整攻击节点；旧资产支持显式可撤销迁移。
- 分类和根节点改名为“攻击基础”。

## Unreleased — 武器双攻击

- 攻击基础节点提供普通／特殊两个输入槽输出，释放节点拥有独立伤害、冷却与距离；范围必须连接释放。
- Playground 左右键触发各自分支，蓄力互斥、独立冷却、分支预览与 HUD。
- 保留旧技能模式，新增可撤销转换入口及不覆盖已有资产的双攻击示例。
- 仅修改 Ability Studio，未打包或更新主项目。

## Unreleased — 通用蓄力范围

- 蓄力比例可连接圆形半径、扇形距离；每个范围独立配置最小／最大值，预览与实际结算共用计算。
- 未连线保留固定范围；圆心、扇形角度与现有技能默认行为不变。
- 界面“技能核心”更名“基础攻击”，“技能效果”更名“攻击效果”。

## Unreleased — 蓄力与分区伤害

- 基础攻击新增普通／特殊攻击，模块库新增瞬发、蓄力与伤害节点。
- 蓄力可驱动长方形长度；普通区域与前端区域独立路由伤害和状态效果。
- 保留旧技能的瞬发、固定范围及默认基础伤害；新增独立示例和输入、场景回归测试。
- 仅 Ability Studio 开发，未更新主项目或发布包。

# Changelog

## 命中响应与中文界面

- 新增穿透、连锁、返回模块和投射物专用响应接口，未连接时不生效。
- 投射物支持连续碰撞检测、逐程命中去重、参数快照、组合响应和回程。
- 模块库、节点、接口、参数、提示、检查器、菜单与 Playground HUD 中文化。
- 新增独立命中响应示例和运行时、编辑器、场景回归测试。

## Circle default cast distance

- Unconnected Circle.Center now defaults to the mouse-following Core endpoint clamped by Distance, matching gameplay hits and Playground previews. Radius and explicit position bindings remain unchanged.
- Added unbound near/boundary/far/zero-aim hit tests and bound/unbound scene preview coverage.

## 0.1.1 — Initialize Playground on first open

- Open Playground and Play in Playground create missing Playground assets in the host project before opening the scene.
- Existing scenes, skill edits and materials are preserved; automatic setup does not create the unrelated Gameplay scene or change Build Settings.
- Cancelling the scene-save prompt or requesting setup during Play Mode does not start scene generation.

## Unreleased — Required Effect Targets and node help

- Effects now execute only from explicit Targets bindings; parked nodes and orphaned completion chains stay inactive. Existing assets are not auto-wired.
- Added pointer-adjacent Chinese help cards on colored node headers, explaining purpose, parameters and every port.
- Playground composition summary excludes inactive effects; newly generated Grappling Hook examples include an explicit hit-to-Pull binding.
- Updated effect test fixtures to author explicit routes and added parked/connect/disconnect and help coverage.

## Explicit Board connections

- Removed effect timing instruction labels from node cards.
- Removed automatic membership wires and ports; the Board now draws only authored bindings, including after adding modules.
- Existing manual connections and unbound runtime defaults are preserved.

## Maximum cast distance

- Core Distance now limits the mouse-following endpoint; bound Circle previews and casts share the clamped position.
- Grouped the module library into Skill Core, Skill Area and Skill Effect; retired Self and Mouse Position authoring entries while preserving existing assets.
- Added near/boundary/far/zero-distance, library filtering and Playground preview regression coverage.

## Core Distance

- Merged Skill Endpoint into Skill Core as Distance and a reusable endpoint position output.
- Added Undo-aware migration preserving legacy distance/connections, shared runtime/preview resolution and migration/regression coverage.

## Dot grid background

- Board now draws subtle round dots at the same 20-unit intersections as snapping, following canvas pan/zoom without intercepting input.

## Effect completion and grid layout

- Pull completion targets now accept every supported effect, including chained Pulls, with independent cast snapshots and cycle prevention.
- Completion fan-out applies effects without repeating base damage; replacement/reset cancellation remains intact.
- Board placement and movement snap to a 20-unit grid with Undo and persistence. Existing layouts are preserved until moved.
- Added completion fan-out, chained destination/timing, cycle rejection, grid Undo/persistence tests and scene Bleed/Slow completion coverage.

## Position and target bindings

- Added draggable Self, Mouse Position and Skill Endpoint sources, reusable area position inputs, per-effect target routing and Pull destination input.
- Added typed editable Board connections with persistence, deletion and Undo/Redo; unbound inputs retain legacy behavior.
- Pull completion targets can feed Stun; overridden/reset/dead targets cannot trigger the old pending completion. Explicit positions and parameters are cast snapshots.
- Playground previews bound area locations and selected saved skills; a separate Endpoint Gather sample demonstrates Circle → Pull → Stun.
- Added deterministic binding/serialization/Undo/runtime tests and a scene preview/gather/completion test.

## Cone attack

- Added Cone module with independent range/full-angle fields, validation, cast snapshots and shared runtime damage/effects.
- Added circle-sector geometry for narrow/wide/full-circle attacks, including arc, radial edges and apex; preview uses the same arc geometry.
- Playground displays mouse-oriented per-instance blue previews and orange cast outlines, with reset/removal cleanup.
- Added geometry boundaries, rotated origin, friendly filtering, repeated cones, invalid aim/parameters, authoring/serialization/Undo and multi-target scene tests.

## Rectangle attack

- Added draggable Rectangle (LineThrustAttack) with independent length/width fields, validation and cast snapshots.
- Added shared oriented rectangle geometry for precise target-circle intersections and preview corners; attacks extend from cast origin toward mouse aim.
- Shared runtime applies damage/effects to every intersecting enemy and publishes activation feedback. Playground shows per-instance blue previews/orange flashes.
- Added edge/corner, rotation/offset, friendly filtering, duplicate instance, zero-aim, invalid dimension, authoring/Undo and scene preview/hit/reset tests.

## Slow

- Added Slow module with independent reduction-percent/duration controls, validation, cast snapshots and ISlowReceiver capability.
- CombatTarget supports refreshing/expiring slow and status-aware voluntary movement while preserving forced displacement.
- Added optional Move Dummies Playground toggle and SLOWED feedback for visible speed comparisons; Reset disables patrol.
- Added movement, refresh/expiry/reset, stun/forced displacement, snapshots, duplicate fields/Undo, validation and AoE patrol scene tests.

## Bleed

- Added Bleed library/node module with independent damage-per-second and duration fields, validation and cast snapshots.
- Added optional IBleedReceiver and shared BleedEffectHandler; CombatTarget ticks armor-adjusted damage each second and settles fractional tails.
- Refresh preserves the next tick and accrued damage while replacing rate/duration; reset and death clear pending damage.
- Playground shows BLEEDING countdowns and reuses floating damage feedback. Added deterministic timing, refresh, armor, reset, snapshot, authoring and multi-target scene tests.

## Stun

- Added draggable Stun module and independent duration field with Undo, shared runtime snapshots, status handler and IStatusReceiver capability.
- CombatTarget tracks refreshable stun timers, expiration and reset without blocking forced displacement. SkillExecutor rejects stunned caster adapters without spending cooldown.
- Fixed policy orders Stun before Knockback and Pull; Playground displays yellow STUNNED countdowns.
- Added status/combination/UI regression tests; existing Grappling Hook smoke tests now use temporary compositions so user edits to the shared skill do not break them.

## Knockback

- Added draggable Knockback module with independent distance editing in Board and Inspector.
- Shared runtime snapshots distance and displaces targets away from the caster on XZ over 0.3 seconds; fixed policy keeps Pull last.
- Playground composition feedback includes Knockback and uses a generic moving status for displacement.
- Added direction, coincident fallback, cast snapshot, validation, Undo/independent instance and AoE scene displacement/reset coverage.

## AoE and independent module instances

- Added circular AoE damage/effects, radius editing, aim previews and activation rings.
- Allow repeated attacks and effects with independent parameters, identity, layout, deletion and Undo; all attacks share one cast cooldown.
- Preserve legacy primary attack serialization; extra attacks use an additive serialized list. Later Pull displacement replaces the active displacement.
- Added area targeting, mixed attacks, invalid radius, independent editing/persistence and Playground regression tests.

## Module library and composition editing

- Simplified library cards to module names, removed category subtitles, constrained search width, and unified sidebar spacing. Cards remain enabled and draggable; duplicate validation stays at the drop/command boundary.

- Library cards now support left-button drag and drop onto the board; AddModule accepts an optional canvas position recorded in the same Undo operation. Drops reject duplicates and mismatched source assets.

- Added metadata-driven ModuleCatalog and searchable ModuleLibraryView for the implemented Projectile/Pull modules.
- Added AddModule/RemoveModules commands, SkillDefinition.RemoveModule and IBoardView.DeleteSelectedModules; removal restores identity, parameters and layout through Undo/Redo.
- Added Delete-key and toolbar deletion, permanent Skill Core protection, duplicate prevention and missing-attack guidance.
- Inspector composition refreshes after edits; delayed parameter callbacks cannot edit a replacement module using a stale field.
- Added catalog, add/remove, persistence and assembled-runtime coverage. Playground HUD now reflects removed/restored modules and invalid compositions, with a PlayMode UI regression test.

## Grappling Hook composition board

- Added Skill Board with editable Skill Core, Projectile and Pull nodes, derived composition links, zoom/pan, saved node positions and a Play in Playground action.
- Added framework-independent IBoardView; GraphView references are confined to AbilityStudio.Editor.Board.GraphView.
- Added shared SkillParameterFields and SetParameter/MoveModule authoring commands with Undo/Redo, used by the board and Inspector.
- Added optional ModuleLayout presentation data and SkillDefinition.GetBoardPosition/SetBoardPosition; runtime casting remains unchanged.
- Added board/parameter/layout tests and extended persistence and next-cast coverage.

## 0.1.0 — Playable prototype

- Added Model, Runtime, Playground and Editor assemblies in an embedded UPM package.
- Added versioned SkillDefinition composition, stable module identity, Projectile and Pull data, and validation that blocks invalid casts.
- Added deterministic shared SkillExecutor with cast-time snapshots, cooldowns, swept XZ targeting, armor-adjusted damage and Pull handlers.
- Added game-facing caster/target/capability interfaces, effect ordering policy and manual ticking.
- Added CombatTarget health/displacement and optional runtime host.
- Added WASD/mouse playground, UI Toolkit HUD, dummy controls, feedback, Undo-aware Inspector and asset saving.
- Added host-project sample scene generation and tests. Pull includes a serialized duration (default 0.45 seconds) for editable motion feedback.











