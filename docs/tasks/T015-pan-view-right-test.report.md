## 完成情况

- ❌ 使用 Unity 自带的 C# 编译器编译了 Runtime、Editor 和 EditMode 测试程序集，均无错误或警告。Unity 内三次场景生成、引用检查和 84 个测试尚未执行。
- ❌ 已实现靠近或举起锅台时显示右侧全高窗口，走远自动收起。右侧宽度、WASD 移动和往返显示待试玩。
- ❌ 右侧窗口复用原有的拖放、翻面、进度环、「翻！」、快焦闪红和冒烟逻辑。完整煎制、成品入包及焦土取回待试玩。
- ❌ 已接入左键优先级，右侧窗口内的点击由锅界面或背包处理。窗口外举锅及举锅时的显示待试玩。
- ❌ 已实现 Play 中实时切换，关闭时回到头顶小锅。来回切换待试玩。
- ✅ 已在「煎锅」组添加「锅画面在右边（试验）」，默认开启。

## 改动文件

- `Assets/_MMF/Scripts/Data/CookingTuning.cs` — 新增试验开关。
- `Assets/_MMF/Scripts/UI/PanCookingView.cs` — 共用锅的显示与动画逻辑，按开关切换画面。
- `Assets/_MMF/Scripts/UI/PanDragInput.cs` — 根据当前画面处理命中和手势，按锅图宽度计算最短划动距离。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 生成右侧窗口并接好引用，将窗口放在背包栏和拖动图标下方。

## 疑问（需要确认，没有就写"无"）

无。

## 决定 / 假设（仅限技术层面）

- 两个 `PanCookingView` 共用同一个 `PanCooking`，由一个 `PanDragInput` 处理输入。切换画面时取消正在进行的拖动。
- 右侧窗口复用背包的 Overlay Canvas，命中检测传 `null`。背包格与窗口重叠时，按下鼠标优先命中上层背包格。
- 旧场景保留头顶小锅的兼容路径，重新生成后即可使用两种画面。

## 用户需要在 Unity 里做的事

1. 退出 Play，运行三次 `MMF → Build Prototype Scene`，检查 Console 和场景引用，再用 Test Runner 运行 84 个测试。
2. 进入 Play，捡蘑菇后走到锅台旁。确认右侧窗口随靠近和离开而显示、收起，并检查窗口内外的左键行为。
3. 把蘑菇拖进锅，等「翻！」后划过食物，两面各翻一次。另煎一份直到变成焦土，再拖回背包。
4. 保持 Play，在 `MMF → 数值 → 煎锅` 中来回切换「锅画面在右边（试验）」。

## 已知问题 / 未完成

- Computer Use 未获准操作 Unity 编辑器，`Prototype.unity` 尚未通过生成器更新。场景引用、测试结果和试玩行为仍需在 Unity 内确认。