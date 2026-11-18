# T009d1 — 血归零倒地 3 秒，然后在车正中间复活（规则部分）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §5.3.2（"倒地细节"）。
> T009a～c 已完成并提交：`PlayerHealth`（`Current`、`CanBeHit`、`IsInvincible`、`StartInvincibility()`、`Hit` / `Changed` / `InvincibilityStarted` 事件）、`PlayerHitReaction`（弹开、闪烁、穿怪）、`MonsterAttack`、左上角血量方块。
> 这是**小任务**：只做倒地和复活的**规则**。**不做**：倒地的样子（变灰、压扁、倒计时 3/2/1）——那是下一个小任务 T009d2；餐车血量（T009e）。

## 目标
- **倒地**：血掉到 0 的那一刻玩家倒地，持续"倒地几秒"（默认 **3** 🟡）：
  - 不能动（立刻停下）、不能做任何操作（移动、E、Q、左键、右键、选格、滚轮都没反应）。
  - 正在蓄力丢东西 → 取消蓄力，东西留在背包里。
  - 正在煎东西 → 按现有的取消逻辑退出（车里怪物打不到，一般不会发生，但要安全）。
  - **举着锅台** → 先试着把锅台放在**玩家倒地位置**所在的格子（用现有的放置规则：对齐格子、要在车里、不能和东西重叠）；放不下就**放回这次举起前的位置**。
  - 怪物**当作玩家不在**：和玩家在车里时一样，不追玩家、去车头 / 车尾（`MonsterTargeting`）；也不会打倒地的玩家（`CanBeHit` 已经是 false）。
  - 倒地时被弹开（T009c）照常弹完；倒地期间不再闪烁、不穿怪也可以（你选清楚的一种，回报写明）。
- **复活**：倒地时间到：
  - 玩家瞬间移到**车正中间**（`TruckArea` 的中心），速度清零。
  - **血回满**（用当时的"最多几格血"），左上角方块跟着变满。
  - **背包完全不变**（不掉东西）。
  - 开始和被打后**一样的无敌**（`StartInvincibility()`：一闪一闪、能穿怪）。
  - 恢复所有操作。

## 要做的事
1. **`PlayerHealthTuning`**：加 `_downSeconds`（中文 `倒地几秒`，默认 3，`Min(0)`）。
2. **`PlayerHealth`**：加"倒地"状态（例如 `IsDown`）、倒地 / 复活事件、`Revive()`（回满血、发 `Changed`、开始无敌）。计时放这里还是新脚本你选，回报写明。
3. **新脚本 `Scripts/Player/PlayerDowned.cs`**：只管倒地时停掉操作（停移动、关输入、取消蓄力 / 煎、处理锅台）和时间到以后复活（移到车中间、调 `Revive()`、恢复操作）。
4. **`PlayerInteractor`**：只加一个公开方法，例如 `DropCarriedStation()`：按上面的规则放下锅台（先试倒地位置的格子，放不下回到举起前的位置）；为此在 `Lift()` 举起时记下原来的位置。别的不改。
5. **`MonsterTargeting`**：玩家倒地时当作玩家不在（和在车里一样）。只改这一个判断。
6. **生成器**：给玩家加 `PlayerDowned`、填引用；怪物需要新引用就填上。已有的 `PlayerHealthTuning.asset` 不覆盖。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；93 个测试全部通过
- [ ] 车外被打到 0：玩家停住，按任何键都没反应；附近的怪物不再追 / 不再打，转去车头车尾
- [ ] 3 秒后：玩家出现在车正中间，左上角 5 格全满，背包和倒地前一模一样，一闪一闪 1 秒能穿怪，然后能正常操作
- [ ] 蓄力时被打到 0：箭头消失，东西还在背包里
- [ ] 车外举着锅台被打到 0：锅台回到举起前的位置（不会留在车外、不会跟着玩家飞到车中间、不会和东西重叠）
- [ ] 复活后再出车：怪物又会追你、打你
- [ ] 数值窗口"玩家血量"组多了"倒地几秒"；Play 中改了下一次倒地生效
- [ ] 没倒地时所有原来的行为不变

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/PlayerHealthTuning.cs` | 修改 | 只加 1 个字段和属性 |
| `Assets/_MMF/Scripts/Player/PlayerHealth.cs` | 修改 | 倒地状态、事件、`Revive()` |
| `Assets/_MMF/Scripts/Player/PlayerDowned.cs` | 新增 | 倒地停操作、复活 |
| `Assets/_MMF/Scripts/Player/PlayerInteractor.cs` | 修改 | 只加放下锅台的方法和记录举起前位置 |
| `Assets/_MMF/Scripts/Combat/MonsterTargeting.cs` | 修改 | 只加"倒地当作不在" |
| `Assets/_MMF/Scripts/Player/PlayerHitReaction.cs` | 修改（只在需要时） | 只改倒地时的闪烁 / 穿怪处理 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加组件和引用 |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（项目里的 `.cs` 都是 LF，新文件也用 LF）、不加 / 删 BOM）
- 不要做：倒地的样子（变灰、压扁、倒计时）、餐车血量、失败画面
- `Update` / `FixedUpdate` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
（倒地的样子是 T009d2，这一步倒地时角色看起来只是不动。）
1. Play，出车让怪物把你打到 0：你动不了，怪物走开去车头车尾。
2. 3 秒后你出现在车中间，血满，一闪一闪。
