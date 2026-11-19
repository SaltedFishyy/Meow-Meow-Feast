# T014c1 — 新版煎锅 3a：鼠标从背包栏拖进锅、从锅拖回背包

> 给 Codex：先读 `AGENTS.md`，再读本说明书和 `docs/GDD.md` §5.5.1 **"新版"**那一段（"放 / 拿（鼠标）"、"鼠标左键"）。
> 已完成：T014a `MMF.Cooking.PanCooking`（`TryPlace` / `TryFlip` / `TryTakeBack` / `PutBackCooked`、`StateChanged`），T014b `MMF.UI.PanCookingView`（锅台上方世界空间 Canvas 小锅，玩家在互动距离内才显示）。
> 新版煎锅 3 拆成两步：**c1 拖进 / 拖回（本任务）** → c2 划一下翻面。旧版（E 键 + WASD）保持原样。

## 目标
- **拖进锅**：小锅显示时，在**背包栏**某一格上按下鼠标左键 → 这一格的物品图标跟着鼠标走（一个半透明小方块，物品颜色）→ 在**小锅画面**上松开 → 从那一格拿 1 个，`PanCooking.TryPlace`。
  - 放不进去（锅里已有东西、这个东西不能煎、旧版正在煎）→ 物品留在背包，玩家头顶显示"x"（`PlayerBackpack.ShowFailure()`）。
  - 在别处松开 → 什么都不发生，图标消失。
  - 拖的时候不改变背包选中的格子（拖完以后选中格和以前一样）。
- **拖回背包**：在小锅画面里**按在食物上** → 食物图标跟着鼠标走 → 在**背包栏**上松开 → `PanCooking.TryTakeBack`，物品进背包（按正常捡东西的规则放）。
  - 背包放不下 → 锅里的东西**不动、状态不变**（正在煎的照样煎），显示"x"。所以要**先判断背包放得下**再拿（需要的话给 `BackpackModel` / `PlayerBackpack` 加一个"放得下吗"的只读方法）。
  - 在别处松开 → 什么都不发生。
- **左键的优先级**（GDD）：小锅显示时，鼠标按在小锅画面上、或按在背包栏的格子上 → 这一下左键给拖放用，**不**举锅台、**不**挥平底锅。其他情况左键照旧（锅台旁边举 / 放锅台，车外攻击）。小锅没显示时，背包栏上的左键也照旧。
- 玩家倒地、失败画面、玩家输入被关掉时，拖放也不能用；正在拖时发生这些 → 取消拖动。
- **翻面动画的遗留问题**：现在直接拿回锅里的东西（`ReadySecond → Empty`）也会播翻面动画。请让画面能分清"翻面"和"拿回"（例如 `PanCooking` 加一个翻面事件，`PanCookingView` 只在翻面事件时播动画），拿回时食物直接消失。
- 判断"鼠标在不在小锅 / 背包格子上"用 `RectTransformUtility.RectangleContainsScreenPoint`（小锅是世界空间 Canvas，要传它的镜头；背包栏是屏幕空间，传 null），不需要加 EventSystem。鼠标位置用 Input System（`Mouse.current` 或现有输入动作），不要用旧的 `Input` 类。

## 结构
- 新脚本 `Assets/_MMF/Scripts/UI/PanDragInput.cs`：只管"拖进 / 拖回"和跟着鼠标的图标。
- `PlayerInputReader`：左键（`Lift` 动作）处理开头加一句"鼠标在小锅 / 背包格子上就不举锅台、不攻击"（问 `PanDragInput`），别的不动。
- `BackpackBar`：如需要，对外提供"屏幕上某点在第几格"（只读方法），别的不动。
- `PanCooking`：只加区分翻面的事件（或等效做法）。`PanCookingView`：只改"什么时候播翻面动画"。
- `BackpackModel` / `PlayerBackpack`：只在需要时加"放得下吗"、"从指定格拿 1 个"之类的最小方法。
- 生成器：建跟着鼠标的图标（在背包栏那个 Canvas 上，最上层，不挡鼠标判断）、挂 `PanDragInput`、填引用。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；105 个测试全部通过（如果改了 `BackpackModel`，给新方法加测试）
- [ ] Play：在车里拿着蘑菇走到锅台旁 → 从背包栏把蘑菇拖到小锅 → 锅里出现蘑菇，开始转进度环，背包里少 1 个
- [ ] 锅里有东西时再拖一个进去 → 放不进、显示"x"、背包不变
- [ ] 把锅里的东西拖回背包栏 → 回到背包、锅空、**没有翻面动画**；背包满时拖回 → 显示"x"、锅里照样在煎
- [ ] 在小锅 / 背包格子上按左键不会举起锅台；在锅台旁其他地方按左键照样举锅台；车外照样能攻击
- [ ] 旧版 E 键 + WASD 照常能用

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/UI/PanDragInput.cs` | 新增 | 拖进 / 拖回 |
| `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` | 修改 | 只加左键让给拖放的判断 |
| `Assets/_MMF/Scripts/UI/BackpackBar.cs` | 修改（只在需要时） | 只加"某点在第几格"的只读方法 |
| `Assets/_MMF/Scripts/Cooking/PanCooking.cs` | 修改 | 只加区分翻面的事件 |
| `Assets/_MMF/Scripts/UI/PanCookingView.cs` | 修改 | 只改翻面动画的触发 |
| `Assets/_MMF/Scripts/Player/BackpackModel.cs`、`PlayerBackpack.cs` | 修改（只在需要时） | 只加最小的方法 |
| `Assets/_MMF/Tests/EditMode/`（`BackpackModel` 的测试） | 修改（只在改了 `BackpackModel` 时） | 新方法的测试 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加图标、组件和引用 |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不要做：划一下翻面（T014c2）、删旧版（T014d）
- `Update` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，捡一个蘑菇（地上米色小方块），进车走到左上角锅台旁边，上方出现小锅。
2. 从屏幕下面的背包栏按住蘑菇那一格，拖到小锅上松开：蘑菇进锅，开始转圈。
3. 从小锅里把蘑菇拖回背包栏：回到背包。
（翻面要等 T014c2。）
