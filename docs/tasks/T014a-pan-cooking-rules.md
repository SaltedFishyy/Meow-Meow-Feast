# T014a — 新版煎锅：锅台自己管"煎的规则"（先不做画面和鼠标）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §5.5.1 **"新版"**那一段。
> 新版煎锅分几步做：**a 规则（本任务）** → b 锅上方的画面和提示 → c 鼠标拖放和划一下翻面 → d（用户确认后）删掉旧版。
> **旧版（E 键 + WASD，`PlayerInteractor` / `FryingPanel` / `FryingSequence`）这次不删、不改行为**，和新版并存。

## 目标
新版的规则放在**锅台自己身上**，所以玩家走开以后锅里照样在煎。

- 新的纯 C# 类 `Scripts/Cooking/PanCookingModel.cs`（不继承 MonoBehaviour，方便测试），状态：
  - `Empty`（空锅）→ 放入生食材 → `CookingFirst`（煎第 1 面）→ 时间到 → `ReadyFirst`（可以翻）→ 翻 → `CookingSecond` → 时间到 → `ReadySecond` → 翻 → **煎好**：吐出结果（`ItemDefinition.FryingResult`），锅变空。
  - `ReadyFirst` / `ReadySecond` 里超过"焦"的时间还没翻 → `Burnt`：锅里的东西变成焦土（留在锅里）。
  - 锅里有**煎好但背包放不下**的东西、或焦土、或生的但被拿走前：都能"拿回"。
- 对外的操作（返回成功 / 失败，失败时调用方会显示"x"）：
  - `TryPlace(ItemDefinition item)`：锅空、`item.FryingResult != null` 才行 → 开始煎第 1 面。
  - `TryFlip()`：只有 `ReadyFirst` / `ReadySecond` 时成功；煎好时通过返回值 / 事件给出煎好的物品。其他状态（包括还没煎够 3 秒）→ 失败，没有别的效果。
  - `TryTakeBack(out ItemDefinition item)`：锅里有东西（生的、正在煎的、可以翻的、焦土、煎好留在锅里的）就拿出来、锅变空、计时清掉。
  - `PutBackCooked(ItemDefinition item)`：煎好但背包满了时，把煎好的放回锅里留着（状态例如 `Holding`，只能拿回）。
  - `Tick(float deltaTime)`：推进计时。
  - 只读：当前状态、锅里的物品、当前这面已经煎了几秒、这面要煎几秒、已经可以翻了几秒、几秒后焦（给 T014b 画面用）。
  - 一个"状态变了"的事件（T014b 画面要听）。
- 新组件 `Scripts/Cooking/PanCooking.cs`（挂在锅台上，和 `FryingStation` 同一个物体）：
  - 每帧用 `Time.deltaTime` 推进模型（锅台被举着时也照样煎）。
  - 锅里显示的东西用现有的 `FryingStation.SetContents` 更新（生的颜色 → 焦土颜色等），这样锅台上的小方块照旧显示锅里有什么。
  - **和旧版并存**：如果旧版（E 键）改了锅里的东西，新版的状态要清回 `Empty`/对应内容，不能两边打架。你选简单可靠的做法（例如 `FryingStation` 加一个"内容变了"事件，新版自己改的时候不重置），回报写明。旧版的行为不能变。
  - 对外暴露模型的操作（`TryPlace` 等）给 T014c 用；这次没有人调用它们，测试会调用。
- `CookingTuning` 加 2 个数值（`TuningLabel` 中文名、`Min`）：
  | 字段 | 中文名 | 默认 |
  |---|---|---|
  | `_sideSeconds` | 每面煎几秒 | 3 |
  | `_burnSeconds` | 可以翻以后几秒焦 | 5 |
  Play 中改了，下一面开始时生效。
- 生成器：给锅台加 `PanCooking`、填引用。已有的 `CookingTuning.asset` 不覆盖（新字段用默认值）。

## 测试
新 EditMode 测试 `Assets/_MMF/Tests/EditMode/PanCookingModelTests.cs`，至少覆盖：
- 空手 / 不能煎的东西 / 锅里已有东西时 `TryPlace` 失败
- 没到 3 秒翻 → 失败、状态不变；到了 → 成功进入第 2 面
- 两面都翻完 → 吐出 `FryingResult`、锅空
- 可以翻以后 5 秒没翻 → 焦土留在锅里；之后翻 → 失败；拿回 → 得到焦土
- 任何时候拿回 → 得到锅里的东西、锅空、计时清掉
- `PutBackCooked` 后只能拿回、不会再焦

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；原来 93 个 + 新测试全部通过
- [ ] Play：旧版 E 键 + WASD 煎东西和以前完全一样
- [ ] 数值窗口"煎锅"组多了"每面煎几秒""可以翻以后几秒焦"
- [ ] （Claude 用 script eval 测）对锅台的 `PanCooking` 调 `TryPlace(蘑菇)`，3 秒后能翻，6 秒后再翻得到煎蘑菇；不翻则 8 秒后变焦土；锅台上的小方块颜色跟着变

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Cooking/PanCookingModel.cs` | 新增 | 规则（纯 C#） |
| `Assets/_MMF/Scripts/Cooking/PanCooking.cs` | 新增 | 挂在锅台上推进计时、更新锅里显示 |
| `Assets/_MMF/Scripts/Cooking/FryingStation.cs` | 修改（只在需要时） | 只加"内容变了"的通知之类的最小改动 |
| `Assets/_MMF/Scripts/Data/CookingTuning.cs` | 修改 | 只加 2 个数值 |
| `Assets/_MMF/Tests/EditMode/PanCookingModelTests.cs` | 新增 | 测试 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加组件和引用 |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不要做：锅上方的画面和提示（T014b）、鼠标拖放和翻面（T014c）、删旧版（T014d）
- `Update` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
- 这一步没有新的画面和操作，旧版煎锅照常能玩。新规则由 Claude 用脚本测。
