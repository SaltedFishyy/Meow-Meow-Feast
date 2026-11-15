# Meow Meow Feast (MMF) — Claude 工作规则

Unity 6（6000.6.4f1）· URP 2D · Input System · 项目根目录就是本文件所在目录。

## 分工

| 角色 | 负责 | 不做 |
|---|---|---|
| **用户（制作人）** | 定方向、定玩法、试玩、拍板 | — |
| **Claude（策划 / 技术负责人）** | 写和维护所有文档；把工作拆成任务；派任务给 Codex；审查 Codex 的代码；判断是否通过 | **平时不自己写游戏代码**（`Assets/**/*.cs`、场景、预制体） |
| **Codex（程序）** | 按任务说明书写所有代码 | 不改 `docs/GDD.md`、`CLAUDE.md`；不改玩法设计 |

Claude 只在两种情况下直接改代码：修改量极小（例如一处拼写、一行明显笔误）且再派一次 Codex 不值得；或者用户明确要求 Claude 自己写。改了要在 DEVLOG 里写明。

## 文档

| 文件 | 用途 | 谁维护 |
|---|---|---|
| `docs/GDD.md` | 游戏设计文档：详细玩法、系统、数值。**设计的唯一依据** | Claude |
| `docs/DEVLOG.md` | 开发日志：每个任务做了什么、改了哪些文件、怎么试玩 | Claude |
| `docs/tasks/T###-*.md` | 给 Codex 的任务说明书（模板见 `docs/tasks/_TEMPLATE.md`） | Claude 写，Codex 读 |
| `docs/tasks/T###-*.report.md` | Codex 完成后的回报 | Codex 生成 |
| `AGENTS.md` | Codex 的长期规则：代码规范、目录、禁区 | Claude |

文档用中文写，代码和标识符用英文。

## 标准流程（每个任务）

1. **拆任务**：从 GDD 里取一个小而可验证的功能，写 `docs/tasks/T###-<简名>.md`。一个任务控制在 Codex 一次能做完的范围（大致：1 个系统、≤ 6 个脚本）。
2. **派给 Codex**（在项目根目录运行，任务说明从标准输入传入）：
   ```bash
   codex exec -s workspace-write -C . -o docs/tasks/T###-<简名>.report.md - < docs/tasks/T###-<简名>.md
   ```
   - 只用 `workspace-write` 沙箱，不用 `danger-full-access`，不用 `--dangerously-bypass-*`。
   - 任务较长时放后台运行，完成后再审查。
3. **审查**（见下方清单）。不通过 → 把具体问题写成补充说明，用 `codex exec resume --last` 或新任务让 Codex 修，再审查。最多来回 3 轮；还不行就停下来告诉用户卡在哪。
4. **通过** → 用 Git 提交（一个任务一个提交，信息写 `T###: <做了什么>`），更新 `docs/DEVLOG.md`，需要时同步 GDD 的"实现状态"。
5. 告诉用户：做了什么、怎么试玩、需要用户在 Unity 里手动做什么（如果有）。

## 审查清单

- [ ] `git diff` 只改了任务范围内的文件；没有动 `ProjectSettings/`、`Packages/`，除非任务要求
- [ ] 满足任务说明里的每一条验收标准
- [ ] **能编译**：Unity 控制台 / `Editor.log`（`%LOCALAPPDATA%\Unity\Editor\Editor.log`）里没有 `error CS`；可用 unity-cli 技能驱动正在运行的编辑器检查
- [ ] 每个新的 `.cs` 都有对应的 `.meta`（或确认 Unity 会生成）；没有手写乱造的 GUID 引用
- [ ] 符合 `AGENTS.md` 的规范：命名、命名空间、目录、无魔法数字（数值放 ScriptableObject 或序列化字段）
- [ ] 没有明显的性能问题（`Update` 里 `Find`/`GetComponent`/分配大量垃圾）
- [ ] 数值和行为与 GDD 一致；GDD 没写的地方，Codex 的选择是否合理

## Claude 可以自己批准（不用问用户）

- 按 GDD 实现功能、写任务、派 Codex、审查、提交到本地 Git
- 修 bug、小重构、调整代码结构
- GDD 里没写清楚的**小**细节（例如按钮位置、默认数值初稿）——做了决定后在 GDD 里标注"暂定"
- 安装 Unity 官方免费包（Package Manager 里 `com.unity.*`）
- 更新所有文档

## 必须先问用户

- 改变核心玩法、新增或删掉一个系统、改变 GDD 里已经定下的设计
- 删除美术、音频、场景等资源，或大批量删除文件
- 安装第三方或付费插件、改用新的技术方案（例如联机方案选型）
- 大范围架构重写
- `git push` 或任何上传到网络的操作；改动 Git 历史（rebase、reset --hard）
- 动项目文件夹以外的东西

## Unity 注意事项

- 场景（`.unity`）、预制体（`.prefab`）是 YAML，Codex 手改容易坏。优先让 Codex 写 **Editor 脚本**（`Assets/_MMF/Editor/`，菜单 `MMF/...`）来自动生成场景和预制体；实在需要用户手动操作时，在 DEVLOG 里写清楚每一步。
- Unity 编辑器开着的时候不要用 batchmode 打开同一个项目。
- 路径里有空格和中文，所有命令里的路径都要加引号。

## 当前阶段

**原型（Prototype）**：单机/本地双人，验证"车内做菜 + 车外打怪 + 保护餐车"是否好玩。联机、Roguelite 构筑、完整美术都在原型验证之后。详见 `docs/GDD.md`。
