## 完成情况

- ❌ 编译检查通过（0 警告、0 错误）；场景生成 3 次及原有 76 个测试未执行。
- ❌ 第 1 格灰色“平底锅”的配置已实现，未进行 Play 验证。
- ❌ 丢弃时使用无限寿命，保留 2 秒拾取延迟；未实测超过 60 秒。
- ✅ 其他物品仍使用原有 60 秒寿命，默认开关关闭。
- ✅ 平底锅无煎制结果，静态确认现有逻辑会显示“x”，不放入锅台。
- ✅ 开局物品使用现有 `TryAdd`，其他物品继续按原规则入包。

## 改动文件

- `Assets/_MMF/Scripts/Data/ItemDefinition.cs` — 增加不会消失的序列化开关及只读属性。
- `Assets/_MMF/Scripts/Player/PlayerBackpack.cs` — 增加开局物品列表，丢弃时按开关选择寿命。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 生成并校正平底锅资源，设为唯一开局物品。

## 疑问（需要确认，没有就写"无"）

无。

## 决定 / 假设（仅限技术层面）

- 使用 `float.PositiveInfinity` 表示无限寿命；不修改 `WorldItem`。
- 保留原始 LF 换行及无 BOM 编码；`git diff --check` 通过。

## 用户需要在 Unity 里做的事

- 运行 `MMF → Build Prototype Scene` 三次，执行原有 76 个测试。
- Play 后验证初始背包、Q 丢弃与拾回、超过一分钟不消失，以及锅台 E 显示“x”。

## 已知问题 / 未完成

- `FryingPan.asset` 和 `Prototype.unity` 尚未通过生成器生成／更新。

Unity 界面操作被工具拒绝，返回“Computer Use was not approved to use Unity”，因此未能继续执行生成、Unity 测试及试玩验收。