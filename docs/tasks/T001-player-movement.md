# T001 — 项目骨架 + 原型场景 + Player 移动

> 给 Codex：先读 `AGENTS.md`，再读本说明书，再读下面列出的 GDD 章节。

## 目标
在 Unity 里通过菜单一键生成原型场景：俯视固定镜头、一块地面、一个 `Player` 占位方块。按 Play 后用 WASD 移动 Player，移动有一点点加速和减速，Player 不会走出场地。

## 相关设计
- `docs/GDD.md` §4（原型范围）、§5.2（镜头）、§5.3（玩家：移动、操作方式）
- 只实现**移动**。GDD 里其他内容（鼠标攻击、E 互动、背包、餐车、食材……）**本任务都不做**。

## 要做的事
1. **目录与程序集**
   - 按 `AGENTS.md` 建立 `Assets/_MMF/` 目录结构（只建本任务用到的文件夹即可）。
   - 运行时程序集 `MMF.Runtime`（`Assets/_MMF/Scripts/MMF.Runtime.asmdef`），引用 `Unity.InputSystem`。
   - 编辑器程序集 `MMF.Editor`（`Assets/_MMF/Editor/MMF.Editor.asmdef`），仅 Editor 平台，引用 `MMF.Runtime`。
2. **输入**
   - 新建输入资源 `Assets/_MMF/Input/MMFControls.inputactions`，Action Map `Gameplay`，只有一个动作 `Move`（Value / Vector2），绑定 **WASD**（2D Vector Composite）。
   - **不要**加其他动作（鼠标、E 键以后的任务再加）。不要改 `Assets/Settings/InputSystem_Actions.inputactions`。
3. **Player 移动（为以后网络联机留结构）**
   - 把"读输入"和"移动逻辑"分开：
     - `PlayerInputReader`（`MMF.Player`）：从 Input System 读取 `Move`，对外提供当前移动方向 `Vector2 MoveInput`。
     - `PlayerMovement`（`MMF.Player`）：只接收一个移动方向（不直接碰 Input System），负责加减速和移动。以后联机时，移动方向可以来自网络而不是本地键盘。
   - 用 `Rigidbody2D`（重力 0、锁定旋转）+ 碰撞体移动，在 `FixedUpdate` 里处理。
   - 参数用 `[SerializeField]`，可在 Inspector 调：
     - `maxSpeed` = **5**（单位/秒）
     - 加速到满速的时间 = **0.1 秒**，松手减速到停的时间 = **0.1 秒**（"一点点"加减速）
   - 斜向移动不能比直线快（方向向量要归一化 / 限制长度）。
4. **原型场景生成器（Editor 脚本）**
   - 菜单 `MMF/Build Prototype Scene`：生成（或覆盖重建）`Assets/_MMF/Scenes/Prototype.unity` 并打开它。可以重复运行。
   - 场景内容：
     - **Main Camera**：正交、俯视（看向 XY 平面）、固定不动；正交尺寸 5（16:9 下约 17.8 × 10 单位可见）。
     - **Global Light 2D**（URP 2D 需要，否则精灵是黑的）。
     - **Ground**：铺满镜头可见范围的地面，用一种素色占位。
     - **Bounds**：沿镜头可见范围的四条边放 4 个看不见的 `BoxCollider2D` 墙，Player 走不出画面。
     - **Player**：一个占位方块（约 1 × 1 单位，和地面颜色明显不同），名字就叫 `Player`，挂 `Rigidbody2D`、`BoxCollider2D`、`PlayerInputReader`、`PlayerMovement`，放在场地中央。
   - 占位图：可以用代码生成一张白色方块贴图保存到 `Assets/_MMF/Art/Placeholder/` 并作为 Sprite 导入，再用 `SpriteRenderer.color` 上色。
   - 不要改 Build Settings、不要改 `ProjectSettings/`、不要动 `Assets/Scenes/SampleScene.unity`。

## 验收标准
- [ ] 能编译，控制台无报错、无警告（Input System 相关的除外请说明）
- [ ] 运行菜单 `MMF/Build Prototype Scene` 后生成并打开 `Assets/_MMF/Scenes/Prototype.unity`；再运行一次也不出错
- [ ] 按 Play：画面是俯视固定镜头，中间有一个 `Player` 方块
- [ ] WASD 能 8 方向移动，最高速度 5 单位/秒；斜向不会更快
- [ ] 起步和停下有很短的加速 / 减速（约 0.1 秒），不是瞬间
- [ ] Player 走不出画面边缘
- [ ] `PlayerMovement` 不引用 Input System；输入只在 `PlayerInputReader` 里读取
- [ ] 速度、加速时间、减速时间都能在 Inspector 里调

## 范围限制
- 只新增 / 修改 `Assets/_MMF/` 下的文件。
- 不要做：鼠标、攻击、E 键互动、背包、餐车、食材、怪物、UI、网络代码、手柄支持。
- 不要安装新的包。

## 给用户的试玩步骤
1. 回到 Unity，等它编译完（右下角转圈结束）。
2. 顶部菜单点 `MMF → Build Prototype Scene`。
3. 按 Play，用 WASD 移动方块。
4. 应该看到：方块能 8 个方向移动，起步和停下有一点点顺滑感，走到画面边缘会被挡住。
