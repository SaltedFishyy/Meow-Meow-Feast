# T010b — 顾客 2：把煎熟的食物扔到顾客身上 = 喂，变亮 1 秒后消失

> 给 Codex：先读 `AGENTS.md`，再读本说明书和 `docs/GDD.md` §5.7（顾客，"怎么喂""吃到以后"）。
> T010a 已完成：顾客 = 带 `MMF.Combat.Customer` 标记组件的怪物（`Monster` + `MonsterTargeting` + `MonsterAttack` + `MonsterTruckRoutine`），由 `CustomerSpawner` 生成，`Monster.Died` 时 `Destroy` 并空出名额。扔出去的东西是 `MMF.Player.ThrownItem`（匀速平移 + 滑动，碰到墙 / 怪物会弹开，碰撞是它自己扫的 `BoxCollider2D`）。

## 目标
- **哪些东西能喂**：`ItemDefinition` 加一个只读开关"能喂顾客"（字段例如 `_feedsCustomer`，默认 false）。生成器给所有**煎好的食物**打开：煎怪肉、煎蘑菇、煎怪物手臂、煎怪物腿（就是生成器里作为 `FryingResult` 创建的那些 `Fried*` 物品）。焦土、生的、平底锅都是 false。生成器对已经存在的物品资源也要把这个开关设对（这些是物品定义，不是玩家调过的数值文件）。
- **喂上**：扔出去的东西（平移或滑动中都算）**碰到顾客**时：
  - 东西"能喂顾客" 且 顾客还没吃过 → **喂上**：扔出去的东西**消失**（用掉，不掉在地上），顾客进入"吃到了"。
  - 否则（生的、焦土、平底锅，或者这个顾客已经吃过）→ 和现在一样**弹开**。
  - 碰到普通团子：和现在一样弹开。
- **吃到以后**（顾客身上）：
  - 马上**停下**、**不再打人 / 打车**（正在"蹲一下"准备扑的也取消，不会再扑）。
  - **不挡路、打不到**：平底锅打不到它、玩家和其他怪物穿过它、扔的东西也穿过它。
  - 颜色在"吃完几秒消失"（新数值，默认 **1** 🟡）内从自己的颜色**慢慢变成亮白色**，时间到就消失。
  - 消失时**空出名额**（和被打死一样），`CustomerSpawner` 照常再来新的。消失**不掉东西**、不重生。
- 新数值放进 `CustomerTuning`：
  | 字段 | 中文名 | 默认 |
  |---|---|---|
  | `_purifySeconds` | 吃完几秒消失 | 1 |

## 结构
- `Customer.cs`：加"吃到了"的处理（公开方法例如 `TryFeed()`，返回是否喂上）、变亮计时、消失。需要停掉的组件（`MonsterTargeting`、`MonsterAttack`、`MonsterTruckRoutine`、碰撞体等）在 `Awake` 里缓存引用，不要每帧 `GetComponent`。
- `ThrownItem.cs`：只在"碰到障碍"那一处加判断——碰到的是顾客、东西能喂、`TryFeed()` 成功 → 东西消失，不弹开。顾客的引用在 `Initialize` 里和怪物碰撞体一起缓存（不要每帧找）。
- `CustomerSpawner.cs`：顾客被净化消失时也要空出名额并 `Destroy`（例如 `Customer` 发一个事件，或调用和死亡同一条清理路径；你选简单可靠的，回报说明）。
- 如果停掉攻击需要 `MonsterAttack` 加一个"取消 / 停用"的小方法，可以加，只加这一处；不改它别的行为。
- `ItemDefinition.cs`：只加开关和只读属性。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；84 个测试全部通过（"能不能喂"如果抽成纯函数，加测试）
- [ ] Play：把煎蘑菇（或煎怪肉）右键扔中粉色顾客 → 煎蘑菇消失；顾客停下、不打人，1 秒内变亮白后消失；过一个间隔又来新顾客
- [ ] 扔生蘑菇 / 焦土 / 平底锅打中顾客 → 弹开，顾客照常
- [ ] 吃到以后的 1 秒里：平底锅打不到、玩家能穿过、再扔的东西穿过
- [ ] 煎好的食物扔中紫色团子 → 照样弹开
- [ ] 数值窗口"顾客"组多了"吃完几秒消失"

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/ItemDefinition.cs` | 修改 | 只加"能喂顾客"开关 |
| `Assets/_MMF/Scripts/Data/CustomerTuning.cs` | 修改 | 只加 1 个数值 |
| `Assets/_MMF/Scripts/Combat/Customer.cs` | 修改 | 喂、变亮、消失 |
| `Assets/_MMF/Scripts/Combat/CustomerSpawner.cs` | 修改 | 净化消失时空出名额 |
| `Assets/_MMF/Scripts/Player/ThrownItem.cs` | 修改 | 只加碰到顾客时的判断 |
| `Assets/_MMF/Scripts/Combat/MonsterAttack.cs` | 修改（只在需要时） | 只加取消攻击的小方法 |
| `Assets/_MMF/Tests/EditMode/` 下测试 | 新增（可选） | — |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只设煎好食物的开关、填新引用 |
| `Assets/_MMF/Data/Items/Fried*.asset` | 由生成器更新 | — |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不加奖励、计数、音效、特效（除了变亮）
- `Update` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，打团子捡蘑菇 / 怪肉，进车拖进锅煎好（煎蘑菇进背包）。
2. 出车，选中煎蘑菇，按住右键对着粉色顾客蓄力、松开扔过去：扔中后顾客停下、变白、1 秒后消失。
3. 扔生的东西打中顾客：弹开，顾客照样来打你。
