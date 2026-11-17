# T006d2 — 怪物、武器、掉落表也放进数值窗口

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §6.4。T006d1 已完成并提交：`TuningGroupAttribute`、`TuningLabelAttribute` 在 `Scripts/Data/TuningAttributes.cs`，窗口 `Editor/TuningWindow.cs` 会自动列出标了 `TuningGroup` 的类。
> 这是**小任务**：只给 3 个数值类加标记属性和中文名。**不改窗口代码、不改字段、不改默认值、不改逻辑。**

## 目标
数值窗口 `MMF → 数值` 里多出"怪物、武器、掉落"三组，字段用中文名。

## 要做的事
给下面 3 个类加 `[TuningGroup]`，每个字段加 `[TuningLabel]`（加在已有的 `[SerializeField, ...]` 里或单独一行都可以，跟 T006d1 里 `PlayerTuning` 的写法一致）：

| 类 | 分组（顺序） | 字段 → 中文名 |
|---|---|---|
| `MonsterDefinition` | 怪物（5） | `_size` 大小（下次出生生效）；`_health` 血量（下次出生生效）；`_speed` 移动速度；`_respawnDelay` 死后几秒重生；`_flashDuration` 被打闪白几秒；`_knockbackDistance` 被打后退距离；`_color` 颜色（下次出生生效）；`_chaseStartDistance` 多近开始追玩家；`_chaseStopDistance` 多远不再追玩家；`_leftEndProbability` 去车左边的几率（下次出生生效）；`_avoidanceDuration` 绕开障碍几秒 |
| `WeaponDefinition` | 武器（6） | `_range` 攻击距离；`_angle` 攻击角度（度）；`_damage` 伤害；`_cooldown` 两次攻击间隔（秒）；`_swingFlashDuration` 挥动光效几秒 |
| `DropTable` | 掉落（7） | `_entries` 掉落物和几率（0～1，每样单独算）；`_scatterRadius` 散开半径 |

- `DropTable.Entry` 里的 `_item`、`_chance` 也加中文名：掉落物；几率。（如果窗口因为嵌套字段读不到 `TuningLabel`、显示了英文默认名，**不要改窗口**，写进回报的"疑问"。）
- 不加 `using` 以外的任何东西；`MonsterDefinition.cs` 有 BOM，保留；另外两个文件没有 BOM，不要加。

## 验收标准
- [ ] 编译无报错无警告；原有 93 个测试全部通过
- [ ] `MMF → 数值` 按"玩家、背包、煎锅、镜头、怪物、武器、掉落"顺序显示 7 组，新的 3 组字段都是中文名
- [ ] Play 中在窗口里改武器"伤害"为 3：下一次打怪就是 3 点伤害（怪物 3 血，一下打死）
- [ ] Play 中改怪物"移动速度"：怪物马上变快 / 变慢
- [ ] 掉落表的几率能在窗口里改，改了马上保存
- [ ] `git diff` 只有属性的增加，没有其他改动

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/MonsterDefinition.cs` | 修改 | 只加标记属性和中文名 |
| `Assets/_MMF/Scripts/Data/WeaponDefinition.cs` | 修改 | 只加标记属性和中文名 |
| `Assets/_MMF/Scripts/Data/DropTable.cs` | 修改 | 只加标记属性和中文名 |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM）
- 不改窗口、不改任何默认值或逻辑
- 不要做：Designer 场景（T006e）
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. 菜单 `MMF → 数值`：多了"怪物、武器、掉落"三组。
2. Play 中改武器"伤害"、怪物"移动速度"，马上生效；标着"下次出生生效"的，要等怪物死了重生才变。
3. 注意："恢复默认值"不会动掉落表里的掉落物列表（列表没有代码默认值）。
