## 完成情况

- ❌ 编译通过，0 错误、0 警告；93 个测试未能运行。
- ✅ 已实现嵌套字段中文标签，几率仍由 Unity 绘制 0～1 滑条；待窗口实测。
- ✅ 已保留个数、加删、排序、折叠及原有保存流程；待交互实测。
- ✅ 其他 6 组普通字段保持原绘制方式，`labelWidth` 保留。

## 改动文件

- `Assets/_MMF/Editor/TuningWindow.cs` — 使用 `ReorderableList` 配合递归绘制；保留原编码、换行符及 BOM 状态。未修改其他文件。

## 疑问（需要确认，没有就写"无"）

无。

## 决定 / 假设（仅限技术层面）

- 叶子字段继续使用 `PropertyField`，保留 `Range`、`Min`；未添加 `PropertyDrawer`。
- 缓存列表及序列化对象，窗口关闭时释放。

## 用户需要在 Unity 里做的事

- 运行 93 个 EditMode 测试。
- 打开 `MMF → 数值`，检查中文标签、滑条、列表操作、即时保存和撤销。

## 已知问题 / 未完成

Unity CLI 无法连接编辑器，未完成运行测试及窗口交互验收。

自动审批拒绝了 computer-use 访问 Unity，返回原因：“Computer Use was not approved to use Unity”。