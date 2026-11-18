## 完成情况

- ❌ `MMF.Runtime`、`MMF.Editor` 和 `MMF.Tests.EditMode` 使用 Unity 自带的编译器编译通过，无错误或警告。场景连续生成 3 次、生成后引用检查及 93 项测试执行仍待验证。
- ✅ 已实现车外追击时停步、闪约 0.5 秒、扑击并扣血。默认每只怪约每 2 秒扣 1 格，玩家血量从 5 降至 0 后无法再被选为攻击目标。
- ✅ 已实现蹲伏期间走出范围时扑空，随后进入冷却。
- ✅ 已实现蹲伏期间进车时扑空。
- ✅ 已实现玩家在车内时，车头和车尾的怪物不会发起攻击。
- ✅ 已接入死亡取消和重生重置。平底锅打中未击杀的怪物时，蹲伏计时继续。
- ✅ 已接入「玩家血量」和「怪物」数值组。攻击范围和冷却在判定时读取，最大血量在玩家启用时读取。窗口显示与 Play 中调值仍待验证。
- ❌ 扑击复用原有碰撞裁切，挤动和受击后退保留优先级。原有追击、绕车和避让仍待 Play 回归。

## 改动文件

- `Assets/_MMF/Scripts/Data/PlayerHealthTuning.cs` — 新增最大血量数值。
- `Assets/_MMF/Scripts/Data/MonsterDefinition.cs` — 新增攻击范围、提示时长、冷却和伤害。
- `Assets/_MMF/Scripts/Player/PlayerHealth.cs` — 新增满血初始化、扣血和 `Changed` 事件。
- `Assets/_MMF/Scripts/Combat/MonsterAttack.cs` — 新增蹲伏提示、扑击判定、冷却和重生重置。
- `Assets/_MMF/Scripts/Combat/Monster.cs` — 接入攻击位移、碰撞停止和受击闪白状态。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 挂载并连接新组件，缺少血量数值资产时创建。

## 疑问（需要确认，没有就写"无"）

无。

## 决定 / 假设（仅限技术层面）

- 受击闪白优先于蹲伏提示色。闪白结束时，若仍在蹲伏，就恢复提示。
- 扑击按物理帧移动，由现有碰撞检测裁切。保留 `TakeHit` 的 `direction` 参数供 T009c 使用。
- 保留 `Monster.cs` 工作副本原有的 CRLF。新增脚本使用 LF。

## 用户需要在 Unity 里做的事

1. 连续运行 3 次 `MMF → Build Prototype Scene`，检查玩家和五只怪物的新组件引用，再在 Test Runner 运行现有 93 项测试。
2. 进入 Play，分别验证车外站定挨打、蹲伏时走出范围或进车、蹲伏时击杀后等待重生。通过 `PlayerHealth.Current` 检查扣血，并在 `MMF → 数值` 验证范围和冷却的实时修改。

## 已知问题 / 未完成

桌面工具返回未获准操作 Unity。因此 `Prototype.unity` 尚未重新生成，`PlayerHealthTuning.asset` 尚未创建，测试运行和试玩验收也未完成。