## 完成情况

- ❌ 编译与全部测试：离线编译无报错、无警告；目标与避让相关 49 个测试全部通过，Unity 全套测试未验证。
- ❌ 15 秒内错开并到达：两处修复已完成，实际双怪物理场景尚未验证。
- ❌ 其他行为不变：原有目标与避让测试通过，Play 回归尚未验证。

## 改动文件

均位于 `Assets/_MMF/`：

- `Scripts/Combat/MonsterTargeting.cs` — 安全区内、车外起点投影到最近边界后计算绕角路线。
- `Scripts/Combat/MonsterAvoidance.cs` — 左侧回退与对方横移同向时等待。
- `Scripts/Combat/Monster.cs` — 提供只读避让方向；接入等待，并处理两怪更新先后顺序。
- `Tests/EditMode/MonsterTargetingTests.cs` — 增加安全区内起点及路线到达测试。
- `Tests/EditMode/MonsterAvoidanceTests.cs` — 增加同向等待、对方静止和更新顺序测试。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

- 等待沿用现有避让时长，不新增参数。
- 保留原算法对等长路线的选择顺序。

## 用户需要在 Unity 里做的事

- 运行全部 EditMode 测试。
- Play 复现指定 A/B 场景，检查是否在 15 秒内分别到达车头、车尾，并回归追逐、绕角和碰撞阻挡。

## 已知问题 / 未完成

Unity 自动审批拒绝了窗口访问，原因为 “Computer Use was not approved to use Unity”，因此未完成编辑器内编译、全套测试及物理实测。