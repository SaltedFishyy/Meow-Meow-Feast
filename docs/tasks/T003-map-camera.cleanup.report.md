## 完成情况
- ❌ 编译无报错无警告：未能运行 Unity 编译；静态检查及 `git diff --check` 通过。
- ❌ 行为与 T003 相同：镜头判定、墙、门及数值未改，尚未进行 Play 验证。
- ✅ 指定成员全部删除，没有留下被注释的代码。
- ❌ 生成场景引用非空：未能重新生成场景，原有引用检查保留。
- ✅ 已检查其他代码和空文件夹，未发现其他确定可删项。

## 改动文件
- `Assets/_MMF/Scripts/Truck/TruckArea.cs` — 删除事件、属性、`_player`、`LateUpdate` 和 `using System;`；精简 `Start`，更新注释。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 删除餐车 `_player` 赋值及 `CreateTruck` 的无用参数。
- `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` — 删除 `MoveInput`，直接传递移动方向，取消时传零。

## 疑问（需要确认，没有就写"无"）
无。

## 决定 / 假设（仅限技术层面）
- 不手改场景 YAML，保留生成器的引用验证逻辑。

## 用户需要在 Unity 里做的事
- 运行 `MMF/Build Prototype Scene`。
- 确认 Console 无错误和警告，再按 Play 验证镜头缩放、墙壁碰撞和车门通行。

## 已知问题 / 未完成
- 窗口控制连接失败：找不到 native pipe，无法操作 Unity。
- 场景未重新生成，仍保留旧 `_player` 序列化记录。