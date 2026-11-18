# T013 — "武器工坊"窗口：调队友技能工坊做的武器

> 给 Codex：先读 `AGENTS.md`，再读本说明书。
> 背景：组员 EC 做了技能工具 Ability Studio（本地包 `LocalPackages/com.emilycai.ability-studio/`，0.3.2-2d.1），武器是它的 `AbilityStudio.Model.SkillDefinition` 资产，例如 `Assets/_MMF/Data/Weapons/Tongs.asset`（夹子）。用户要在 `MMF` 菜单里加一个单独的窗口"武器工坊"，专门调这些武器的数值。
> **不要改 EC 的包**（`LocalPackages/` 下任何文件都不许动）。也不改我们的数值窗口 `TuningWindow.cs`。平底锅（`WeaponDefinition`）留在数值窗口"武器"组，不进武器工坊。

## 目标
- 菜单 **`MMF/武器工坊`** 打开一个 `EditorWindow`，标题"武器工坊"。可以停靠在 Game 画面旁边，Play 中也能用。
- **左边：武器列表**——自动找出 `Assets/_MMF/Data/Weapons/` 文件夹（含子文件夹）里所有 `SkillDefinition` 资产，按文件路径排序，显示名字。`Assets/_Sandbox/` 等其他地方的不列。项目里增删武器资产后列表自动更新（例如 `OnProjectChange`）。没有武器时显示一句提示。
- **右边：选中武器的参数**——直接用 EC 工具自带的检查器界面显示（它的 `SkillDefinitionInspector` 是 `[CustomEditor(typeof(SkillDefinition))]`，用 UI Toolkit 写的；例如 `new InspectorElement(asset)` 或 `Editor.CreateEditor(asset).CreateInspectorGUI()`，你选可靠的做法，回报写明）。这样它以后加的新节点、新参数会自动出现，我们不用改。它界面里已经有"打开技能画板"按钮和保存说明，照它的来。
- **顶部按钮**（对选中的武器）：
  - "试玩预览" → 调 `AbilityStudio.Editor.PlaygroundWindow.OpenSkill(skill)`。
  - "在 Project 里选中" → 选中并高亮这个资产。
- **不放"恢复默认值"按钮**（这些武器共用一种类型，恢复默认会把武器清空）。
- 窗口记住上次选中的武器（关掉再开还是那把；找不到就选第一把）。
- 列表很长时左右两边都能滚动。

## 结构要求
- 新建独立的编辑器程序集，放在 `Assets/_MMF/Editor/WeaponWorkshop/`：
  - `MMF.Editor.WeaponWorkshop.asmdef`：只用于 Editor 平台，引用 `AbilityStudio.Model`、`AbilityStudio.Editor`（EC 的包 `autoReferenced` 是 false，必须显式引用）。命名空间 `MMF.Editor.WeaponWorkshop`。
  - 这样以后 EC 的包改名或拿掉，只坏这一个文件夹，不影响我们其他编辑器代码。**不要改** `Assets/_MMF/Editor/MMF.Editor.asmdef`。
- 一个脚本就够：`WeaponWorkshopWindow.cs`（列表、右侧检查器、按钮）。如果你觉得列表查找应该单独一个小类，最多 2 个脚本，回报说明。
- 文件夹路径 `Assets/_MMF/Data/Weapons` 写成常量。

## 验收标准
- [ ] 编译无报错无警告；93 个测试全部通过（场景不用重新生成，这次不碰场景）
- [ ] `MMF → 武器工坊` 打开窗口；左边列出 `Tongs`；不列 `_Sandbox` 里的钩爪；不列平底锅
- [ ] 选中 `Tongs`，右边显示和 Inspector 里一样的参数（冷却、伤害、投射物速度 / 射程 / 大小、拉拽停止距离 / 时间、镜头震动……）；改了数值，资产里的值跟着变
- [ ] "打开技能画板"（EC 界面里的）、"试玩预览"、"在 Project 里选中"都能用
- [ ] 在 `Assets/_MMF/Data/Weapons/` 复制一份 `Tongs`，列表自动多一项；删掉自动少一项
- [ ] 关掉窗口再打开，还是上次选中的武器
- [ ] `LocalPackages/`、`TuningWindow.cs`、`MMF.Editor.asmdef` 都没有改动

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Editor/WeaponWorkshop/MMF.Editor.WeaponWorkshop.asmdef` | 新增 | 独立编辑器程序集 |
| `Assets/_MMF/Editor/WeaponWorkshop/WeaponWorkshopWindow.cs` | 新增 | 武器工坊窗口 |
| （可选）`Assets/_MMF/Editor/WeaponWorkshop/` 下第 2 个脚本 | 新增 | 只在需要时 |

## 范围限制
- 只动上面清单里的文件；新文件用 LF、不加 BOM
- 不要做：把武器接进游戏（装备、左键发射）、改 EC 的包、改数值窗口、给武器加我们自己的数值类型
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. 菜单 `MMF → 武器工坊`，把窗口拖到 Game 旁边。
2. 左边点"Tongs"，右边改数值；点"试玩预览"在 EC 的试玩场景里试；点"打开技能画板"改节点。
