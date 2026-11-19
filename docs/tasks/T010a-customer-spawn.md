# T010a — 顾客 1：顾客出现（还不能喂）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §5.7（顾客）、§5.8.1（怪物的行为和攻击）。
> 顾客分两步：**a 顾客出现（本任务）** → b 把煎熟的食物扔到顾客身上 = 喂、变亮消失。
> 这次**不做喂食**：扔东西碰到顾客，和碰到普通怪物一样弹开（现在 `ThrownItem` 已经这样处理所有 `Monster`）。

## 目标
- **顾客是一种怪物**：和咒化团子用同一套组件（`Monster`、`MonsterTargeting`、`MonsterAttack`、`MonsterTruckRoutine`、避让等），所以追玩家、去车头车尾、打车、打人、被平底锅打、被玩家挤开都和团子一样。
  - 用**自己的** `MonsterDefinition` 数值文件 `Assets/_MMF/Data/Monsters/Customer.asset`：数值默认和团子一样，**颜色橙色**（例如 `(1, 0.6, 0.15)`）。它会自动出现在数值窗口"怪物"组里（和团子并排）。
  - **不掉东西**：顾客身上不挂 `MonsterDrops`。
  - **头顶一个"饿"的小图标**（占位：一个小的世界空间文字"饿"或小方块，用和背包栏一样能显示中文的字体），跟着顾客走。
  - 加一个标记组件 `Customer`（`Assets/_MMF/Scripts/Combat/Customer.cs`），让后面 T010b 能认出"这是顾客"。这次它只需要存在（可以只放图标的引用），不要提前写喂食逻辑。
- **出生**（新脚本 `CustomerSpawner`）：
  - 场地里 **2 个顾客出生点**，在场地下边：车中心偏移 **(-6, -7.5)** 和 **(6, -7.5)**（生成器里写，位置以后用户再调），两个点**轮流**用。
  - **开局 15 秒**后来第 1 个，之后**每 15 秒**来 1 个；场上已经有"最多几个"时就不来，等有顾客消失后，**再等一个间隔**才来下一个（计时在满员时暂停或重新开始都行，选简单的，回报写明）。
  - 顾客被打死 → 消失，**不重生**（团子的"死后几秒重生"不用在顾客身上）；名额空出来，由出生规则再来新的。
  - 场上最多几个是可调的，数量不能写死在生成器里：生成器可以预先建一个**模板**顾客（不激活），`CustomerSpawner` 需要时 `Instantiate` 或用对象池（只在出生时分配，不在每帧分配）。**注意**：`ThrownItem.Initialize` 会用 `FindObjectsByType<Monster>(FindObjectsInactive.Include)` 收集要碰撞的怪物，新出生的顾客必须也被它找到（例如出生后才创建的顾客，在下一次扔东西时能被找到即可）；模板本身不能出现在场上或被打到。
  - 新数值文件 `Assets/_MMF/Data/Tuning/CustomerTuning.asset`（新类 `CustomerTuning`，`[TuningGroup("顾客", …)]`，序号排在"怪物"后面）：
    | 字段 | 中文名 | 默认 |
    |---|---|---|
    | `_firstDelay` | 开局几秒来第一个 | 15 |
    | `_interval` | 每隔几秒来一个 | 15 |
    | `_maxCount` | 场上最多几个 | 3 |
    生成器只在缺失时用默认值创建，已有的不覆盖。
- 顾客出生时和团子一样处理车门（门挡怪物、不挡玩家）——复用现有的做法，不要复制粘贴一大段；需要的话把生成器里"建一只怪物"的代码抽成一个方法给团子和顾客共用（只在生成器里整理，团子的结果必须和现在一样）。
- 失败画面 / 按 R 重开：和团子一样处理（现在重开是整局重来的话，顾客也一起回到开局状态）。

## 结构
- 新脚本：`Scripts/Combat/Customer.cs`（标记 + 头顶图标）、`Scripts/Combat/CustomerSpawner.cs`（出生规则）、`Scripts/Data/CustomerTuning.cs`（数值）。不超过这 3 个。
- 不改 `Monster` / `MonsterTargeting` / `MonsterAttack` / `MonsterTruckRoutine` 的行为。如果为了"不重生"必须改 `MonsterSpawner` 以外的东西，先写进回报的疑问。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；84 个测试全部通过（`CustomerSpawner` 的计时规则如果抽成纯 C# 类，加测试）
- [ ] Play：开局约 15 秒，场地下边出现第 1 个橙色顾客（头顶"饿"），之后每 15 秒一个，最多 3 个，两个出生点轮流
- [ ] 顾客和团子一样追玩家、去车头车尾、打车、打人
- [ ] 用平底锅打死顾客 → 消失、不掉东西；之后空出的名额会再来新顾客
- [ ] 扔东西碰到顾客会弹开（和团子一样）
- [ ] 数值窗口："怪物"组多了顾客那一份；新"顾客"组有 3 个数值；团子行为、掉落和以前一样

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Combat/Customer.cs` | 新增 | 标记 + 头顶图标 |
| `Assets/_MMF/Scripts/Combat/CustomerSpawner.cs` | 新增 | 出生规则 |
| `Assets/_MMF/Scripts/Data/CustomerTuning.cs` | 新增 | 3 个数值 |
| `Assets/_MMF/Tests/EditMode/` 下新测试 | 新增（可选） | 计时规则 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加顾客的建法、出生点、引用；可把"建一只怪物"抽成共用方法 |
| `Assets/_MMF/Data/Monsters/Customer.asset`、`Assets/_MMF/Data/Tuning/CustomerTuning.asset` | 由生成器创建 | — |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不要做：喂食、吃完变亮消失（T010b）
- `Update` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，等约 15 秒，场地下边出现橙色方块（头顶"饿"）——这是顾客，会和紫色团子一样来打你和餐车。
2. 用平底锅把它打死：消失，不掉东西。之后还会再来。
3. 现在还不能喂（下一步 T010b）。
