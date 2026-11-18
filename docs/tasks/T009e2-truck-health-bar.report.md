## 完成情况

- ❌ Unity 编译、连续生成场景 3 次、引用检查和 93 个测试尚未验证。`git diff --check` 已通过。
- ❌ 开局满条和 `100/100` 已实现，尚未在 Play 中验证。
- ❌ 受击时按当前血量比例缩短填充并更新数字。归零时显示 `0/100`、填充宽度为零，尚未实测。
- ❌ 血条锚在 Canvas 顶部正中，默认放在左上角玩家血量方块的下沿以下。尚未实测不同宽高比。
- ✅ 通过代码检查确认，文字在首次显示、重新启用和收到 `TruckHealth.Changed` 时刷新，没有 `Update` 轮询。

## 改动文件

- [TruckHealthBar.cs](<C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Scripts/UI/TruckHealthBar.cs>) — 新增餐车血量显示和序列化外观字段。
- [PrototypeSceneBuilder.cs](<C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Editor/PrototypeSceneBuilder.cs>) — 在现有 Canvas 中生成餐车血条并绑定 `TruckHealth`。

## 疑问（需要确认，没有就写“无”）

- 无。

## 决定 / 假设（仅限技术层面）

- 使用深色底、绿色填充和居中白字。填充宽度由 `RectTransform` 的右侧锚点控制。
- 首次读取血量安排在 `Start`，确保餐车已在 `Awake` 初始化血量。

## 用户需要在 Unity 里做的事

1. 运行 `MMF → Build Prototype Scene` 三次，确认 Console 没有编译错误、警告或缺失引用，再运行现有 93 个测试。
2. 按 Play，确认顶部正中显示满条和 `100/100`。待在车里让怪物攻击餐车，观察数字和条长同步下降，归零时显示 `0/100`。
3. 切换 Game 视图的宽高比，确认血条保持居中且不遮挡玩家血量方块。

## 已知问题 / 未完成

- computer-use 自动权限检查拒绝了对 Unity 的访问。`Prototype.unity` 尚未重新生成，编辑器和试玩验收仍待完成。