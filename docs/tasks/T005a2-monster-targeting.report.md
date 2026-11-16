## 完成情况
- ❌ 编译检查通过，运行时、Editor、测试程序集均无错误或警告；场景生成 3 次、引用检查和 EditMode 测试运行未完成。
- ✅ 已实现车内玩家不追逐，怪物前往出生时抽定的左右端。
- ✅ 已实现距离 ≤8 开始追、>10 放弃，中间保持状态。
- ✅ 已实现玩家进车后停止追逐。
- ✅ 已实现主动移动碰撞后的切线滑动；实际绕角效果待试玩。
- ❌ 两个门的正常通行及无怪堵门尚未实测。
- ✅ 已新增 3 个可调参数；数据资源待生成器保存。

## 改动文件
- `Assets/_MMF/Scripts/Combat/MonsterTargeting.cs` — 新增目标选择、出生抽签和纯函数判断。
- `Assets/_MMF/Scripts/Combat/Monster.cs` — 删除 `_target`，接入目标逻辑并增加滑动。
- `Assets/_MMF/Scripts/Data/MonsterDefinition.cs` — 新增距离和概率参数。
- `Assets/_MMF/Scripts/Truck/TruckArea.cs` — 新增 `EndPoint(bool front)`。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 添加并连接目标组件。
- `Assets/_MMF/Tests/EditMode/MonsterTargetingTests.cs` — 新增 11 个测试用例。

## 疑问（需要确认，没有就写“无”）
无。

## 决定 / 假设（仅限技术层面）
- 每次启用怪物时重置追逐状态并重新抽签，适配现有重生方式。
- 目标中心按怪物半宽及碰撞间隙向墙外偏移，便于绕过车角。
- 击退保留原有碰撞截停方式。

## 用户需要在 Unity 里做的事
- 连续运行 `MMF → Build Prototype Scene` 3 次并检查引用。
- 运行全部 EditMode 测试。
- 按任务试玩步骤检查追逐、沿墙绕角和两个门的通行。

## 已知问题 / 未完成
- 未更新 `Prototype.unity` 和 `CursedDumpling.asset`。
- 未运行 Unity 测试及 Play 模式验证；本地程序集编译和 `git diff --check` 已通过。

自动审批拒绝了通过 computer-use 操作 Unity，工具未提供具体原因，因此未能完成编辑器内生成和验证。