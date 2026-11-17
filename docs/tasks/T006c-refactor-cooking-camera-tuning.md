# T006c — 整理：煎锅、镜头数值搬进数值文件（行为不变）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)" 和 Refactor tasks），再读本说明书和 `docs/GDD.md` §6.4。T006a（`PlayerTuning`）、T006b（`BackpackTuning`）已完成并提交，照它们的做法来。
> 这是**整理小任务**：只把数值搬家，**不加功能、行为完全不变**。

## 目标
煎锅台和镜头的玩法数值，从场景组件搬进两个数值文件。组件引用数值文件、用的时候读。

## 要做的事
1. 新建 `CookingTuning`（ScriptableObject，`Scripts/Data/`，`[CreateAssetMenu]`），默认值必须和现在一样：
   | 字段 | 默认值 | 原来在 | 说明 |
   |---|---|---|---|
   | 互动距离 | 1 | `FryingStation._interactionDistance` | |
   | 一次放几个 | 1 | `FryingStation._itemsPerBatch` | |
   | 每轮几个字母 | 4 | `FryingStation._lettersPerRound` | 开始煎的时候读，下一次开始煎生效（Tooltip 写明） |
   | 每轮几秒 | 10 | `FryingStation._roundSeconds` | 同上 |
   | 翻面动画几秒 | 0.4 | `FryingPanel._flipSeconds` | |
2. 新建 `CameraTuning`（ScriptableObject，`Scripts/Data/`，`[CreateAssetMenu]`）：
   | 字段 | 默认值 | 原来在 |
   |---|---|---|
   | 车里镜头大小 | 5 | `FollowCamera._insideSize` |
   | 车外镜头大小 | 7 | `FollowCamera._outsideSize` |
   | 拉近拉远时间 | 0.5 | `FollowCamera._zoomDuration` |
   | 跟随延迟 | 0.2 | `FollowCamera._followTime` |
3. `FryingStation`、`FryingPanel`：删掉上面那些字段，改成 `[SerializeField] CookingTuning _tuning`，用的时候读。`FryingPanel._widthFraction`（画面宽度）是界面排版，**留在组件上不动**。
4. `FollowCamera`：删掉那 4 个字段，改成 `[SerializeField] CameraTuning _tuning`，用的时候读。`OutsideSize` 属性改成读 `_tuning`。
5. 生成器：`Assets/_MMF/Data/Tuning/CookingTuning.asset`、`CameraTuning.asset`，**已经存在就不覆盖**，不存在才用默认值创建；填进对应组件。如果生成器里有直接设置这些旧字段的代码，删掉。
6. 现有测试如果会 `AddComponent<FryingStation>()` / `FollowCamera` 并用到这些数值，在测试里 `CreateInstance` 对应的数值文件填进去，测试结束销毁（照 T006a 对 `FryingSequenceTests` 的做法）。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；原有 93 个测试全部通过
- [ ] 生成 3 次后两个数值文件各只有一个；手动改值再生成场景，值不被覆盖
- [ ] Play：煎锅的互动距离、一次 1 个、每轮 4 个字母、每轮 10 秒、翻面动画，都和原来一样；镜头车里 5、车外 7、切换 0.5 秒、跟随有一点延迟，都和原来一样
- [ ] Play 中把车外镜头大小改成 9：出车后镜头马上变成 9
- [ ] 那些组件上不再有被搬走的数值字段

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/CookingTuning.cs` | 新增 | 煎锅数值文件 |
| `Assets/_MMF/Scripts/Data/CameraTuning.cs` | 新增 | 镜头数值文件 |
| `Assets/_MMF/Scripts/Cooking/FryingStation.cs` | 修改 | 只把 4 个数值字段换成读 `CookingTuning` |
| `Assets/_MMF/Scripts/UI/FryingPanel.cs` | 修改 | 只把 `_flipSeconds` 换成读 `CookingTuning` |
| `Assets/_MMF/Scripts/Core/FollowCamera.cs` | 修改 | 只把 4 个数值字段换成读 `CameraTuning` |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加两个数值文件的加载 / 创建和引用填写，删掉旧字段的设置 |
| `Assets/_MMF/Tests/EditMode/FryingSequenceTests.cs` | 修改（只在需要时） | 只给测试里的组件填数值文件 |
| `Assets/_MMF/Data/Tuning/*.asset`（新的两个）、`Assets/_MMF/Scenes/Prototype.unity` | 由生成器生成 / 更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM）
- **行为必须和原来完全一样**，默认值必须一模一样
- 不要做：数值窗口、标记属性（T006d）
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
这一步只是整理，玩起来和原来一模一样。
