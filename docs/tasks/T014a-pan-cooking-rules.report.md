## 完成情况

- ❌ Runtime、Editor 和 EditMode 测试程序集使用项目的 Unity 编译配置通过编译，零错误、零警告。新增 12 个模型用例在 Unity 外使用替身通过。场景连续生成三次、引用检查和 Unity Test Runner 全量测试尚未验证。
- ❌ 旧版 E 键和 WASD 的输入及会话代码保持原样，Play 实测尚未完成。
- ✅ 「煎锅」数值组已接入「每面煎几秒」和「可以翻以后几秒焦」，默认值为 3 秒和 5 秒。数值窗口的实际显示尚未核对。
- ❌ 模型测试覆盖了两面各煎 3 秒后翻面、满 8 秒变焦，以及取回和暂存。锅台组件的脚本调用和小方块颜色仍待 Play 验证。

## 改动文件

- [PanCookingModel.cs](Assets/_MMF/Scripts/Cooking/PanCookingModel.cs) — 新增两面计时、翻面、变焦、取回和暂存规则。
- [PanCooking.cs](Assets/_MMF/Scripts/Cooking/PanCooking.cs) — 每帧推进模型，并同步锅内物品与显示。
- [FryingStation.cs](Assets/_MMF/Scripts/Cooking/FryingStation.cs) — 在 `SetContents` 完成后发出内容变更通知。
- [CookingTuning.cs](Assets/_MMF/Scripts/Data/CookingTuning.cs) — 增加两个数值及其只读访问属性。
- [PanCookingModelTests.cs](Assets/_MMF/Tests/EditMode/PanCookingModelTests.cs) — 新增 12 个测试用例。
- [PrototypeSceneBuilder.cs](Assets/_MMF/Editor/PrototypeSceneBuilder.cs) — 为锅台添加组件并绑定锅台、数值和旧版交互引用。

## 疑问

无。

## 决定 / 假设

- 旧版写入锅内容时，新模型清除计时并同步为空锅或暂存状态。旧版会话进行期间，新版操作返回失败，避免两套规则同时修改锅内容。
- 每面开始时读取两个时长。跨过煎好阈值的帧会把剩余时间计入焦化倒计时。

## 用户需要在 Unity 里做的事

- 连续运行三次 **MMF → Build Prototype Scene**，检查锅台 `PanCooking` 的三个引用，再运行全部 EditMode 测试。
- 打开 **MMF → 数值**，检查「煎锅」组的两个新字段。进入 Play 后试玩旧版 E 键和 WASD，另由 Claude 用脚本验证新版翻面、变焦及小方块颜色。

## 已知问题 / 未完成

- 自动审批拒绝了 Unity 界面操作，返回 `Computer Use was not approved to use Unity`。`Prototype.unity` 尚未由生成器更新，编辑器内验收待完成。