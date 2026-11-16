## 完成情况

- ❌ 编译检查通过，零错误、零警告；Unity 内生成场景 3 次及全量测试尚未验证。新增 6 个测试用例。
- ✅ 生成器已移除 4 个测试怪肉，保留原位置的 4 个蘑菇。
- ✅ 已实现三项独立掉落、半径 0.5 内散开，并复用现有自动拾取规则；待 Play 验证。
- ✅ 订阅保持到销毁，支持重生后再次掉落且不重复订阅；待 Play 验证。
- ✅ 已配置手臂、腿及对应煎制成品，成品颜色乘以 0.65。
- ✅ 几率从掉落表读取，重建场景保留已有几率调整。

## 改动文件

- `Assets/_MMF/Scripts/Data/DropTable.cs` — 新增掉落表和纯逻辑掷几率方法。
- `Assets/_MMF/Scripts/Combat/MonsterDrops.cs` — 新增死亡掉落组件。
- `Assets/_MMF/Tests/EditMode/DropTableTests.cs` — 新增 6 个测试用例。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 新增物品、掉落表及组件配置；删除测试怪肉生成逻辑和无用变量。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

- 使用 `Awake` 订阅、`OnDestroy` 退订，因为死亡事件在怪物停用后触发。
- 复用结果列表及随机数委托。

## 用户需要在 Unity 里做的事

- 运行 `MMF → Build Prototype Scene` 三次。
- 在 Test Runner 运行全部 EditMode 测试。
- 按任务试玩步骤验证拾取、重生掉落和煎制。

## 已知问题 / 未完成

新资源及场景尚未生成更新，Unity 测试及 Play 验收未执行。

自动审批拒绝了 Unity 窗口控制，提示“Computer Use was not approved to use Unity”，因此无法完成编辑器内验证。