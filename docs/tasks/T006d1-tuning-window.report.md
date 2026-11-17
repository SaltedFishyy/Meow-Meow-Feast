## 完成情况
- ❌ 完整编译与测试验收未完成：使用 Unity 现有编译参数检查两个程序集，无编译错误或警告；未运行原有 93 个测试。
- ✅ 已实现菜单、四组排序、中文标签和滚动窗口；待 Unity 界面确认。
- ✅ 已实现修改后立即保存；“最大速度改为 6”的文件验证待实测。
- ❌ Play 中镜头实时变化及退出后保留尚未实测。
- ✅ 已实现确认后恢复代码默认值，保留列表和物体引用；待点击验证。
- ✅ 自动遍历可见序列化字段，新字段无需修改窗口代码。

## 改动文件
- `Assets/_MMF/Scripts/Data/TuningAttributes.cs` — 新增两个标记属性。
- `Assets/_MMF/Editor/TuningWindow.cs` — 新增数值窗口、自动发现、保存、撤销和恢复默认值。
- `Assets/_MMF/Scripts/Data/PlayerTuning.cs` — 添加分组和中文名。
- `Assets/_MMF/Scripts/Data/BackpackTuning.cs` — 添加分组和中文名，删除重复 Tooltip。
- `Assets/_MMF/Scripts/Data/CookingTuning.cs` — 添加分组和中文名，删除英文 Tooltip。
- `Assets/_MMF/Scripts/Data/CameraTuning.cs` — 添加分组和中文名。

## 疑问（需要确认，没有就写"无"）
无。

## 决定 / 假设（仅限技术层面）
- 使用 Unity 序列化接口编辑和恢复字段，支持撤销；列表和物体引用不参与恢复。
- 保留原文件编码、BOM 和换行方式；未改默认值及游戏逻辑。

## 用户需要在 Unity 里做的事
- 打开 `MMF → 数值`，验证修改、Play 实时生效、退出后保留及恢复默认值。
- 在 Test Runner 中运行原有 93 个测试。

## 已知问题 / 未完成
- Unity 界面、Play 行为及测试尚未验证；未启动 batchmode。