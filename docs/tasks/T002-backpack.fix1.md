# T002 补丁 1 — 场景生成器有时把输入配置引用存成空值

> 给 Codex：先读 `AGENTS.md`。这是 T002 的补丁，只修下面这一个问题。

## 问题
反复运行 `MMF/Build Prototype Scene` 时，生成的场景里 `Player/PlayerInputReader._controls` **有时为空**（Claude 实测：连续运行 3 次，有 2 次为空；`ValidateReferences` 也报了 `Player/PlayerInputReader: missing _controls`）。为空时 Play 会报错，WASD、选格、Q 全部失效。T001 也出现过同样的问题。

## 原因（Claude 的判断）
`Build()` 在调用 `EditorSceneManager.NewScene(..., NewSceneMode.Single)` **之前**就用 `AssetDatabase.LoadAssetAtPath<InputActionAsset>` 加载了 `controls`。新建场景（Single）会关闭旧场景并卸载不再使用的资源。InputActionAsset 是 ScriptedImporter 生成的 ScriptableObject，被卸载后，手里那个 C# 引用就变成了已销毁的对象（Unity 的 "fake null"）。之后再把它赋给序列化字段，存下来的就是空值。

## 要做的事
1. 把**所有资源的加载**（`controls`、占位 sprite、物品资源、字体）都挪到 `NewScene` **之后**；或者至少在赋值前重新 `LoadAssetAtPath` 一次。原来的"缺资源就报错并退出"检查可以保留（可以先检查、新建场景后再重新加载一遍）。
2. 保存场景后，**从磁盘重新打开刚保存的场景**，再跑一遍 `ValidateReferences`。这样能确认存进文件的引用不是空的，而不只是内存里的。
3. 其他行为一律不改。

## 验收标准
- [ ] 能编译，无报错无警告
- [ ] 连续运行 `MMF/Build Prototype Scene` **5 次**，每次生成的场景里 `PlayerInputReader._controls` 都不为空，Console 没有 `missing` 报错
- [ ] 背包相关功能和 T002 一样，没有变化

## 范围限制
- 只改 `Assets/_MMF/Editor/PrototypeSceneBuilder.cs`。
- 不要动运行时脚本、输入资源、数值。
