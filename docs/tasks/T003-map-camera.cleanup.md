# T003 清理补丁 — 删掉用不到的代码

> 给 Codex：先读 `AGENTS.md`（尤其是新加的 "No dead code" 一节）。这是纯清理，**不改任何行为**。

## 背景
用户新定了规则：用不到的代码要删掉。审查 T001–T003 的代码后，发现下面这些没人用。

## 要做的事
1. `Assets/_MMF/Scripts/Truck/TruckArea.cs`
   - 删掉 `OccupancyChanged` 事件（没人订阅）。
   - 删掉 `IsPlayerInside` 属性（没人读；`FollowCamera` 直接调用 `Contains()`）。
   - 删掉只为上面两项服务的 `LateUpdate` 和 `_player` 字段；`Start` 里只保留 `RefreshLayout()`。
   - 删掉因此不再需要的 `using System;`，更新类的 XML 注释。
2. `Assets/_MMF/Editor/PrototypeSceneBuilder.cs`
   - 不再给 `TruckArea` 设置 `_player`（`CreateTruck` 的 `player` 参数如果因此没用了，也删掉）；引用检查里如果有 `_player` 相关项，一并删掉。
3. `Assets/_MMF/Scripts/Player/PlayerInputReader.cs`
   - 删掉公开属性 `MoveInput`（外面没人读），改成直接把计算结果传给 `_movement.SetMoveDirection(...)`，取消时传 `Vector2.zero`。
4. 运行菜单 `MMF/Build Prototype Scene` 重新生成场景，如果你无法运行 Unity，就在回报里说明。
5. 顺手检查 `Assets/_MMF/` 下还有没有别的没人用的代码（方法、字段、`using`、空文件夹）。**只限代码**；有就删掉并在回报里列出。

## 验收标准
- [ ] 能编译，无报错无警告
- [ ] 行为和 T003 完全一样：镜头进出餐车拉近拉远照常、墙和门照常
- [ ] 上面列的成员都删掉了，没有留下注释掉的代码
- [ ] 生成的场景里所有引用都不为空

## 范围限制
- 只改 `Assets/_MMF/` 下的代码和生成的场景。
- 不要删任何美术、音频、场景资源（包括 `Assets/Welcome`、`Assets/Scenes`）。
- 不改任何数值、规则或行为。
