# 开发日志

最新的写在最上面。每条：日期 · 任务 · 做了什么 · 改动文件 · 怎么试玩 · 遗留问题。

---

## 2026-11-15 · T001 · 项目骨架 + 原型场景 + Player 移动

**做了什么**（Codex 编写，Claude 审查通过）
- 建立 `Assets/_MMF/` 目录、运行时程序集 `MMF.Runtime` 和编辑器程序集 `MMF.Editor`。
- 输入资源 `MMFControls.inputactions`：目前只有 `Move`（WASD）。
- `PlayerInputReader` 读取键盘输入，`PlayerMovement` 负责移动。两者分开，方便以后接网络联机。
- 移动参数：最高速度 5，加速 0.1 秒，减速 0.1 秒，斜向不会更快。三个值都能在 Inspector 里调。
- `PrototypeArena`：地面和四面看不见的墙自动贴合镜头可见范围，换窗口比例也不会走出画面。
- 菜单 `MMF/Build Prototype Scene`：自动生成 `Assets/_MMF/Scenes/Prototype.unity`，包括镜头、2D 全局光、地面、边界和 Player。

**审查**
- 代码符合 `AGENTS.md`：命名空间、目录、参数可调，`Update` 里没有 Find 或 GetComponent。
- `unity recompile`：0 错误 0 警告。
- 场景生成器连续运行两次都成功，场景里是 5 个根物体：Main Camera、Global Light 2D、Ground、Bounds、Player。Player 组件齐全，四面墙在画面外侧。
- 还没验证：实际按键移动的手感，要你试玩确认。

**改动文件**：`Assets/_MMF/**`（脚本、asmdef、输入资源、场景、占位贴图），以及 Unity 自动生成的 `ProjectSettings/SceneTemplateSettings.json`。

**怎么试玩**
1. 在 Unity 里打开 `Assets/_MMF/Scenes/Prototype.unity`（已经生成好，也可以重新运行菜单 `MMF → Build Prototype Scene`）。
2. 按 Play，先点一下 Game 窗口，再用 WASD 移动橙色方块。
3. 应该看到：可以 8 个方向移动，起步和停下有一点点顺滑感，走到画面边缘会被挡住。

**遗留问题**：见下面的补丁。

**补丁（同日）**：用户按 Play 后，Console 报错 `PlayerInputReader requires the MMFControls input asset.`，WASD 没有反应。
- 原因：第一次生成场景时，Player 身上的输入配置引用被存成了空值。生成器代码本身没问题，现在重新生成就正常了。推测是第一次生成时，输入资源还在被 Unity 导入，但没能完全确认。
- 处理：重新运行 `MMF/Build Prototype Scene`，已确认场景文件里引用正确，并提交了修好的场景。
- 如果以后又出现同样的报错，重新运行一次这个菜单就行。如果反复出现，再派 Codex 给生成器加一道检查。

---

## 2026-11-15 · T000 · 搭建工作流

**做了什么**
- 初始化 Git（只在本地），加了 Unity 用的 `.gitignore` / `.gitattributes`。
- `CLAUDE.md`：分工和审查规则（Claude 写文档、派任务、审查；Codex 写代码）。
- `AGENTS.md`：Codex 的代码规范、目录结构、禁区、回报格式。
- `docs/GDD.md`：原型设计 v0.1，以企划书为基础，补了详细玩法和初版数值。
- `docs/tasks/_TEMPLATE.md`：任务说明书模板。

**怎么试玩**：还没有可玩内容。

**下一步**：T001，项目骨架 + 自动生成原型场景 + 双人移动。
