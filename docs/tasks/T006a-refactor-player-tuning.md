# T006a — 整理：玩家数值搬进数值文件（行为不变）

> 给 Codex：先读 `AGENTS.md`（尤其是新加的 "Tunable gameplay values (数值)"，以及 "Good structure, minimal changes" 里的 Refactor tasks），再读本说明书和 `docs/GDD.md` §6.4。
> 这是**整理小任务**：只把数值搬家，**不加功能、行为完全不变**。数值窗口是 T006d，这里不做。

## 目标
玩家移动相关的玩法数值，现在写在场景组件 `PlayerMovement`、`PlayerShove` 的字段上，每次生成场景都会被重置。改成放在一个数值文件 `PlayerTuning`（ScriptableObject）里，组件引用这个文件、用的时候读，这样以后在数值窗口里改了能永久保存，Play 中改也马上生效。

## 要做的事
1. 新建 `PlayerTuning`（ScriptableObject，`Scripts/Data/`，`[CreateAssetMenu]`），字段（默认值必须和现在一样）：
   | 字段 | 默认值 | 原来在 |
   |---|---|---|
   | 最大速度 | 5 | `PlayerMovement._maxSpeed` |
   | 加速时间 | 0.1 | `PlayerMovement._accelerationTime` |
   | 减速时间 | 0.1 | `PlayerMovement._decelerationTime` |
   | 挤怪物时的速度比例 | 0.5 | `PlayerShove._speedRatio` |
   序列化字段 + 只读属性，`Min` / `Range` 限制和原来一样。
2. `PlayerMovement`：删掉这 3 个字段，改成 `[SerializeField] PlayerTuning _tuning`，用到的地方从 `_tuning` 读（每次用时读，不要在 `Awake` 里缓存数值）。`MaxSpeed` 属性改成读 `_tuning`。其他逻辑一行都不改。
3. `PlayerShove`：删掉 `_speedRatio`，改成从 `PlayerTuning` 读（可以引用同一个 `PlayerTuning`）。`_detectionDistance` 是技术数值，**留在组件上不动**。
4. 生成器：数值文件放在 `Assets/_MMF/Data/Tuning/PlayerTuning.asset`。**已经存在就直接用，不覆盖里面的值**；不存在才用默认值创建。把它填进 `PlayerMovement` 和 `PlayerShove`。
5. 现有测试（`FryingSequenceTests` 里会 `AddComponent<PlayerMovement>()`）要继续通过。如果测试需要 `PlayerTuning`，在测试里 `CreateInstance` 一个并用 `SerializedObject` 填进去，测试结束销毁。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；原有 93 个测试全部通过
- [ ] 生成 3 次后 `PlayerTuning.asset` 只有一个；手动把里面的最大速度改成 7 再生成场景，值还是 7（生成器没覆盖）
- [ ] Play：玩家速度 5、起步 / 停下的顺滑感、挤怪物时速度 2.5，都和原来一样
- [ ] Play 中把数值文件的最大速度改成 8：玩家马上变快（不用重开 Play）
- [ ] `PlayerMovement`、`PlayerShove` 上不再有这 4 个数值字段

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/PlayerTuning.cs` | 新增 | 玩家数值文件 |
| `Assets/_MMF/Scripts/Player/PlayerMovement.cs` | 修改 | 只把 3 个数值字段换成读 `PlayerTuning` |
| `Assets/_MMF/Scripts/Combat/PlayerShove.cs` | 修改 | 只把 `_speedRatio` 换成读 `PlayerTuning` |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加 `PlayerTuning` 资源的加载 / 创建和引用填写 |
| `Assets/_MMF/Tests/EditMode/FryingSequenceTests.cs` | 修改（只在需要时） | 只给测试里的 `PlayerMovement` 填 `PlayerTuning` |
| `Assets/_MMF/Data/Tuning/PlayerTuning.asset`、`Assets/_MMF/Scenes/Prototype.unity` | 由生成器生成 / 更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM）
- **行为必须和原来完全一样**，数值默认值必须一模一样
- 不要做：数值窗口、标记属性（T006d）；背包 / 煎锅 / 镜头的数值（T006b / T006c）
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
这一步只是整理，玩起来和原来一模一样：菜单 `MMF → Build Prototype Scene` → Play，走路、挤怪物都和之前一样。
