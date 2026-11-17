## 完成情况

- ❌ 编译、生成 3 次、93 个测试：未在 Unity 中验证；`git diff --check` 通过。
- ❌ 资源唯一性及保留改值：已实现固定路径、存在即复用，尚未实测。
- ❌ 默认试玩行为：数值和原计算逻辑保持一致，尚未试玩。
- ❌ Play 中车外大小改为 9：已改为实时读取，尚未实测。
- ✅ 三个组件已删除迁移的数值字段；`_widthFraction` 保留。

## 改动文件

- `Assets/_MMF/Scripts/Data/CookingTuning.cs` — 新增煎锅数值及生效时机 Tooltip。
- `Assets/_MMF/Scripts/Data/CameraTuning.cs` — 新增镜头数值。
- `Assets/_MMF/Scripts/Cooking/FryingStation.cs` — 从数值文件读取四项参数。
- `Assets/_MMF/Scripts/UI/FryingPanel.cs` — 从数值文件读取翻面时长。
- `Assets/_MMF/Scripts/Core/FollowCamera.cs` — 从数值文件读取四项参数。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 添加资源加载、创建和引用填写。
- `Assets/_MMF/Tests/EditMode/FryingSequenceTests.cs` — 创建、注入并销毁测试数值对象。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

- 沿用现有 tuning 的私有序列化字段、只读属性和资源复用方式。
- 未添加数值窗口或标记属性。

## 用户需要在 Unity 里做的事

- 执行 `MMF/Build Prototype Scene`，生成两个数值资源并更新场景。
- 完成三次生成、改值保留、93 个测试及所列试玩检查。

## 已知问题 / 未完成

两个 `.asset` 和 `Prototype.unity` 尚未生成更新，生成前请勿直接试玩。

自动审批拒绝了 Unity 操作，返回原因是“Computer Use was not approved to use Unity”，因此未能执行上述 Unity 验证。