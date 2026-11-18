## 完成情况

- ❌ 使用 Unity 自带编译器检查了运行时、编辑器和 EditMode 测试程序集，均无报错、无编译警告。场景生成三次、引用检查和 93 个测试尚未执行。
- ❌ 已实现扣血后沿命中方向弹开默认 0.5 格，并闪烁约 1 秒。待 Play 验证。
- ❌ 已实现无敌期间免伤、双向穿怪、玩家不推怪也不减速。待 Play 验证。
- ❌ 已实现无敌到期后可再次受伤，若仍与怪物重叠则继续穿怪，脱离后恢复碰撞。待 Play 验证。
- ❌ 已实现弹开时对墙、餐车墙和锅台的碰撞检测。待 Play 验证。
- ❌ 已实现再次受击时重新扣血、弹开和闪烁。待 Play 验证。
- ❌ 已在「玩家血量」组注册两个调参项，每次命中时读取当前值。待验证数值窗口和 Play 中调参。
- ❌ 普通状态沿用原有推怪减速和怪物接触逻辑。待回归验证。

## 改动文件

- `Assets/_MMF/Scripts/Data/PlayerHealthTuning.cs` — 新增弹开距离和无敌时长。
- `Assets/_MMF/Scripts/Player/PlayerHealth.cs` — 管理无敌计时，发出命中事件并提供 `StartInvincibility()`。
- `Assets/_MMF/Scripts/Player/PlayerHitReaction.cs` — 新增弹开、闪烁和穿怪逻辑。
- `Assets/_MMF/Scripts/Player/PlayerMovement.cs` — 弹开期间暂时让出移动控制。
- `Assets/_MMF/Scripts/Combat/PlayerShove.cs` — 穿怪期间跳过推怪和减速。
- `Assets/_MMF/Scripts/Combat/Monster.cs` — 怪物移动查询跳过已忽略的碰撞对。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 生成时挂载受击反应组件并填写引用，沿用已有的血量数值资产。

## 疑问（需要确认，没有就写“无”）

- 无。

## 决定 / 假设（仅限技术层面）

- `PlayerHealth` 按游戏时间管理免伤。`PlayerHitReaction` 用 `Physics2D.IgnoreCollision` 按碰撞体配对穿怪，并独立检查重叠来恢复碰撞。
- 弹开通过物理步进施加速度，每步先做形状扫掠。弹开结束后，原有移动控制立即接回。

## 用户需要在 Unity 里做的事

1. 连续运行三次 `MMF → Build Prototype Scene`，检查玩家的受击反应组件引用，然后在 Test Runner 中运行 93 个测试。
2. 进入 Play，出车让怪物扑中。检查扣血、弹开、闪烁和穿怪。让闪烁在与怪物重叠时结束，走出来后再确认碰撞和推怪减速恢复。
3. 背靠墙、餐车墙和锅台分别测试弹开。在 `MMF → 数值` 修改弹开距离与无敌时长，确认下一次受击采用新值。

## 已知问题 / 未完成

- computer-use 窗口工具未获准操作 Unity。`Prototype.unity` 尚未由生成器更新，编辑器内验收仍待执行。