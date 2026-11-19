## 完成情况

- ❌ Unity 无报错无警告编译、连续生成场景 3 次、生成引用检查和 84 项测试均未验证。
- ✅ 右侧锅窗口显示时，按 E 尝试放入选中格的 1 件物品。成功才扣除；锅已占用、选中格为空或物品不可煎时显示 `x`。已静态核对，待试玩。
- ✅ 头顶小锅显示时走同一条放置路径。已静态核对，待试玩。
- ✅ 锅画面隐藏、玩家倒地、失败画面出现或输入停用时，E 不触发放置或 `x`。已静态核对，待试玩。
- ✅ 鼠标拖放、划动翻面和举锅台的处理代码保留原样。拖动期间仍可按 E，拖动来源格的最后一件被取走时，由现有逻辑取消拖动。待试玩回归。

## 改动文件

- `Assets/_MMF/Input/MMFControls.inputactions` — 新增 `Gameplay/PlaceInPan` 按钮动作，绑定 E。
- `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` — 订阅新动作并调用 `TryPlaceSelected()`。
- `Assets/_MMF/Scripts/UI/PanDragInput.cs` — 新增 `TryPlaceSelected()`，复用锅画面判断、放置规则和失败反馈。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 在 `requiredActions` 中加入 `PlaceInPan`。

## 疑问（需要确认，没有就写"无"）

- 无。

## 决定 / 假设（仅限技术层面）

- 复用 `Gameplay` 动作映射的启停机制，以及鼠标拖放使用的 `PanCooking.TryPlace` 和背包扣除接口。四个文件的 LF 和 BOM 状态均已核对。

## 用户需要在 Unity 里做的事

- 在可保存生成产物的工作副本中，连续运行 `MMF → Build Prototype Scene` 3 次，检查引用与 Console，再运行全部 84 项测试。
- 进入 Play，捡蘑菇，用 1–5 或滚轮选中对应格，走近锅台按 E。确认蘑菇进锅且该格少 1 个，再按 E 确认显示 `x`。分别用右侧窗口和头顶小锅验证，并检查远离锅台、空格、平底锅和鼠标拖动中的行为。

## 已知问题 / 未完成

- 场景生成器会写入清单外的场景和资源，本次未运行。Unity 编译、测试及试玩结果仍待确认。