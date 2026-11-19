## 完成情况

- ❌ 已删除旧版 E / Esc 煎菜、WASD 字母判定和右侧面板。Unity 编译、连续三次场景生成、引用检查及预计剩余的 84 个测试均待验证。本次删除的测试文件包含 23 个用例。
- ❌ `_MMF` 的代码、输入和生成器已清除指定旧符号。现有 `Prototype.unity` 仍有旧面板和 `_legacyInteractor` 引用。历史任务文档和范围外的 `Assets/Settings` 默认输入资产也有匹配项。
- ❌ 静态检查确认 WASD 移动绑定仍在，E / Esc 煎菜绑定已移除，1–5 和滚轮已解除旧煎菜限制。新版拖放、翻面、成品入包、烧焦和拖回待 Play 验证。
- ❌ 搬锅台的举起、预览、放下与倒地回退逻辑已保留。举着时仍会阻止 Q 和右键蓄力，待 Play 验证。
- ❌ `CookingTuning` 源码只剩互动距离、翻面动画几秒、每面煎几秒、可以翻以后几秒焦、快焦提示提前几秒，待在数值窗口核对。

## 改动文件

- `Assets/_MMF/Scripts/UI/FryingPanel.cs`、`Assets/_MMF/Scripts/Cooking/FryingSequence.cs`、`Assets/_MMF/Tests/EditMode/FryingSequenceTests.cs` — 删除，各自的 `.meta` 一并删除。
- `Assets/_MMF/Scripts/Player/PlayerInteractor.cs` — 移除旧煎菜会话，`CanDrop` 只判断是否举着锅台。
- `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` — 移除旧煎菜订阅和选格限制。
- `Assets/_MMF/Scripts/Player/PlayerDowned.cs` — 移除倒地时退出旧煎菜的调用。
- `Assets/_MMF/Scripts/Cooking/PanCooking.cs`、`PanCookingModel.cs`、`FryingStation.cs` — 移除旧批量内容和双向同步，保留新版食物标记。
- `Assets/_MMF/Scripts/Data/CookingTuning.cs` — 移除三个旧煎菜数值。
- `Assets/_MMF/Input/MMFControls.inputactions` — 删除六个旧动作及绑定。该资产未启用 C# 包装类生成。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 移除旧面板的生成和引用，继续为新版加载同一张锅图。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

新版锅内物品由 `PanCookingModel` 管理，锅台仅在显示的物品变化时刷新食物标记。保留了现有 `CookingTuning.asset` 和全部美术资源。

## 用户需要在 Unity 里做的事

1. 连续运行三次 `MMF → Build Prototype Scene`，检查 Console 和组件引用，再在 Test Runner 运行剩余测试。
2. Play 后捡蘑菇，进车拖进小锅，等提示后划两次，确认成品进背包。另试烧焦、拖回、E / Esc、WASD、1–5 和滚轮。
3. 左键举锅台，检查影子、红 `x`、车内放下、举锅时 Q 和右键受限，以及车外倒地后锅台回到原位。打开 `MMF → 数值` 核对煎锅组。

## 已知问题 / 未完成

自动审批拒绝了通过 computer-use 操作 Unity 编辑器。`Prototype.unity` 尚未由生成器更新，Unity 内的验收均未执行。源文件的 `git diff --check` 已通过，输入资源已通过 JSON 解析。