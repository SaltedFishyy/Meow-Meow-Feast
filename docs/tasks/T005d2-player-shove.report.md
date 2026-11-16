## 完成情况

- ❌ 编译通过（0 错误、0 警告）；场景生成 3 次、引用检查和原有 93 个测试尚未执行。
- ❌ 单怪推挤已实现，2 秒约 5 格及不重叠待试玩验证。
- ❌ 推挤复用怪物碰撞检测，墙边停止待验证。
- ❌ 支持同时推多只怪、减速不叠加，三面包围待验证。
- ❌ 无移动意图时不推怪，T005d1 行为待回归。
- ❌ 推挤不调用伤害或闪白，平底锅与击退待回归。
- ❌ 未顶怪时恢复倍率 1，原速度 5 待试玩验证。
- ❌ 未修改追逐和避让规则，车头／车尾与迎面错开待回归。

## 改动文件

- `Assets/_MMF/Scripts/Combat/PlayerShove.cs` — 新增无分配检测、推挤与减速。
- `Assets/_MMF/Scripts/Combat/PlayerShove.cs.meta` — Unity 自动生成。
- `Assets/_MMF/Scripts/Combat/Monster.cs` — 新增单帧推挤通道，复用碰撞限距。
- `Assets/_MMF/Scripts/Player/PlayerMovement.cs` — 新增速度倍率及推挤所需只读属性。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 添加组件并填写引用。

## 疑问（需要确认，没有就写"无"）

无。

## 决定 / 假设（仅限技术层面）

- `PlayerShove` 使用执行顺序 `-100`，先提交推挤与倍率；怪物和玩家在同次物理模拟前处理移动。
- 武器击退优先于推挤；保留原加减速逻辑。
- 保留原文件换行格式和 BOM。

## 用户需要在 Unity 里做的事

- 执行 `MMF → Build Prototype Scene` 三次，检查引用并运行 93 个测试。
- 按 Play，依照任务步骤验证推挤、墙边停止及原有行为。

## 已知问题 / 未完成

`Prototype.unity` 尚未由生成器更新，物理行为尚未实测。

自动审批拒绝了 computer-use 操作 Unity，原因是“未获准使用 Unity”，因此无法完成编辑器内的生成、测试和试玩验收。