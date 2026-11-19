# T014d — 删掉旧版煎锅（E 键 + WASD 按字母煎）

> 给 Codex：先读 `AGENTS.md`（尤其是"不留用不到的代码"），再读本说明书和 `docs/GDD.md` §5.5.1（已经改好：只剩新版 + "搬锅台"）。
> 用户试玩了新版（T014a～c2：靠近弹出、鼠标拖放、划一下翻面），说好玩，要删掉旧版。
> 这是**删代码**的任务：**新版煎锅和搬锅台的行为必须和现在完全一样**，只是旧版没了。

## 要删的（旧版）
- 按 **E** 把东西放进锅 / 拿回、自动打开右边的煎菜画面；**Esc / E** 退出。
- 右边的煎菜画面 `UI/FryingPanel` 和按 W/A/S/D 字母的规则 `Cooking/FryingSequence`（连同测试 `Tests/EditMode/FryingSequenceTests.cs`）。
- 旧版煎的时候：玩家不能走、1–5 和滚轮不能用、左键不能打、不能 Q / 右键——这些"正在煎"的限制都删掉（新版没有"正在煎"的模式）。
- 只给旧版用的东西：
  - `PlayerInteractor` 里煎菜的部分（`Interact`、`Cancel`、`Press`、`Cooking`、`ReturnContents`、煎完的处理、`_panel` 引用等）；
  - `PanCooking` 里和旧版并存的部分（`_legacyInteractor` 和所有 `Cooking` 判断、"锅里有好几个"的旧批量处理、`FryingStation.ContentsChanged` 的同步）；`PanCookingModel.SetExternalContents` 如果没人用了也删；
  - `FryingStation` 里只给旧版用的（`CreateSequence`、`ItemsPerBatch`、`ContentsChanged` 等——没人用了就删）；
  - `CookingTuning` 里只给旧版用的数值：**一次放几个**、**每轮几个字母**、**每轮几秒**。**"翻面动画几秒"新版在用，保留**；"互动距离"、新版 3 个数值保留；
  - 输入：`Assets/_MMF/Input/MMFControls.inputactions` 里的 `Interact`（E）、`Cancel`（Esc）、`FryW` / `FryA` / `FryS` / `FryD` 动作和绑定（**WASD 移动不动**）；`PlayerInputReader` 里对应的订阅和 `Cooking` 判断；生成器 `requiredActions` 里的这几个名字；
  - `PlayerDowned` 里"倒地时退出煎菜"那一句（倒地时放下举着的锅台**保留**）；
  - 生成器里建 `FryingPanel` 画面、填它引用的代码。
- 删之前 grep 一下脚本 GUID，确认场景 / 预制体里没有别的地方引用；`.cs` 连同 `.meta` 一起删。

## 要保留的（行为不能变）
- **搬锅台**全部保留：锅台旁边左键举起、再点放下、放置预览（影子 + 红"x"）、只能放在车里、倒地时放下 / 放回原处、举着时不能 Q / 右键蓄力（`PlayerInteractor.CanDrop` 变成只看"是不是举着"）。
- 新版煎锅（`PanCooking`、`PanCookingModel`、`PanCookingView`、`PanDragInput`）的所有行为。
- `PlayerMovement.SetBlocked`（倒地还在用）。
- **美术 / 字体 / 贴图等资源一个都不要删**（例如旧画面用的锅图，新版也在用）。如果发现某个资源以后没人用了，写进回报，由用户决定。
- 已有的 `CookingTuning.asset` 不要覆盖、不要手改（删掉的字段留在文件里不影响）。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；剩下的测试全部通过（107 减去删掉的 `FryingSequenceTests`）
- [ ] 全项目 grep 不到 `FryingPanel`、`FryingSequence`、`_legacyInteractor`、`LettersPerRound`、`RoundSeconds`、`ItemsPerBatch`、`FryW`、`"Interact"`
- [ ] Play：新版煎锅照常（拖进、划翻、煎好进背包、焦、拖回）；E / Esc 按了没反应；WASD 只是移动；1–5、滚轮随时能用
- [ ] 搬锅台照常：举起、预览、放下、放不下时红"x"；举着时不能 Q / 右键蓄力；车外举着被打倒 → 锅台回到举起前的位置
- [ ] 数值窗口"煎锅"组只剩：互动距离、翻面动画几秒、每面煎几秒、可以翻以后几秒焦、快焦提示提前几秒

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 / 删除 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/UI/FryingPanel.cs`（+ .meta） | 删除 | — |
| `Assets/_MMF/Scripts/Cooking/FryingSequence.cs`（+ .meta） | 删除 | — |
| `Assets/_MMF/Tests/EditMode/FryingSequenceTests.cs`（+ .meta） | 删除 | — |
| `Assets/_MMF/Scripts/Player/PlayerInteractor.cs` | 修改 | 只删煎菜部分，搬锅台不动 |
| `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` | 修改 | 只删 E / Esc / 字母键订阅和 `Cooking` 判断 |
| `Assets/_MMF/Scripts/Player/PlayerDowned.cs` | 修改 | 只删退出煎菜那一句 |
| `Assets/_MMF/Scripts/Cooking/PanCooking.cs` | 修改 | 只删和旧版并存的部分 |
| `Assets/_MMF/Scripts/Cooking/PanCookingModel.cs` | 修改（只在需要时） | 只删没人用的 `SetExternalContents` |
| `Assets/_MMF/Scripts/Cooking/FryingStation.cs` | 修改 | 只删旧版专用的成员 |
| `Assets/_MMF/Scripts/Data/CookingTuning.cs` | 修改 | 只删 3 个旧版数值 |
| `Assets/_MMF/Tests/EditMode/PanCookingModelTests.cs` | 修改（只在需要时） | 只删测已删方法的部分 |
| `Assets/_MMF/Input/MMFControls.inputactions` | 修改 | 只删 6 个动作和它们的绑定 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只删旧画面的建法、引用、`requiredActions` 里的名字 |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

（`.inputactions` 如果有 Unity 生成的 C# 包装类，也一并处理并在回报说明。）

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不加任何新功能，不改新版和搬锅台的行为
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，捡蘑菇，进车走到锅台旁边，拖进小锅，划两次翻面，煎蘑菇进背包——和之前一样。
2. 按 E、Esc 都没反应了；右边不会再弹出旧的煎菜画面。
3. 锅台旁边左键举起锅台、走两步再点一下放下——和之前一样。
