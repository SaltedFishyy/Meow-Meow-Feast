## 完成情况

- ❌ 完整验证未完成：离线编译无诊断；新增 8 个及原有目标选择 25 个测试全部通过，尚未跑完全部测试及三次场景生成。
- ✅ 已实现右侧优先、左侧回退、两侧阻塞则停；迎面错开效果待试玩确认。
- ✅ 横移使用 Cast 限制距离，保留怪物碰撞阻挡。
- ✅ 保留沿墙滑动、绕角及目标选择规则；补充沿墙滑动遇怪时的避让。
- ✅ 新增 `Avoidance Duration` 参数，默认 0.5 秒，可在怪物数据 Inspector 调整。

## 改动文件

均位于 `Assets/_MMF/`：

- `Scripts/Combat/MonsterAvoidance.cs` — 新增纯方向判断。
- `Scripts/Combat/Monster.cs` — 接入怪物阻挡识别及等速横移。
- `Scripts/Data/MonsterDefinition.cs` — 新增避让时长。
- `Tests/EditMode/MonsterAvoidanceTests.cs` — 新增 8 个测试用例。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

- 一次避让期间保持选定方向；前方清空后恢复原移动。
- 仅在实际阻挡时调用 `TryGetComponent`，复用 Cast 缓冲区。

## 用户需要在 Unity 里做的事

- 执行 `MMF → Build Prototype Scene` 三次，检查引用。
- 运行全部 EditMode 测试。
- Play 后待在车内，观察上下两侧怪物迎面避让及沿墙移动。

## 已知问题 / 未完成

- 场景和数据资源未通过生成器更新；Unity 物理效果尚未实测。

Unity 界面操作被工具拒绝，原因为“Computer Use was not approved to use Unity”，因此无法完成编辑器内验证。