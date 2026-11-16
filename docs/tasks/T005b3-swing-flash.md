# T005b3 — 挥锅时扇形闪 0.2 秒

> 给 Codex：先读 `AGENTS.md`（尤其是 "Good structure, minimal changes"），再读本说明书，再读 `docs/GDD.md` §5.3.1。T005b2（`PlayerAttack.TryAttack`）、T005b2b（左键接上）已完成并提交。
> 这是**小任务**，只做"挥一下时显示一个扇形，0.2 秒后消失"。不改攻击判定、不做掉落（T005c）。以后用户会换成正式特效，现在是占位。

## 目标
每次真的挥了锅（`TryAttack` 返回 true，**包括在车里挥空**），玩家前方朝瞄准方向出现一个半透明扇形，形状和攻击范围一致（半径 = 武器范围，张角 = 武器角度），**0.2 秒** 🟡 后消失。

## 要做的事
1. `WeaponDefinition` 加一个"扇形显示时长"字段，默认 **0.2** 秒 🟡（序列化 + 只读属性）。
2. `PlayerAttack` 加一个事件，例如 `event Action<WeaponDefinition, Vector2> Swung`（武器、瞄准方向）。**只在真的挥了时**触发，在车里挥空也触发；返回 false 的情况不触发。`TryAttack` 的其他逻辑不动。
3. 新脚本 `SwingFlash`（MonoBehaviour，放在 Player 的子物体上）：
   - 订阅 `PlayerAttack.Swung`（`OnEnable` / `OnDisable` 里订阅 / 取消）。
   - 收到事件时，显示一个扇形：圆心在玩家位置，朝瞄准方向，半径和张角取自这把武器的数据，所以**改了范围或角度，扇形跟着变**。
   - 扇形可以用运行时生成的 Mesh（`MeshFilter` + `MeshRenderer`，用 Sprites-Default 这种支持顶点色 / 透明的材质），网格只在武器数据变化时重建，**不要每帧 new Mesh / 分配数组**。也可以用其他不产生垃圾的办法，在回报里写清楚。
   - 颜色：占位用半透明白色（例如 alpha 0.5），做成序列化字段。
   - 显示时长到了就隐藏。冷却期间不会再触发（`TryAttack` 返回 false），所以不用处理重叠；如果冷却比显示时长短，新的挥锅直接重新开始计时。
   - 画在地面和怪物上面，能看清（排序层 / sortingOrder 设好）。
   - 瞄准方向长度为 0 时不显示。
4. 生成器：给 Player 加一个子物体 "Swing Flash"，挂 `SwingFlash` 和需要的渲染组件，引用（`PlayerAttack`、材质等）都填好；默认隐藏。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；原有 87 个测试全部通过
- [ ] Play，选中平底锅按左键：朝鼠标方向出现一个 90°、半径 1.5 的半透明扇形，0.2 秒后消失
- [ ] 在车里按左键也会闪（但打不到东西）；选中空格、在锅台旁边、举着锅台、煎菜时按左键都不闪
- [ ] 把武器资源的范围改成 3、角度改成 180，再挥：扇形跟着变大、变成半圆
- [ ] 连续挥锅时没有明显的每帧内存分配（不要在 `Update` 里 new）
- [ ] 攻击判定、冷却、左键优先级都不变

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/WeaponDefinition.cs` | 修改 | 只加扇形显示时长字段 + 只读属性 |
| `Assets/_MMF/Scripts/Combat/PlayerAttack.cs` | 修改 | 只加 `Swung` 事件和触发它的那一行 |
| `Assets/_MMF/Scripts/Combat/SwingFlash.cs` | 新增 | 扇形显示 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加 "Swing Flash" 子物体的生成和引用 |
| `Assets/_MMF/Data/Weapons/FryingPan.asset`、`Assets/_MMF/Scenes/Prototype.unity` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM。**新加的行要和文件原来的换行符一样**）
- 不要做：正式特效、音效、挥锅动画、屏幕震动；掉落（T005c）；改攻击数值或判定
- 设计上有不清楚的地方，写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. 菜单 `MMF → Build Prototype Scene` → Play。
2. 按 1 选中平底锅，按左键：鼠标方向闪一下白色扇形。
3. 出车去打怪：扇形盖到的怪会闪白、后退。
