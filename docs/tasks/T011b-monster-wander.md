# T011b — 怪物扑完车以后在车附近走动 8～12 秒，再回车边

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §5.8.4（怪物打车的节奏）。
> T011a 已完成并提交：`MonsterTruckRoutine`（`Approaching / Waiting / Ready`，`OnTruckAttackLaunched()` 现在是"重新等"），`MonsterAttack` 每个物理帧推进它。
> 这是**小任务**：把"扑完重新等"改成"扑完去走动，走完再回车边等"。

## 目标
- 怪物**扑车出手**以后（`OnTruckAttackLaunched`）进入**走动**：
  - 走动多久：在"走动最少几秒"和"走动最多几秒"之间随机（默认 **8** 和 **12** 🟡）。扑的那一下（冲出去那一小段）照常做完再开始走。
  - 走去哪里：在**离车外墙 3～6 格**（"走动离车最近几格"默认 **3**、"走动离车最远几格"默认 **6** 🟡）的环形区域里随机选一个点走过去；到了（很近，技术常量）就再选一个点。点要在地图里面（`MapBounds.WorldRect` 往里留出怪物半个身位）。
  - 走的时候用现有的移动方式（速度、绕车、避让、被墙挡都和现在一样，路线用 `MonsterTargeting.NextWaypoint` 绕车）。
  - 走不过去（例如被别的怪物或墙挡住，一段时间没靠近目标，技术常量，例如 2 秒）就换一个点，不要一直卡着。
  - 走动时**经过车边也不打车、不开始等**，也不会停下来（T011a 里"在打车范围但还没 Ready 就贴着车不动"只对 `Approaching / Waiting` 有效）。
- 走动时间到 → 回到 `Approaching`：回自己那一头的车边，照 T011a 先等 8～12 秒再扑。
- **开始追玩家**：走动也立刻结束（和 T011a 一样清回 `Approaching`）。之后不追玩家时，直接去车边（不会接着走动）。
- 怪物死了 / 重生：清回 `Approaching`。
- 打玩家的规则不变。

## 要做的事
1. **`MonsterDefinition`**：加 4 个字段（`TuningLabel` 中文名、`Min(0)`）：
   | 字段 | 中文名 | 默认值 |
   |---|---|---|
   | `_wanderMin` | 走动最少几秒 | 8 |
   | `_wanderMax` | 走动最多几秒 | 12 |
   | `_wanderNear` | 走动离车最近几格 | 3 |
   | `_wanderFar` | 走动离车最远几格 | 6 |
   最多 < 最少、最远 < 最近时按"最少 / 最近"算（不要报错）。
2. **`MonsterTruckRoutine`**：加 `Wandering` 状态、走动计时、当前走动目标点的选法（需要 `TruckArea`、`MapBounds` 引用）。对外提供例如 `IsWandering` 和 `WanderTarget`。
3. **`MonsterTargeting`**：不追玩家且正在走动时，目的地换成走动目标点（仍然经过 `NextWaypoint` 绕车）。别的不改。
4. **`MonsterAttack`**：只改"在打车范围但还没 Ready 就不动"这一条，让它在走动时不生效。
5. **生成器**：填新引用。已有的 `CursedDumpling.asset` 不覆盖。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；93 个测试全部通过
- [ ] 玩家待在车里：怪物到车边等 8～12 秒 → 扑一下 → 离开车边，在车附近（离车 3～6 格）走来走去 8～12 秒 → 回车边再等 → 再扑……
- [ ] 走动时不会走出地图，不会穿车，经过车边不停下、不扑
- [ ] 被挡住时会换方向，不会一直卡在一个地方
- [ ] 玩家出车靠近：不管在等还是在走，都马上追玩家；玩家回车以后，怪物直接回车边重新等
- [ ] 数值窗口"怪物"组多了 4 个走动数值；Play 中改了下一次走动生效

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/MonsterDefinition.cs` | 修改 | 只加 4 个字段和属性 |
| `Assets/_MMF/Scripts/Combat/MonsterTruckRoutine.cs` | 修改 | 加走动状态、计时、选点 |
| `Assets/_MMF/Scripts/Combat/MonsterTargeting.cs` | 修改 | 只加"走动时目的地 = 走动点" |
| `Assets/_MMF/Scripts/Combat/MonsterAttack.cs` | 修改 | 只改走动时不贴车停下 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加引用 |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（项目里的 `.cs` 都是 LF）、不加 / 删 BOM）
- 不要做：新的寻路系统、怪物挤在一起的处理
- `Update` / `FixedUpdate` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，待在车里：怪物到车边停一会儿，扑一下，然后在车附近溜达约 10 秒，再回来停、再扑。
