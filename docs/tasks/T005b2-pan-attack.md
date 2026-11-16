# T005b2 — 平底锅攻击逻辑（先不接鼠标）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Good structure, minimal changes" 和 "No dead code"），再读本说明书，再读 `docs/GDD.md` §5.3.1。T005b1（平底锅物品）已完成并提交。
> 这是**小任务**，只做"挥一下能打到扇形里的怪"的**逻辑**，用一个公开方法触发。**不接左键 / 鼠标**（T005b2b），**不做扇形显示**（T005b3），不做掉落（T005c）。

## 目标
玩家身上多一个攻击组件。调用 `TryAttack(瞄准的世界坐标)` 时，如果手上选中的是武器、冷却好了，就朝瞄准方向挥一下：扇形范围里的怪物**全部**挨打。

## 相关设计（GDD §5.3.1）
- 选中平底锅才能打。前方扇形：范围 **1.5** 🟡、角度 **90°** 🟡（以瞄准方向为中心，左右各 45°），伤害 **1** 🟡，冷却 **0.4 秒** 🟡。
- **扇形里的怪全部打到**，每只扣一次伤害。
- **在车里**：能挥（冷却照算），但打不到任何东西。

## 要做的事
1. **武器数据** `WeaponDefinition`（ScriptableObject，新文件）：范围、角度（度）、伤害、冷却。全部是序列化字段 + 只读属性，默认值就是上面的数。**不要加扇形显示用的参数**，T005b3 再加。
2. **物品指向武器**：`ItemDefinition` 加一个 `WeaponDefinition` 引用字段 + 只读属性（为空 = 不是武器）。
3. **扇形判断** `WeaponArc`（纯 C#，静态方法，新文件）：给定起点、瞄准方向、目标点、范围、角度，返回目标点在不在扇形里。规则：
   - 距离 ≤ 范围，并且与瞄准方向的夹角 ≤ 角度 / 2。
   - 目标点和起点重合时，算在扇形里。
   - 瞄准方向长度为 0 时，返回 false。
4. **攻击组件** `PlayerAttack`（MonoBehaviour，新文件，放在 Player 上）：
   - 公开方法 `bool TryAttack(Vector2 aimWorldPosition)`：
     - 当前选中格的物品不是武器 → 返回 false，什么都不做、不算冷却。
     - 冷却没好 → 返回 false。
     - 否则就算挥了：开始冷却，返回 true。
       - 玩家在车里（用 `TruckArea.Contains(玩家位置)`）→ 不打任何东西。
       - 不在车里 → 找范围内的怪物，用怪物碰撞体上**离玩家最近的点**做扇形判断；在扇形里的，每只调用一次 `Monster.TakeHit(伤害, 从玩家指向怪物的方向)`。同一只怪一次挥锅只挨一下。
   - 查找时用不产生垃圾的写法（复用 List / 数组），不要每帧 `Find` / `GetComponent`。
   - 需要知道选中物品：用已有的 `PlayerBackpack.ItemAt(Model.SelectedSlot)`，不改背包。
   - **不在这个任务里读输入**，也不改 `PlayerInputReader`、`PlayerInteractor`。
5. **生成器**：生成武器资源 `Data/Weapons/FryingPan.asset`（默认值），把它填进 `Data/Items/FryingPan.asset` 的武器字段（资源已存在时也要填对）。给 Player 加上 `PlayerAttack`，序列化引用（例如 `TruckArea`）都填好。
6. **测试** `WeaponArcTests`：正前方在范围内 → 在；超出范围 → 不在；正好 45° 边上 → 在；50° → 不在；正后方 → 不在；目标和起点重合 → 在；瞄准方向为 0 → 不在。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；原有 76 个测试 + 新测试全部通过
- [ ] （Claude 用 script eval 在 Play 里测）选中平底锅、车外：瞄准一只 1 单位外的怪调用 `TryAttack` → 返回 true，怪扣 1 血、闪白、被击退
- [ ] 两只怪都在扇形里 → 都扣血；一只在玩家背后 → 不扣
- [ ] 0.4 秒内再调用 → 返回 false，不扣血；过了冷却 → 又能打
- [ ] 选中空格或其他物品 → 返回 false
- [ ] 玩家在车里 → 返回 true（挥了），但没有怪扣血
- [ ] 现有功能（背包、锅台、怪物移动）不变

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/WeaponDefinition.cs` | 新增 | 武器数据 |
| `Assets/_MMF/Scripts/Data/ItemDefinition.cs` | 修改 | 只加武器引用字段 + 只读属性 |
| `Assets/_MMF/Scripts/Combat/WeaponArc.cs` | 新增 | 扇形判断（纯 C#） |
| `Assets/_MMF/Scripts/Combat/PlayerAttack.cs` | 新增 | 攻击组件 |
| `Assets/_MMF/Tests/EditMode/WeaponArcTests.cs` | 新增 | 扇形判断测试 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加武器资源生成、填进平底锅物品、给 Player 加 `PlayerAttack` |
| `Assets/_MMF/Data/Weapons/FryingPan.asset`、`Assets/_MMF/Data/Items/FryingPan.asset`、`Assets/_MMF/Scenes/Prototype.unity` | 由生成器生成 / 更新 | — |

如果 `MMF.Tests.EditMode.asmdef` 访问不到 `MMF.Combat`，在回报里说明，不要自己改 asmdef 以外的东西（改 asmdef 需要写明原因）。

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM）
- 不要做：左键 / 鼠标输入、左键优先级（T005b2b）；扇形显示、闪 0.2 秒（T005b3）；掉落（T005c）；怪物打人、血条
- 不改 `Monster`、`PlayerBackpack`、`PlayerInteractor`、`PlayerInputReader`
- 设计上有不清楚的地方，写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
这个小任务还不能用鼠标打（下一个小任务接上），Claude 会用脚本在 Play 里测。
