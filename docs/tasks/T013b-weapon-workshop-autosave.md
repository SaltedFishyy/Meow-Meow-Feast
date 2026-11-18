# T013b — 武器工坊：改了就自动保存

> 给 Codex：先读 `AGENTS.md`，再读本说明书。
> T013 已完成并提交：`Assets/_MMF/Editor/WeaponWorkshop/WeaponWorkshopWindow.cs`（`MMF → 武器工坊`），右边嵌入 EC 工具的检查器。EC 的检查器改了数值只改内存（资产变 dirty），要点它的"保存技能"（`SkillAssetCommands.Save` = `SetDirty` + `AssetDatabase.SaveAssetIfDirty`）才存盘。
> 这是**小任务**：用户要武器工坊和我们的数值窗口一样，**改了就自动存盘**。

## 目标
- 武器工坊窗口打开时，列表里的武器只要有未保存的改动（`EditorUtility.IsDirty`），**很快自动存盘**（`AssetDatabase.SaveAssetIfDirty`）。做法建议：用 `rootVisualElement.schedule.Execute(...).Every(<毫秒>)` 定时检查列表里的武器（数量很少），间隔用常量，例如 300 毫秒。这样不管是在武器工坊里改，还是在 EC 的技能画板里改（窗口开着时），都会存。
- 另外在这些时候也立刻检查一次：切换选中的武器时、窗口关闭（`OnDisable`）时。
- 只存列表里（`Assets/_MMF/Data/Weapons/`）的武器，别的资产不碰。
- Play 中改了也照样存盘。
- 在顶部工具栏加一个不能点的小字"改了自动保存"，让用户知道不用点"保存技能"。EC 检查器里的"保存技能"按钮和提示保持原样（那是 EC 的界面，不改）。
- 正在输入数字、还没按回车 / 离开输入框时不需要存（EC 的输入框本来就是确认后才改资产）。

## 验收标准
- [ ] 编译无报错无警告；93 个测试全部通过
- [ ] 在武器工坊改夹子的冷却，回车后不到 1 秒，`Tongs.asset` 文件里的值就变了，资产不再是 dirty
- [ ] 窗口开着时在技能画板里改参数，也会自动存盘
- [ ] 不改任何东西时不会反复写文件（只在 dirty 时存）
- [ ] 工具栏有"改了自动保存"字样
- [ ] `LocalPackages/`、`TuningWindow.cs`、`MMF.Editor.asmdef` 没有改动

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Editor/WeaponWorkshop/WeaponWorkshopWindow.cs` | 修改 | 只加自动保存和工具栏小字 |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符（LF）、不加 BOM）
- 定时检查里不要分配大量垃圾、不要 `FindAssets`
- 有不清楚的地方写进回报的"疑问"，不要自己决定
