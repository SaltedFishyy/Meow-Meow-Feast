## 完成情况
- ✅ 编译通过：`dotnet build MMF.Editor.csproj --no-restore`，0 错误、0 警告。
- ❌ 连续生成 5 次：尚未在 Unity 中实测；已修复加载顺序，并加入保存后重新打开场景的引用校验。
- ✅ 背包相关代码、输入配置及数值未改动；功能回归尚未实测。

## 改动文件
- [PrototypeSceneBuilder.cs](</C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Editor/PrototypeSceneBuilder.cs>) — 所有资源均在 `NewScene` 后加载；保存后通过 `OpenScene` 重新读取，再运行 `ValidateReferences`。

## 疑问（需要确认，没有就写"无"）
无。

## 决定 / 假设（仅限技术层面）
- 保留保存前校验，新增保存后校验；缺资源检查在新建场景后执行。

## 用户需要在 Unity 里做的事
- 连续运行 `MMF/Build Prototype Scene` 5 次，每次确认 `_controls` 非空且 Console 无 `missing`。
- 按 Play 验证 WASD、选格、Q 和背包功能。

## 已知问题 / 未完成
- Unity 内的五次生成验证及玩法回归尚未完成。