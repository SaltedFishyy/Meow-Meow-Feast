## 完成情况

- ❌ `MMF.Editor` 已用 Unity 6 自带编译器独立编译，零报错、零警告。全项目编译和 93 个测试待 Unity 验证。
- ❌ 已接入每 300 毫秒检查并保存武器的逻辑。夹子冷却回车后，`Tongs.asset` 是否在 1 秒内更新且清除 dirty 状态，待实测。
- ❌ 自动保存会检查列表中所有武器，技能画板和 Play 模式下的保存效果待实测。
- ✅ 仅对列表中仍位于武器目录且标记为 dirty 的资产调用 `SaveAssetIfDirty`。
- ✅ 工具栏已添加不可点击的「改了自动保存」提示，EC 检查器保留原有界面。
- ✅ 本次只改了 `WeaponWorkshopWindow.cs`，保留 LF、无 BOM，`git diff --check` 通过。

## 改动文件

- `Assets/_MMF/Editor/WeaponWorkshop/WeaponWorkshopWindow.cs` — 加入定时保存、切换武器及关闭窗口时的即时保存检查，以及工具栏提示。

## 疑问

无。

## 决定 / 假设

- 使用 `EditorApplication.update` 和编辑器时钟控制 300 毫秒检查间隔，因此切到技能画板后也能继续检查。关闭窗口时注销回调。

## 用户需要在 Unity 里做的事

- 运行 Test Runner，确认 93 个测试通过。
- 打开 `MMF → 武器工坊`，修改夹子冷却并按回车，确认 `Tongs.asset` 在 1 秒内更新且清除 dirty 状态。保持工坊打开，在技能画板和 Play 模式下各复测一次。

## 已知问题 / 未完成

- Unity 内的完整测试和保存行为实测尚未完成。