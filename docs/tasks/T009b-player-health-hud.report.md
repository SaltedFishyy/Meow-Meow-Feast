## 完成情况

- ❌ 已接入场景生成器。自动审批拒绝了对 Unity 编辑器的操作，因此未确认编译是否无报错无警告，未执行三次场景生成和引用检查，也未运行 93 个测试。
- ❌ 已实现 `Max` 决定方块数量、`Current` 决定亮红格数量，扣除的格子显示为半透明深灰。尚未在 Play 中验证从 5 红到 0 红的过程。
- ❌ 血量栏锚定左上角，背包栏锚定底部。尚未在 Game 视图验证不同分辨率和宽高比下的实际布局。
- ✅ 代码没有 `Update`。扣血时通过 `PlayerHealth.Changed` 更新已有方块的颜色，无托管分配。仅在所需方块数量超过已有数量时创建方块。

## 改动文件

- `Assets/_MMF/Scripts/UI/PlayerHealthBar.cs` — 新增血量显示组件，方块大小、间距、边距和颜色均可在 Inspector 调整。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 添加左上角血量栏容器并绑定 `PlayerHealth`。

## 疑问

无。

## 决定 / 假设

- 复用背包栏的 uGUI Canvas 和缩放设置。血量栏使用独立的左上角容器，并在启用时同步当前血量。

## 用户需要在 Unity 里做的事

1. 连续运行三次 `MMF → Build Prototype Scene`，每次检查 `Player Health Row` 上的血量引用，再运行全部 93 个测试。
2. 按 Play，确认左上角有 5 个红方块。出车让怪物攻击，确认每次命中有一格转暗，血量归零时全部变暗。
3. 切换 Game 视图的分辨率和宽高比，确认血量栏始终贴在左上角且不遮挡背包栏。

## 已知问题 / 未完成

- `Assets/_MMF/Scenes/Prototype.unity` 尚未由生成器更新。现有场景需要先重新生成，才能试玩血量显示。