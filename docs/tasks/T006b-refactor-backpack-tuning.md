# T006b — 整理：背包、掉落物数值搬进数值文件（行为不变）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)" 和 Refactor tasks），再读本说明书和 `docs/GDD.md` §6.4。T006a（`PlayerTuning`）已完成并提交，照它的做法来。
> 这是**整理小任务**：只把数值搬家，**不加功能、行为完全不变**。

## 目标
背包和掉落物的玩法数值，从场景组件 `PlayerBackpack`、`MonsterDrops` 的字段搬进数值文件。组件引用数值文件、用的时候读。

## 要做的事
1. 新建 `BackpackTuning`（ScriptableObject，`Scripts/Data/`，`[CreateAssetMenu]`），默认值必须和现在一样：
   | 字段 | 默认值 | 原来在 | 说明 |
   |---|---|---|---|
   | 格数 | 5 | `PlayerBackpack._slotCount` | **只在开局读**（`Awake` 建 `BackpackModel`），Play 中改了下次 Play 才生效。在字段上用 `[Tooltip]` 写明"下次 Play 生效" |
   | 每格叠几个 | 3 | `PlayerBackpack._stackLimit` | 同上，只在开局读 |
   | 自动捡起距离 | 0.5 | `PlayerBackpack._pickupRadius` | |
   | 丢掉后几秒才能捡回 | 2 | `PlayerBackpack._dropPickupDelay` | |
   | 掉落物几秒后消失 | 60 | `PlayerBackpack._worldLifetime` | 怪物掉落物也用这个（见第 3 条） |
   | 头上"x"显示几秒 | 1 | `PlayerBackpack._failureSeconds` | |
2. `PlayerBackpack`：删掉上面 6 个字段，改成 `[SerializeField] BackpackTuning _tuning`，用到的地方从 `_tuning` 读（格数和叠放数在 `Awake` 读一次；其他每次用时读）。**其他字段不动**（贴图、头上图标的位置 / 大小、`_worldItemSize`、`_showHeldIcon`、开局物品），它们是界面 / 外观数值。
3. `MonsterDrops`：
   - `_lifetime` 删掉，改成从同一个 `BackpackTuning` 读"掉落物几秒后消失"。GDD 规定怪物掉落物和普通掉落物一样 1 分钟消失，现在两边都是 60，合并后行为不变。
   - `_scatterRadius`（散开半径 0.5）搬进 `DropTable`（每种怪的掉落表各自一个散开半径），`MonsterDrops` 从 `_dropTable` 读。生成器创建掉落表时默认 0.5；**已经存在的掉落表资源没有这个字段时，Unity 会用代码里的默认值 0.5，不用生成器去改它**。
   - `_size`（掉落物大小）是外观数值，留在组件上不动。
4. 生成器：数值文件放在 `Assets/_MMF/Data/Tuning/BackpackTuning.asset`，**已经存在就不覆盖**，不存在才用默认值创建。填进 `PlayerBackpack` 和每只怪物的 `MonsterDrops`。删掉生成器里给 `MonsterDrops` 设置 `_scatterRadius`、`_lifetime` 的代码。
5. `WorldItem` **不改**（地图上摆的蘑菇用的是它自己的字段，保持原样）。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；原有 93 个测试全部通过
- [ ] 生成 3 次后 `BackpackTuning.asset` 只有一个；手动改它的值再生成场景，值不被覆盖
- [ ] Play：背包 5 格、每格叠 3 个；捡东西距离、Q 丢掉 2 秒后才能捡回、掉落物 60 秒消失、头上"x" 1 秒，都和原来一样
- [ ] 打死怪物：掉落物散开半径约 0.5、60 秒消失，和原来一样
- [ ] Play 中把"丢掉后几秒才能捡回"改成 5：下一次丢的东西 5 秒后才能捡（马上生效）
- [ ] `PlayerBackpack` 上不再有那 6 个数值字段；`MonsterDrops` 上不再有 `_lifetime`、`_scatterRadius`

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/BackpackTuning.cs` | 新增 | 背包数值文件 |
| `Assets/_MMF/Scripts/Player/PlayerBackpack.cs` | 修改 | 只把 6 个数值字段换成读 `BackpackTuning` |
| `Assets/_MMF/Scripts/Combat/MonsterDrops.cs` | 修改 | 只把 `_lifetime`、`_scatterRadius` 换成读数值文件 / 掉落表 |
| `Assets/_MMF/Scripts/Data/DropTable.cs` | 修改 | 只加散开半径字段 + 只读属性 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加 `BackpackTuning` 的加载 / 创建和引用填写，删掉 `MonsterDrops` 那两个旧字段的设置 |
| `Assets/_MMF/Data/Tuning/BackpackTuning.asset`、`Assets/_MMF/Scenes/Prototype.unity` | 由生成器生成 / 更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM）
- **行为必须和原来完全一样**，默认值必须一模一样
- 不要做：数值窗口、标记属性（T006d）；煎锅 / 镜头的数值（T006c）
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
这一步只是整理，玩起来和原来一模一样。
