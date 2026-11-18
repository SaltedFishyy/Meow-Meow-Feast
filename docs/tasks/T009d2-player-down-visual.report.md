## 完成情况

- ❌ 运行时、编辑器和 EditMode 测试程序集均用 Unity 自带的 C# 编译器编译通过，零错误、零警告。场景生成 3 次、场景引用检查和 93 项测试尚未在 Unity 内验证。
- ✅ 倒地时方块变灰、压扁并隐藏选中物品图标。头顶倒计时跟随玩家，默认显示 3、2、1，按实际剩余秒数向上取整。待试玩确认。
- ✅ 复活时恢复原来的 RGB、缩放和物品图标，并隐藏倒计时。透明度继续由原有无敌闪烁控制。待试玩确认。
- ✅ 缩放只作用于显示子物体，碰撞体尺寸和碰撞处理未改动。
- ✅ 倒计时协程复用现有计时，每帧只比较剩余秒数，数字变化时才生成并更新文本。

## 改动文件

- `Assets/_MMF/Scripts/Player/PlayerDownedView.cs` — 新增倒地外观、头顶倒计时和物品图标显隐。
- `Assets/_MMF/Scripts/Player/PlayerDowned.cs` — 新增只读属性 `RemainingSeconds`。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 生成玩家显示子物体、倒计时文字和所需引用。

## 疑问

无。

## 决定 / 假设

- 倒计时使用世界空间 `TextMesh` 白字。灰色、压扁比例、字号和头顶偏移可在显示组件中调整，默认宽度为原来的 1.2 倍，高度为 0.5 倍。
- 倒地期间通过停用图标对象隐藏选中物品，复活时恢复其原有激活状态。

## 用户需要在 Unity 里做的事

1. 等待 Unity 导入脚本，连续运行 3 次 **MMF → Build Prototype Scene**，检查玩家的 `PlayerDownedView` 引用均已赋值。
2. 在 Test Runner 运行全部 93 项 EditMode 测试。
3. 按 Play，出车让怪物把玩家打到 0。观察玩家弹开时是否变灰、躺平并显示 3、2、1，再确认复活时倒计时消失、图标和外观恢复，闪烁结束后透明度复原。

## 已知问题 / 未完成

- Computer Use 自动审批拒绝访问 Unity 编辑器窗口。`Prototype.unity` 尚未由生成器更新，场景引用、测试和试玩仍待在 Unity 内验证。