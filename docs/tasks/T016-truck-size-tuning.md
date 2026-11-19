# T016 — 餐车长、宽、门宽放进数值窗口（下次 Play 生效）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §5.2（"车的长、宽、门宽都能在数值窗口'餐车'组里调"）。
> 现状：车的大小 `_size`（12 × 7）和门宽 `_doorWidth`（2）是 `MMF.Truck.TruckArea` 组件上的序列化字段，`RefreshLayout()` 按它们摆地板、6 段墙、2 个挡怪物的门；锅台在生成器里放到 `truck.TopLeftCell(Vector2.one)`。

## 目标
- `TruckTuning`（数值窗口"餐车"组）加 3 个数值，**下次 Play 生效**：
  | 字段 | 中文名 | 默认 |
  |---|---|---|
  | `_length` | 车长（左右，下次 Play 生效） | 12 |
  | `_width` | 车宽（上下，下次 Play 生效） | 7 |
  | `_doorWidth` | 门宽（下次 Play 生效） | 2 |
  - 加合理的下限（`Min`），并在使用时保证不会摆出坏的车：门宽不能超过"车长 − 2 × 墙厚"，车长 / 车宽不能小于能放下锅台（1 格）和玩家的大小。超出时用夹住后的值，不报错。墙厚是技术常量，留在 `TruckArea` 上。
- `TruckArea` 改成从 `TruckTuning` 读大小和门宽（删掉组件上原来的 `_size` / `_doorWidth` 字段，不要两处都有），**在 `Awake` 里**读一次并 `RefreshLayout()`，保证别的脚本在 `Start` / 第一帧用到 `ExteriorRect` / `InteriorRect` / `Contains` / `EndPoint` 时已经是新大小。Play 中改数值不会变（下次 Play 才变）。
  - 生成器（编辑模式下）建车时也按数值文件的大小摆一次，让场景视图里看到的车和 Play 时一样。
- **锅台**：Play 开始时放到车内**左上角**那一格（`TopLeftCell`），所以车变大变小它都在左上角。之后玩家搬动它照旧。用最小的改动实现（例如锅台上一个小组件或 `FryingStation` 加一个 `_truck` 引用在 `Start` 里摆位置），回报说明。
- 两个门：始终在上下墙正中间、宽度 = 门宽（`RefreshLayout` 已经这样，只是数值来源变了）。挡怪物的门跟着变。
- 车内镜头：不改（还是对准车中间，大小用"镜头"组的"车内镜头大小"）。
- 玩家出生：还是车中心（不改）。
- 怪物 / 顾客出生点、蘑菇位置：不改。如果车大到盖住了某个出生点，这次不处理，回报里写一句即可。

## 结构
- 改 `Scripts/Data/TruckTuning.cs`（加 3 个数值）、`Scripts/Truck/TruckArea.cs`（读数值、Awake 摆好）、锅台摆位置（`FryingStation.cs` 或一个新的小脚本，二选一）、生成器（填 `TruckArea._tuning` 引用、锅台引用）。
- 已有的 `TruckTuning.asset` 不覆盖（新字段用默认值）。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；84 个测试全部通过
- [ ] 默认值（12 × 7、门 2）时，Play 里车、门、锅台位置、怪物去车头车尾、挡怪物的门和现在完全一样
- [ ] （Claude 测）把数值改成例如 16 × 9、门 3，重新 Play：车墙、地板、两个门变成新大小，门在上下墙正中间、宽 3；锅台在新车的左上角；玩家能从门进出；怪物去新车的车头车尾、打车；怪物进不了门；搬锅台、放置预览只在新车里能放
- [ ] 改成小车（例如 8 × 5、门 2）同样正常
- [ ] Play 中改这 3 个数值，车不变；下次 Play 才变
- [ ] 数值窗口"餐车"组多了这 3 个数值

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/TruckTuning.cs` | 修改 | 只加 3 个数值 |
| `Assets/_MMF/Scripts/Truck/TruckArea.cs` | 修改 | 大小、门宽改从数值读；Awake 里摆好 |
| `Assets/_MMF/Scripts/Cooking/FryingStation.cs` 或 `Assets/_MMF/Scripts/Cooking/` 下 1 个新脚本 | 修改 / 新增（二选一） | Play 开始时把锅台放到左上角 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只填新引用、按数值摆车 |
| `Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不改地图大小、镜头、出生点
- `Update` 里不要 `Find`、`GetComponent`、分配垃圾
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. 打开 `MMF → 数值` →"餐车"组，把"车长""车宽""门宽"改成想要的值。
2. 按 Play：车按新大小出现，门在上下正中间，锅台在左上角。
3. 想改回来就把数值改回 12、7、2。
