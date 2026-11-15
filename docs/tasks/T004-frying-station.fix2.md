# T004 补丁 2 — 测试失败 + 画面透明

> 给 Codex：先读 `AGENTS.md`，再读 `docs/tasks/T004-frying-station.fix1.md` 和 `docs/tasks/T004-frying-station.fix1.report.md`。补丁 1 的代码已在工作区（未提交）。Claude 已在 Unity 里实测：贴着锅台可以操作、能放下、能贴墙、车外放不下、画面不盖背包栏、中文正常、锅台开局在 (-5, 2.5)——**这些都通过了，不要改它们的行为**。只修下面两点。

## 1. 7 个 EditMode 测试失败（必须修）
Unity Test Runner 结果：27 个测试里 7 个失败——
- `StationRangeUsesFootprintEvenWithDisabledCollider` 的 4 个用例
- `PlacementAllowsPlayerContactButRejectsPlayerOrWallOverlap` 的 3 个用例

原因：测试里用 `go.SendMessage("Awake")` 调用 `Awake`。在 EditMode 下这样调用**不会执行** MonoBehaviour 的 `Awake`，Console 会报 `Assertion failed on expression: 'ShouldRunBehaviour()'`（共 13 次）。结果 `_collider` 等缓存字段一直是 null。

修法（技术细节你定，下面是方向）：
- 测试里不要用 `SendMessage("Awake")`。
- 可以让组件在第一次用到时再取缓存。例如在私有属性里写 `if (_collider == null) _collider = GetComponent<BoxCollider2D>();`。这样 EditMode 测试不依赖 `Awake`，运行时行为也不变。`PlayerInteractor`、`PlayerMovement` 等测试里用到的组件也一样处理。
- 也可以把纯计算抽成静态方法或纯 C#，在测试里直接测这些逻辑，不用经过 MonoBehaviour。
- 修好后 **27 个（或更多）测试全部通过，Console 没有 `ShouldRunBehaviour` 断言**。

## 2. 煎的画面背景不是完全不透明
- 现象：画面背景色的 alpha 是 0.98，能隐约看到后面的世界（车墙的线、远处的测试物品）。上次说的"小深色方块"就是透过画面看到的一个测试物品。
- 修法：画面背景改成**完全不透明**（alpha = 1）。

## 验收
- [ ] 编译无报错无警告
- [ ] EditMode 测试全部通过，Console 没有断言错误
- [ ] 画面背景完全不透明，看不到后面的世界
- [ ] 不改任何玩法行为和数值
