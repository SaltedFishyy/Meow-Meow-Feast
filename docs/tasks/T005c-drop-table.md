# T005c — 怪物掉落表：怪物肉 / 手臂 / 腿 按几率掉

> 给 Codex：先读 `AGENTS.md`（尤其是 "Good structure, minimal changes" 和 "No dead code"），再读本说明书，再读 `docs/GDD.md` §5.8.1 的"掉落"。T005a～T005b3 已完成并提交。
> 这是**小任务**，只做"怪物死的时候按掉落表掉东西"，以及新物品和地图测试物品的调整。

## 目标
打死咒化团子时，按掉落表**各自独立**掷几率：怪物肉 60%、怪物手臂 30%、怪物腿 30%（🟡 都是暂定）。每样掉 1 个，掉在死的位置附近、散开一点。捡的规则和现有掉落物一样。手臂和腿能放进煎锅，煎好变成"煎怪物手臂""煎怪物腿"。

## 要做的事
1. **掉落表数据** `DropTable`（ScriptableObject，新文件）：一个列表，每条 = 物品（`ItemDefinition`）+ 几率（0–1）。
   - 提供一个**纯逻辑**的方法用来掷几率，例如 `void Roll(Func<float> random01, List<ItemDefinition> results)`：每条各自调用一次 `random01()`，结果 < 几率就把这件物品加进 `results`（每条最多 1 个）。不在里面用 `UnityEngine.Random`，方便测试。
2. **掉落组件** `MonsterDrops`（MonoBehaviour，新文件，放在每只怪物上）：
   - 引用一个 `DropTable`（序列化字段）。
   - 订阅同一个物体上 `Monster.Died`（`OnEnable` / `OnDisable`，或者 `Awake` / `OnDestroy`，注意怪物死后会被 `SetActive(false)`、重生时再打开，**订阅不能丢，也不能重复**）。
   - 怪物死时：用 `UnityEngine.Random.value` 掷几率，每件掉落物在怪物位置附近随机偏移一点生成（散开半径做成序列化字段，默认 **0.5** 🟡），生成方式和现有掉落物一样（`WorldItem`，同样的贴图、0.4 大小、60 秒消失、**没有**捡回延迟，走到 0.5 内自动捡）。贴图、大小、寿命做成序列化字段，由生成器填。
   - 复用结果 List，不要每次死亡都 new 大量对象（生成 GameObject 本身没关系）。
   - **不改 `Monster`、`MonsterSpawner`、`MonsterDefinition`、`WorldItem`、`PlayerBackpack`**。
3. **新物品**（生成器创建，资源已存在时也要确保字段对）：
   | id | 显示名 | 占位颜色 | 煎的结果 |
   |---|---|---|---|
   | `MonsterArm` | 怪物手臂 | 紫红 (0.75, 0.35, 0.55) | `FriedMonsterArm` |
   | `MonsterLeg` | 怪物腿 | 橙棕 (0.8, 0.5, 0.25) | `FriedMonsterLeg` |
   | `FriedMonsterArm` | 煎怪物手臂 | 生的颜色 × 0.65（和现有煎怪肉一样的算法） | 无 |
   | `FriedMonsterLeg` | 煎怪物腿 | 生的颜色 × 0.65 | 无 |
   怪物肉就用现有的 `MonsterMeat`（显示名"怪肉"，不改）。
4. **掉落表资源** `Data/Monsters/CursedDumplingDrops.asset`：怪肉 0.6、怪物手臂 0.3、怪物腿 0.3。生成器给每只怪物加 `MonsterDrops` 并填好引用。
5. **地图测试物品**：生成器里原来摆在地图上的 8 个测试物品，**去掉 4 个怪肉，保留 4 个蘑菇**（蘑菇的位置不变）。只改这一处，相关的代码删干净，不留没用的变量。
6. **测试** `DropTableTests`：
   - 随机数全部 0 → 三样都掉；全部 0.99 → 一样都不掉；
   - 随机数依次 0.59 / 0.31 / 0.29 → 掉怪肉和腿，不掉手臂；
   - 正好等于几率（0.6）→ 不掉（规则是 < 几率）；
   - 每条只调用一次随机数。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；原有 87 个测试 + 新测试全部通过
- [ ] Play：地图上只剩 4 个蘑菇，没有怪肉
- [ ] 打死怪物：死的位置附近掉下 0–3 个东西（大约 0.5 范围内散开），走过去自动捡起，背包显示"怪肉""怪物手臂""怪物腿"
- [ ] 怪物重生以后再打死，还是会掉（订阅没丢），而且不会一次掉两份（没有重复订阅）
- [ ] 拿怪物手臂 / 腿去锅台煎：煎好变成"煎怪物手臂""煎怪物腿"，颜色比生的深
- [ ] 改掉落表资源里的几率（例如手臂改成 1），不改代码就生效

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/DropTable.cs` | 新增 | 掉落表数据 + 掷几率方法 |
| `Assets/_MMF/Scripts/Combat/MonsterDrops.cs` | 新增 | 怪物死时生成掉落物 |
| `Assets/_MMF/Tests/EditMode/DropTableTests.cs` | 新增 | 掉落表测试 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 加新物品、掉落表资源、给怪物加 `MonsterDrops`；地图测试物品去掉怪肉 |
| `Assets/_MMF/Data/Items/*.asset`（新物品）、`Assets/_MMF/Data/Monsters/CursedDumplingDrops.asset`、`Assets/_MMF/Scenes/Prototype.unity` | 由生成器生成 / 更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM。**新文件用 LF 换行**，新加的行和原文件一致）
- 不要做：新怪物、怪物打人、餐车血量、血条、掉落特效 / 音效、掉落物飞出去的动画
- 不改怪物的移动、受伤、死亡、重生；不改背包和锅台的规则
- 设计上有不清楚的地方，写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. 菜单 `MMF → Build Prototype Scene` → Play。
2. 按 1 选中平底锅，出车打怪物，打 3 下死掉。
3. 看它死的地方：可能掉怪肉（红）、怪物手臂（紫红）、怪物腿（橙棕），也可能什么都不掉。走过去自动捡起。
4. 拿手臂或腿回车上锅台煎：煎好变成"煎怪物手臂""煎怪物腿"。
