## 完成情况

- ❌ 新增窗口代码已用 Unity 6000.6.4f1 自带的 C# 编译器验证，零错误、零警告。Unity 项目编译和 93 个测试尚未验证。
- ❌ 已注册 `MMF → 武器工坊`。列表按路径排序，仅搜索武器目录及其子目录中的 `SkillDefinition`。`Tongs`、钩爪和平底锅的显示结果待 Unity 实测。
- ❌ 已嵌入 Ability Studio 原生检查器。参数显示和修改后写回资产待实测。
- ❌ 已接入「试玩预览」和「在 Project 里选中」，并保留原生检查器的「打开技能画板」。三个按钮待实测。
- ❌ 已通过 `OnProjectChange` 刷新列表。复制和删除 `Tongs` 后的刷新待实测。
- ❌ 已按资产 GUID 保存选择，找不到上次选择时回退到第一把。关闭再打开待实测。
- ✅ 本次仅新增下列两个文件，均为 LF、无 BOM。`LocalPackages/`、`TuningWindow.cs`、`MMF.Editor.asmdef` 和场景均无本次改动。

## 改动文件

- [MMF.Editor.WeaponWorkshop.asmdef](<C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Editor/WeaponWorkshop/MMF.Editor.WeaponWorkshop.asmdef>) — 独立 Editor 程序集，显式引用 `AbilityStudio.Model` 和 `AbilityStudio.Editor`。
- [WeaponWorkshopWindow.cs](<C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Editor/WeaponWorkshop/WeaponWorkshopWindow.cs>) — 实现可分别滚动的武器列表和参数区，以及预览、定位和选择恢复。

## 疑问

无。

## 决定 / 假设

- 用 `InspectorElement` 嵌入原生检查器，沿用 Ability Studio 的字段、画板入口和保存流程。上次选择按项目保存在 `EditorPrefs` 中，重命名武器后仍可通过 GUID 找回。

## 用户需要在 Unity 里做的事

- 打开 `MMF → 武器工坊`，拖到 Game 旁边。选中 `Tongs`，调整参数，再试用「试玩预览」「打开技能画板」和「在 Project 里选中」。
- 用副本验证列表自动增删和重开后保留选择，并在 Test Runner 运行全部 93 个测试。

## 已知问题 / 未完成

- Computer Use 未获准访问当前 Unity 窗口，界面交互和 Test Runner 验收仍待完成。