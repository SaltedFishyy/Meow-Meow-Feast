# T009f — 餐车血 0 = 失败画面，按 R 整局重开

> 给 Codex：先读 `AGENTS.md`，再读本说明书和 `docs/GDD.md` §5.8.3（"失败细节"）。
> T009e1、T009e2 已完成并提交：`MMF.Truck.TruckHealth`（`Current`、`Max`、`CanBeHit`、`TakeHit`、`Changed(current, max)`）、顶部车血条。
> 这是**小任务**：只做失败画面和重开。完整的结算画面以后再做。

## 目标
- 餐车血掉到 **0** 的那一刻：
  - **整个游戏停住**：怪物、玩家、扔出去的东西、煎锅计时、倒地倒计时都不动（例如 `Time.timeScale = 0`，你选可靠的做法，回报写明）。
  - 玩家的所有操作都没反应（移动、E、Q、左右键、选格、滚轮、煎锅的 WASD）。
  - 屏幕中间显示大字 **"失败"**，下面一行 **"按 R 重新开始"**。可以加一层半透明深色底让字清楚。
- 失败画面显示时按 **R**：**整局从头开始**——重新载入当前场景，所以地图、怪物、背包、车血、玩家血全部回到开局；时间恢复正常速度。
- 没失败时按 R **没有任何效果**。
- 数值窗口里调过的数值不受影响（数值文件是资源，重新载入场景不会改它们）。

## 要做的事
1. **`TruckHealth`**：加一个"血到 0"的事件（例如 `Destroyed`），只在从 >0 变成 0 时发一次。
2. **新脚本 `Scripts/UI/GameOverScreen.cs`**（或你觉得更清楚的名字 / 拆法，最多 2 个新脚本，回报说明）：监听 `Destroyed` → 停住游戏、关掉玩家操作、显示失败画面；失败后按 R → 恢复时间、重新载入当前场景。
3. **R 键**：在 `MMFControls.inputactions` 里加一个 `Restart` 按钮动作（绑定 `<Keyboard>/r`）。放在 `Gameplay` 里还是新开一个动作表你选，但要保证失败时（玩家操作已关掉）R 还能用、没失败时 R 没效果。生成器里的 `requiredActions` 检查要跟着加上。
4. **能重新载入场景**：现在 `ProjectSettings/EditorBuildSettings.asset` 里没有任何场景，`SceneManager.LoadScene` 会失败。让生成器在生成场景后把 `Assets/_MMF/Scenes/Prototype.unity` 加进 Build Settings（已经在里面就不重复加）。
5. **生成器**：建失败画面的 UI（放在已有 Canvas，或新建一个排在最上面的 Canvas），默认隐藏，填引用。中文字用和背包栏一样的字体（保证能显示中文）。

## 验收标准
- [ ] 编译无报错无警告；场景生成 3 次，引用都不为空；93 个测试全部通过
- [ ] 车血打到 0：画面中间出现"失败"和"按 R 重新开始"，怪物、玩家都停住，按 WASD / E / Q / 鼠标都没反应
- [ ] 按 R：回到开局（玩家在车中间、5 格血、背包只有平底锅、车血 100/100、怪物在出生点），可以正常玩
- [ ] 没失败时按 R 没反应
- [ ] 重开以后再失败、再按 R，同样有效（可以重复）
- [ ] 失败时正在倒地 / 煎东西 / 蓄力：都停住，重开后一切正常

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Truck/TruckHealth.cs` | 修改 | 只加"血到 0"事件 |
| `Assets/_MMF/Scripts/UI/GameOverScreen.cs` | 新增 | 失败画面、停住、R 重开 |
| `Assets/_MMF/Input/MMFControls.inputactions` | 修改 | 只加 `Restart` 动作和 R 绑定 |
| `Assets/_MMF/Scripts/Player/PlayerInputReader.cs` | 修改（只在需要时） | 只加关掉玩家操作所需的最小改动 |
| `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` | 修改 | 失败画面 UI、引用、`requiredActions`、把场景加进 Build Settings |
| `Assets/_MMF/Scenes/Prototype.unity`、`ProjectSettings/EditorBuildSettings.asset` | 由生成器更新 | — |

## 范围限制
- 只动上面清单里的文件；文件里只改需要改的部分（不重写、不改格式、**不改换行符**（LF）、不加 / 删 BOM）
- 不要做：结算画面（金币、击败数等）、胜利条件、主菜单
- 有不清楚的地方写进回报的"疑问"，不要自己决定

## 给用户的试玩步骤
1. Play，待在车里等怪物把车打到 0（大约 40 秒）：画面停住，中间显示"失败 / 按 R 重新开始"。
2. 按 R：一切回到开局。
