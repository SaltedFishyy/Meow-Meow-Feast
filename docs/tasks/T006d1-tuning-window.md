# T006d1 — 数值窗口 `MMF → 数值`（先接玩家 / 背包 / 煎锅 / 镜头）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §6.4。T006a～c 已完成并提交：`PlayerTuning`、`BackpackTuning`、`CookingTuning`、`CameraTuning` 四个数值文件在 `Assets/_MMF/Data/Tuning/`。
> 这是**小任务**，只做数值窗口，并接上这 4 个数值文件。怪物、武器、掉落表是下一个小任务 T006d2。

## 目标
Unity 编辑器菜单 `MMF → 数值` 打开一个窗口，把所有数值文件按系统分组列出来，用中文名显示。改了马上永久保存；Play 中改也马上生效。每个数值文件有"恢复默认值"按钮。**以后新加的数值文件 / 数值，不用改窗口代码就会自动出现。**

## 要做的事
1. **标记属性**（运行时程序集，新文件 `Scripts/Data/TuningAttributes.cs`）：
   - `TuningGroupAttribute(string group, int order)`：标在数值文件的**类**上，表示这个类的资源要出现在窗口里，属于哪一组、排第几。
   - `TuningLabelAttribute(string label)`：标在**字段**上，窗口里显示的中文名。没有标的字段也要显示（用 Unity 默认的名字），保证新加的数值一定会出现。
2. **给 4 个数值文件加标记和中文名**（只加属性，字段、默认值、逻辑都不改）：
   | 类 | 分组（顺序） | 字段中文名 |
   |---|---|---|
   | `PlayerTuning` | 玩家（1） | 最大速度；加速时间（秒）；减速时间（秒）；挤怪物时的速度比例 |
   | `BackpackTuning` | 背包（2） | 背包格数（下次 Play 生效）；每格最多叠几个（下次 Play 生效）；自动捡起距离；丢掉后几秒才能捡回；掉落物几秒后消失；头上"x"显示几秒 |
   | `CookingTuning` | 煎锅（3） | 互动距离；一次放几个；每轮几个字母（下次开始煎时生效）；每轮几秒（下次开始煎时生效）；翻面动画几秒 |
   | `CameraTuning` | 镜头（4） | 车里镜头大小；车外镜头大小；拉近拉远时间（秒）；跟随延迟（秒） |
   `CookingTuning` 现在的英文 Tooltip 删掉（意思已经写进中文名里）；`BackpackTuning` 的"下次 Play 生效" Tooltip 也可以删掉，同样已经写进中文名。
3. **窗口**（编辑器程序集，新文件 `Editor/TuningWindow.cs`，`EditorWindow`）：
   - 菜单 `MMF/数值`，窗口标题"数值"。
   - 用 `TypeCache.GetTypesWithAttribute<TuningGroupAttribute>()` 找到所有标了的类，再用 `AssetDatabase.FindAssets("t:类名")` 找到它们的资源。按分组顺序排列，每组一个可折叠的标题；组里每个资源一个小标题（资源名）。
   - 每个资源用 `SerializedObject` + `EditorGUILayout.PropertyField` 画出所有可见的序列化字段（跳过 `m_Script`），标签用 `TuningLabel`（没有就用默认名），`Tooltip` 照常显示。列表 / 嵌套字段也要能编辑。
   - **改了就保存**：值一变就 `ApplyModifiedProperties`、`EditorUtility.SetDirty`、`AssetDatabase.SaveAssetIfDirty`。支持撤销（Ctrl+Z）更好。
   - **恢复默认值**：每个资源一个按钮，先弹确认框。把**数字、布尔、向量、颜色、枚举**这类简单字段恢复成代码里的默认值（用 `ScriptableObject.CreateInstance` 建一个临时实例读默认值，读完销毁）；**物体引用和列表不动**（它们在代码里没有默认值）。恢复后同样马上保存。
   - 窗口顶部一行说明："改了马上保存。Play 中改马上生效；标着'下次 Play 生效'的要重新 Play。"
   - 有新资源被创建 / 删除时，窗口刷新列表（`OnProjectChange`）。
   - 窗口内容可以滚动。
4. **不改**任何组件、生成器、场景。

## 验收标准
- [ ] 编译无报错无警告；原有 93 个测试全部通过
- [ ] 菜单 `MMF → 数值` 能打开窗口，按"玩家、背包、煎锅、镜头"的顺序分 4 组，字段都是中文名
- [ ] 不在 Play 时改"最大速度"为 6：`PlayerTuning.asset` 文件里马上变成 6（已保存）
- [ ] Play 中在窗口里改"车外镜头大小"为 9：出车后镜头马上变成 9；退出 Play 后值还是 9（永久保存）
- [ ] 点"恢复默认值"并确认：数值回到默认（例如最大速度 5、车外镜头 7）
- [ ] 给任意一个数值文件临时加一个新字段（审查时 Claude 会试）：不改窗口代码，它就出现在窗口里

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/TuningAttributes.cs` | 新增 | 两个标记属性 |
| `Assets/_MMF/Editor/TuningWindow.cs` | 新增 | 数值窗口 |
| `Assets/_MMF/Scripts/Data/PlayerTuning.cs` | 修改 | 只加标记属性和中文名 |
| `Assets/_MMF/Scripts/Data/BackpackTuning.cs` | 修改 | 只加标记属性和中文名（可删 Tooltip） |
| `Assets/_MMF/Scripts/Data/CookingTuning.cs` | 修改 | 只加标记属性和中文名，删英文 Tooltip |
| `Assets/_MMF/Scripts/Data/CameraTuning.cs` | 修改 | 只加标记属性和中文名 |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM）
- 不要做：怪物 / 武器 / 掉落表进窗口（T006d2）；Designer 场景（T006e）；游戏画面里的面板
- 不改任何数值的默认值、不改游戏逻辑
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. 菜单 `MMF → 数值`，打开数值窗口（可以拖到 Game 画面旁边）。
2. 按 Play，在窗口里改"最大速度"：角色马上变快 / 变慢。
3. 退出 Play，值还在。想回到原来的值，点那一组的"恢复默认值"。
