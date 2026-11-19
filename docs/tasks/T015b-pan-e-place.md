# T015b —【试验】锅的画面显示时，按 E 把选中格的东西放进锅

> 给 Codex：先读 `AGENTS.md`，再读本说明书。这是用户的**试验**的一部分，**不写进 GDD**。
> 现状：`MMF.UI.PanDragInput` 管鼠标拖进 / 拖回 / 划动翻面，知道当前显示的是哪套锅画面（右边窗口或头顶小锅，`VisibleView`）；`MMF.Cooking.PanCooking.TryPlace`；`PlayerBackpack.ShowFailure()` 显示头顶"x"。T014d 删掉了旧版的 `Interact`（E）动作。

## 目标
- **锅的画面显示时**（右边窗口或头顶小锅，哪种都行），按 **E**：
  - 从背包**当前选中的那一格**拿 1 个放进锅（`PanCooking.TryPlace`），成功才从那一格扣 1 个——效果和用鼠标拖进去完全一样。
  - 放不进去 → 头顶显示"x"（`ShowFailure`），背包不变。包括：锅里已经有东西（不管生的、在煎的、焦土、煎好留着的）、选中的格子是空的、选中的是不能煎的东西（平底锅、煎好的食物、焦土）。
- **锅的画面没显示时**按 E：什么都不发生（不显示"x"）。
- 玩家倒地、失败画面、输入被关掉时 E 无效（和其他按键一样）。
- 正在用鼠标拖东西时按 E：照样按上面规则放（如果放进去的正好是正在拖的那一格的最后一个，拖动自然取消，T014c1 已经这样处理）。

## 结构
- `MMFControls.inputactions`：在 `Gameplay` 里加回一个按钮动作（名字例如 `PlaceInPan`），绑定 `<Keyboard>/e`。生成器 `requiredActions` 加上这个名字。
- `PlayerInputReader`：订阅这个动作，调用 `PanDragInput` 上的一个新公开方法（例如 `TryPlaceSelected()`），别的不动。
- `PanDragInput`：加这个方法（用现有的 `VisibleView` 判断画面是否显示）。
- 新数值：无。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；84 个测试全部通过
- [ ] 右边窗口显示时：选中蘑菇按 E → 蘑菇进锅开始煎、那一格少 1 个；再按 E → "x"；选中平底锅 / 空格按 E → "x"
- [ ] 关掉开关（头顶小锅）时同样能用 E 放
- [ ] 离锅台远（画面没显示）按 E → 没反应、没有"x"
- [ ] 鼠标拖放、划动翻面、举锅台都和现在一样

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Input/MMFControls.inputactions` | 修改 | 只加 1 个动作和它的 E 绑定 |
| `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` | 修改 | 只加这个动作的订阅 |
| `Assets/_MMF/Scripts/UI/PanDragInput.cs` | 修改 | 只加"放选中格"的方法 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只改 `requiredActions` |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不改煎的规则；不改 GDD
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，捡蘑菇，用 1–5 / 滚轮选中蘑菇那一格，走到锅台旁边，按 E：蘑菇进锅。
2. 锅里有东西时再按 E：头顶出现"x"。
