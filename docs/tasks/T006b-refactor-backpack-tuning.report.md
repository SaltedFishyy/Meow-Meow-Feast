## 完成情况

- ❌ 完整验收未完成：Runtime、Editor 命令行编译通过，无报错无警告；场景生成 3 次及原有 93 个测试未执行。
- ❌ 单一资源及重新生成不覆盖：已实现“存在即返回”，尚未在 Unity 验证。
- ❌ 背包试玩：默认值保持 5、3、0.5、2、60、1，尚未实测。
- ❌ 怪物掉落试玩：散开半径默认 0.5，共用 60 秒寿命，尚未实测。
- ❌ Play 中延迟改为 5：代码在每次丢弃时读取，尚未实测。
- ✅ `PlayerBackpack` 六个旧字段、`MonsterDrops` 两个旧字段已移除。

## 改动文件

- `Assets/_MMF/Scripts/Data/BackpackTuning.cs` — 新增六项数值和只读属性；两项开局数值注明“下次 Play 生效”。
- `Assets/_MMF/Scripts/Player/PlayerBackpack.cs` — 改从数值资源读取。
- `Assets/_MMF/Scripts/Combat/MonsterDrops.cs` — 寿命读取共享资源，散开半径读取掉落表。
- `Assets/_MMF/Scripts/Data/DropTable.cs` — 新增散开半径及只读属性。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 创建或加载资源并填写引用，删除旧字段设置。

## 疑问（需要确认，没有就写"无"）

无。

## 决定 / 假设（仅限技术层面）

- 沿用 `PlayerTuning` 的资源加载方式。
- 保留原文件换行、编码及未涉及代码；`WorldItem` 未改。

## 用户需要在 Unity 里做的事

- 运行 `MMF/Build Prototype Scene` 三次，并检查修改数值后重新生成不会覆盖。
- 运行原有 93 个测试，再按说明书试玩；开始 Play 前必须先生成场景，填入新引用。

## 已知问题 / 未完成

- `BackpackTuning.asset` 和 `Prototype.unity` 尚未通过生成器创建／更新。
- `git diff --check` 通过；命令行编译不能替代 Unity 内完整验收。

自动审批拒绝了 Unity 窗口访问，原因为“Computer Use was not approved to use Unity”，因此未能执行场景生成、测试及试玩。