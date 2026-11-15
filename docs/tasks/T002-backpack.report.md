## 完成情况
- ✅ 静态编译通过，零警告；包含新增 EditMode 测试代码。
- ❌ 未实测连续两次生成；已实现幂等创建和引用为空时的报错检查。
- ✅ 实现下方居中背包栏，默认 5 格、第 1 格高亮。
- ✅ 实现 0.5 单位自动拾取，同类优先叠放，每格最多 3 个。
- ✅ 满包时物品留在地上。
- ✅ 实现数字键选格、滚轮按方向移动一格，两端不循环。
- ✅ 实现头顶选中物品色块，空格隐藏。
- ✅ Q 丢出一个物品，冷却 2 秒。
- ✅ 掉落物 60 秒消失，丢弃时重新计时。
- ❌ 中文采用 LegacyRuntime 字体并检查字符支持，实际显示尚未验证。
- ✅ 所有暂定参数均可在 Inspector 调整。
- ✅ 背包规则为纯 C#；背包组件不引用 Input System。

## 改动文件
- `Assets/_MMF/Scripts/Data/ItemDefinition.cs` — 物品定义。
- `Assets/_MMF/Scripts/Player/BackpackModel.cs` — 背包规则。
- `Assets/_MMF/Scripts/Player/PlayerBackpack.cs` — 拾取、丢弃、事件和头顶图标。
- `Assets/_MMF/Scripts/Player/WorldItem.cs` — 掉落物与计时。
- `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` — 背包输入转发。
- `Assets/_MMF/Scripts/UI/BackpackBar.cs` — 背包栏。
- `Assets/_MMF/Input/MMFControls.inputactions` — 新增指定动作。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 生成物品资源、8 个掉落物、UI 并校验引用。
- 两个现有 asmdef — 增加 uGUI 引用。
- `Assets/_MMF/Tests/EditMode/` — 四个规则测试及测试程序集。

## 疑问（需要确认，没有就写“无”）
无。

## 决定 / 假设（仅限技术层面）
- 模型使用物品 ID，不依赖 Unity 类型。
- 掉落物采用注册表和距离检测，避免逐帧查找场景。
- UI 无点击交互，因此不需要 EventSystem。

## 用户需要在 Unity 里做的事
1. 等待编译，连续运行两次 `MMF → Build Prototype Scene`。菜单会创建两个物品资源并保存场景。
2. Play 后检查中文、拾取、选格、头顶色块和 Q 丢弃。
3. 站在丢弃物上等待 2 秒；另留物品等待 60 秒检查消失。

## 已知问题 / 未完成
- 电脑控制连接不可用，未完成 Unity 菜单及 Play 实测，不能确认控制台运行时无报错。
- 四个 NUnit 测试方法已在 Unity 外执行通过；尚未通过 Unity Test Runner 运行。