## 完成情况

以下 ✅ 表示代码已实现，运行效果仍待 Unity 验收。

- ❌ 编译无警告、生成 3 次、93 项测试：未完成验证。
- ✅ 右键蓄力及鼠标跟随箭头；默认 1 秒从 1 格增长到 6 格并封顶。
- ✅ 松开右键结束蓄力，不修改背包。
- ✅ 蓄力时左键只取消，不执行攻击或举锅台。
- ✅ 空格、举锅台、煎东西时不能开始蓄力。
- ✅ 不修改移动速度，换格不中断蓄力。
- ✅ 添加“丢东西”数值组及三个中文字段，计算时读取实时数值。
- ✅ 生成器仅在缺失时创建数值资产；重复生成及保值待验证。

## 改动文件

- `Assets/_MMF/Scripts/Data/ThrowTuning.cs` — 新增蓄力数值。
- `Assets/_MMF/Scripts/Player/PlayerThrow.cs` — 新增蓄力与方向规则。
- `Assets/_MMF/Scripts/Player/ThrowArrow.cs` — 新增复用网格的箭头显示。
- `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` — 接入右键、鼠标方向、左键取消。
- `Assets/_MMF/Input/MMFControls.inputactions` — 仅新增 Throw 动作和右键绑定。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 添加组件、引用及数值资产创建。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

- 使用 `performed/canceled` 开始和结束蓄力。
- 箭头使用半透明白色网格，排序为 5；仅几何或外观变化时更新网格。

## 用户需要在 Unity 里做的事

- 等待编译，运行 `MMF/Build Prototype Scene` 三次，并运行原有 93 项测试。
- Play 后检查右键蓄力、松手消失、左键取消和换格。
- 在 `MMF → 数值` 修改蓄满时间，检查即时生效及重新生成后保值。

## 已知问题 / 未完成

- 输入 JSON 和 `git diff --check` 检查通过。
- 命令行构建因缺少 `project.assets.json` 失败，未证明编译通过。
- 未运行生成器，场景和 `ThrowTuning.asset` 尚未生成。
- 检测到外部修改 `PlayerTuning.asset`，未触碰该改动。