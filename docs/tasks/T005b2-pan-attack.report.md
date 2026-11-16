## 完成情况

- ✅ 已实现武器数据、物品武器引用和公开 `TryAttack` 方法。
- ✅ 已实现扇形内全部命中、每怪每次一次，复用现有扣血、闪白和击退。
- ✅ 已实现冷却、非武器拒绝攻击、车内挥击但不造成伤害。
- ✅ 生成器已加入武器资源生成、平底锅引用赋值及 Player 攻击组件。
- ❌ 完整验收尚未执行：本机 C# 编译检查三个程序集均无错误无警告；Unity 场景生成三次、原有 76 个测试及新增 11 个用例、Play 验收待执行。
- ✅ 未修改背包、锅台、怪物移动或输入代码；运行回归待验证。

## 改动文件

- `Assets/_MMF/Scripts/Data/WeaponDefinition.cs` — 新增四项武器参数。
- `Assets/_MMF/Scripts/Data/ItemDefinition.cs` — 增加武器引用及只读属性。
- `Assets/_MMF/Scripts/Combat/WeaponArc.cs` — 新增纯扇形判断。
- `Assets/_MMF/Scripts/Combat/PlayerAttack.cs` — 新增攻击逻辑。
- `Assets/_MMF/Tests/EditMode/WeaponArcTests.cs` — 新增 11 个测试用例。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 增加资源及组件生成逻辑。

## 疑问（需要确认，没有就写"无"）

无。

## 决定 / 假设（仅限技术层面）

- 使用复用的 List 查询碰撞体、HashSet 对怪物去重，无固定命中数量上限。
- 测试程序集已有运行时引用，无需修改 asmdef。

## 用户需要在 Unity 里做的事

- 等待脚本导入后，运行 `MMF/Build Prototype Scene` 三次。
- 运行全部 EditMode 测试。
- Claude 在 Play 中调用 `TryAttack(世界坐标)` 验证；本任务没有鼠标操作。

## 已知问题 / 未完成

当前没有 Unity 执行工具，因此尚未生成资源、更新场景或执行 Unity 测试。未手写 YAML 或 `.meta`。