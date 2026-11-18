# T012b — 出车后镜头固定在场地正中间（可开关）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书。这是用户的**试验**，GDD 里暂时不写，以本说明书为准。
> T012a 已完成并提交：场地 32 × 18，大小在 `MapTuning`，`MapBounds.WorldRect` 是场地范围。
> 这是**小任务**：只改车外镜头。

## 目标
- `CameraTuning` 加一个开关 `_fixedOutside`（中文名"车外镜头固定"，默认 **true**）。
- 开关**开**：玩家在车外时，镜头位置 = **场地正中间**（`MapBounds.WorldRect.center`，再照现在的 `ApplyBounds` 限制，不露出地图外面），不跟玩家动。
- 开关**关**：和现在完全一样（车外跟着玩家，跟随延迟有效）。
- 进出餐车的过渡照旧：出车时镜头一边拉远一边**平滑滑到**场地正中间（时间 = 现在的"拉近拉远时间"）；进车时一边拉近一边滑到车上。车里的镜头不变。
- 车外镜头大小默认值改成 **9** 🟡（`CameraTuning._outsideSize` 的默认值），并把已有的 `Assets/_MMF/Data/Tuning/CameraTuning.asset` 里的 `_outsideSize` 从 7 改成 9（这是用户的设计决定，只改这一个值，别的值不动）。
- Play 中切换开关：下一帧起按新的方式（可以直接跳，也可以平滑，你选简单可靠的，回报写明）。

## 要做的事
1. `CameraTuning`：加 `_fixedOutside` 字段和属性；`_outsideSize` 默认值 7 → 9。
2. `FollowCamera`：车外时按开关选"场地正中间"或"跟着玩家"。
3. `CameraTuning.asset`：`_outsideSize: 9`，加上 `_fixedOutside: 1`。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；93 个测试全部通过
- [ ] Play：出车后镜头平滑拉远、滑到场地中间，然后不动；整个场地一屏看到（16:9 下）；画面不露出地图外面
- [ ] 进车时镜头照旧拉近、固定在车上；背包栏不挡车
- [ ] 数值窗口"镜头"组多了"车外镜头固定"；关掉以后车外镜头又跟着玩家
- [ ] 车外镜头大小显示 9

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/CameraTuning.cs` | 修改 | 加开关、改 `_outsideSize` 默认值 |
| `Assets/_MMF/Scripts/Core/FollowCamera.cs` | 修改 | 只改车外的镜头位置 |
| `Assets/_MMF/Data/Tuning/CameraTuning.asset` | 修改 | 只改 `_outsideSize`、加 `_fixedOutside` |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不要删跟随玩家的代码（开关关掉时要用）
- `LateUpdate` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，走出餐车：镜头拉远到整个场地，然后不动。
2. `MMF → 数值` → "镜头" → 关掉"车外镜头固定"：镜头又跟着你走。
