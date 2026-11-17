# T008a — 右键蓄力 + 箭头（这一步先不丢）

> 给 Codex：先读 `AGENTS.md`（尤其是 "Tunable gameplay values (数值)"），再读本说明书和 `docs/GDD.md` §背包里的"**右键蓄力丢**"。
> 这是**小任务**：只做"按住右键蓄力、箭头跟着鼠标越蓄越长、左键取消"。**松开右键这一步什么都不丢**，只结束蓄力（真正把东西丢出去是下一个小任务 T008b）。

## 目标
- 按住鼠标右键：开始蓄力，角色旁边出现一个箭头，从角色指向鼠标方向，每帧跟着鼠标转。
- 箭头长度 = 现在松手会丢到的距离：刚按下时 = 最近距离，按住越久越长，到"蓄满时间"时 = 最远距离，之后不再变长。
- 松开右键：蓄力结束，箭头消失（T008a 不丢东西）。
- 蓄力时按左键：**只取消蓄力**，箭头消失。这一下左键**不挥锅、不举 / 放锅台**。
- 不能蓄力的情况（按右键没反应）：选中的格子是空的；举着锅台或正在煎东西（就是现在 `PlayerInteractor.CanDrop` 为 false 的时候，和 Q 一样）。
- 蓄力时走路速度不变；蓄力时可以换格子（数字键 / 滚轮），蓄力不中断。
- 鼠标正好在角色身上（方向长度为 0）时，箭头保持上一次的方向；从没有过方向就朝右。

## 要做的事
1. **数值文件** `Scripts/Data/ThrowTuning.cs`（新，ScriptableObject，`[CreateAssetMenu]`，`[TuningGroup("丢东西", 8)]`）：
   | 字段 | 中文名（`TuningLabel`） | 默认值 |
   |---|---|---|
   | 蓄满时间 | 蓄满要几秒 | 1 |
   | 最近距离 | 没蓄力时丢多远 | 1 |
   | 最远距离 | 蓄满时丢多远 | 6 |
   都加 `Min(0)`（蓄满时间用 `Min(0.01)`）。生成器在 `Assets/_MMF/Data/Tuning/ThrowTuning.asset` 创建，**已经存在就不覆盖**。
2. **蓄力规则** `Scripts/Player/PlayerThrow.cs`（新，挂在玩家上）：
   - 公开：`bool Charging`、`float Distance`（现在的丢出距离）、`Vector2 Direction`（单位向量）、`TryStartCharge()`、`Release()`、`Cancel()`、`SetAim(Vector2 worldPosition)`。
   - `TryStartCharge`：按上面的"不能蓄力的情况"判断，能就开始。
   - `Release`：T008a 里只结束蓄力（留一个清楚的位置给 T008b 加"丢出去"，但**不要写空方法或 TODO 注释占位**）。
   - 数值每次用的时候从 `ThrowTuning` 读（Play 中改马上生效）。
   - **不在这里直接读鼠标 / 键盘**：鼠标位置由 `PlayerInputReader` 读，通过 `SetAim` 传进来。
3. **箭头显示** `Scripts/Player/ThrowArrow.cs`（新，玩家的子物体）：只负责画。读 `PlayerThrow` 的 `Charging`、`Direction`、`Distance`。蓄力时显示，不蓄力时隐藏。样子：一条线 + 箭头尖，从角色中心朝 `Direction` 延伸到 `Distance`，半透明白色，画在地面和物品上面、角色下面或上面都可以（选看得清的）。可以照 `SwingFlash` 的做法用 `MeshFilter` + `MeshRenderer` 画，网格只在需要时更新，不要每帧分配新数组。线宽、颜色、箭头尖大小是外观，放在这个组件的序列化字段上，**不进数值文件**。
4. **输入**：
   - `Assets/_MMF/Input/MMFControls.inputactions`：在 `Gameplay` 里加一个按钮动作 `Throw`，绑定 `<Mouse>/rightButton`。只加这一个动作，别的不动。
   - `PlayerInputReader`：`Throw` 按下（`started` 或 `performed`，你选合适的）→ `TryStartCharge`；松开（`canceled`）→ `Release`。蓄力中每帧把鼠标世界坐标传给 `SetAim`（世界坐标的算法照现在 `Lift` 里的写法，可以抽成一个小方法共用）。`Lift`（左键）回调最前面加：如果正在蓄力 → `Cancel()` 然后 return。
5. **生成器** `PrototypeSceneBuilder`：给玩家加 `PlayerThrow`（填 `ThrowTuning`、需要的引用）和箭头子物体 `ThrowArrow`；加载 / 创建 `ThrowTuning.asset`。`requiredActions` 里加 `"Throw"`。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；原有 93 个测试全部通过
- [ ] 手上有东西时按住右键：出现箭头指向鼠标，移动鼠标箭头跟着转；箭头 1 秒内从 1 格长到 6 格，之后不再变长
- [ ] 松开右键：箭头消失，背包里东西不变（T008a 不丢）
- [ ] 蓄力时按左键：箭头消失；没有挥锅（没有扇形闪），也没有举起锅台
- [ ] 手上空的、举着锅台、正在煎东西时，按右键没反应
- [ ] 蓄力时走路速度和平时一样；蓄力时换格子不中断
- [ ] 数值窗口出现"丢东西"组，3 个中文名；Play 中改"蓄满要几秒"马上生效
- [ ] 生成 3 次后 `ThrowTuning.asset` 只有一个；手动改值再生成，值不被覆盖

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Data/ThrowTuning.cs` | 新增 | 丢东西数值文件 |
| `Assets/_MMF/Scripts/Player/PlayerThrow.cs` | 新增 | 蓄力规则 |
| `Assets/_MMF/Scripts/Player/ThrowArrow.cs` | 新增 | 箭头显示 |
| `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` | 修改 | 只加 Throw 的回调、每帧传鼠标位置、Lift 回调最前面的取消判断（可抽一个算鼠标世界坐标的小方法） |
| `Assets/_MMF/Input/MMFControls.inputactions` | 修改 | 只加 `Throw` 动作和它的绑定 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 只加 `PlayerThrow`、箭头子物体、`ThrowTuning` 的创建和引用填写，`requiredActions` 加 `Throw` |
| `Assets/_MMF/Data/Tuning/ThrowTuning.asset`、`Assets/_MMF/Scenes/Prototype.unity` | 由生成器生成 / 更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、不改换行符、不加 / 删 BOM）
- 不要做：真正把东西丢出去、飞行、撞墙（T008b）
- 不改 Q 丢东西、攻击、锅台的现有行为（除了"蓄力时左键只取消"）
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，选中有东西的格子，按住右键：角色旁边出现箭头，跟着鼠标转，1 秒内变长。
2. 松开右键：箭头消失（这一步还不会丢东西）。
3. 蓄力时按左键：箭头消失，不会挥锅。
