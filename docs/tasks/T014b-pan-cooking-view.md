# T014b — 新版煎锅 2：锅台上方的小锅画面 + 4 个提示

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §5.5.1 **"新版"**那一段。
> T014a 已完成并提交：锅台上的 `MMF.Cooking.PanCooking`（`State`、`Item`、`SideElapsed`、`SideSeconds`、`ReadyElapsed`、`BurnSeconds`、事件 `StateChanged`），规则在 `PanCookingModel`。
> 这是**小任务**：只做**显示**。**不做**：鼠标拖放和划一下翻面（T014c）、删旧版（T014d）。旧版（E 键 + 右边的 `FryingPanel`）保持原样。

## 目标
- **锅的画面**：锅台**正上方**一个小锅画面，约 **3 × 3 世界单位**（大小是界面排版，放组件序列化字段，不进数值文件），跟着锅台走（锅台被举着时也跟着）。用**世界空间 Canvas**（T014c 要在上面接鼠标事件，Canvas 的 `worldCamera` 填主镜头），排在游戏物体上面，不被车墙、玩家挡住。
  - 背景：锅（先用旧版 `FryingPanel` 用的那张占位锅图 / 一个深色圆形占位）。
  - 中间：锅里的食物（方块，颜色 = 物品颜色）。
- **什么时候显示**：玩家离锅台在"互动距离"（`CookingTuning.InteractionDistance`，用现有的 `FryingStation.InRange`）以内就显示，走远就隐藏。锅里有没有东西都显示（空锅时就是空的锅）。隐藏时锅里照样在煎（T014a 已经是这样）。不要每帧 `Find` / `GetComponent`。
- **食物的样子**：
  - 生的、煎第 1 面：物品颜色。
  - 翻过一次以后（`CookingSecond` / `ReadySecond`）：翻上来的那面"煎过"的颜色（物品颜色调深，和旧版翻面后的做法一样）。
  - 焦土 / 煎好留在锅里（`Burnt` / `Holding`）：那个物品自己的颜色。
- **4 个提示**（GDD 新版 1～4）：
  1. **熟度进度**：煎的时候（`CookingFirst` / `CookingSecond`）食物外面一圈进度环（`Image` 径向填充），从 0 填到满 = `SideElapsed / SideSeconds`。
  2. **可以翻了**：`ReadyFirst` / `ReadySecond` 时食物一闪一闪（亮度或大小脉动），旁边显示"↕"（或"翻！"之类的字，用和背包栏一样能显示中文的字体）。
  3. **快焦了**：可以翻以后，离焦还剩"快焦提示提前几秒"（新数值，默认 **2** 🟡）以内时：食物闪红 + 冒烟（几个灰色小方块往上飘、慢慢变淡，占位）。
  4. **翻面动画**：每次翻面（`ReadyFirst → CookingSecond`、`ReadySecond → 煎好`）播一个翻过来的动作：食物纵向压扁到 0 再弹回来，时长用现有的"翻面动画几秒"（`CookingTuning.FlipSeconds`）。第 1 次翻到一半时换成"煎过"的颜色；第 2 次（煎好，锅变空）翻完以后消失。
- 只监听 `PanCooking.StateChanged` 切换状态；进度环、闪烁、冒烟这种连续的东西在 `Update` 里按现有数值算，不分配垃圾。
- 新数值放进 `CookingTuning`：
  | 字段 | 中文名 | 默认 |
  |---|---|---|
  | `_burnWarningSeconds` | 快焦提示提前几秒 | 2 |
  颜色、大小、闪烁快慢、烟的数量和速度是外观，放组件序列化字段。

## 结构
- 新脚本 `Assets/_MMF/Scripts/UI/PanCookingView.cs`：只管显示（读 `PanCooking`）。显示 / 隐藏的距离判断可以放在里面，也可以单独一个小脚本，最多 2 个新脚本，回报说明。
- 生成器：建世界空间 Canvas 和里面的元素（锅、食物、进度环、"↕"字、烟），填引用。已有的 `CookingTuning.asset` 不覆盖。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；105 个测试全部通过
- [ ] Play：走到锅台旁边，锅台上方出现小锅；走开就消失；回来又出现
- [ ] （Claude 用 script eval 调 `TryPlace` / `TryFlip` 测）放蘑菇：进度环 3 秒填满 → 食物闪 + "↕" → 翻：压扁再弹回、变深色 → 再 3 秒 → 翻：翻完消失；不翻：最后 2 秒闪红冒烟 → 变焦土颜色
- [ ] 举着锅台时小锅跟着走
- [ ] 旧版 E 键 + WASD 照常能用
- [ ] 数值窗口"煎锅"组多了"快焦提示提前几秒"

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/UI/PanCookingView.cs` | 新增 | 小锅画面和提示 |
| （可选）`Assets/_MMF/Scripts/UI/` 下第 2 个脚本 | 新增 | 只在需要时（例如显示 / 隐藏） |
| `Assets/_MMF/Scripts/Data/CookingTuning.cs` | 修改 | 只加 1 个数值 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加画面的建法和引用 |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不要做：鼠标操作（T014c）、删旧版（T014d）、改 `PanCooking` / `PanCookingModel` 的规则
- `Update` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，在车里走到左上角锅台旁边：锅台上方冒出一个小锅，走开就消失。（还不能用鼠标放东西，T014c 才做。）
