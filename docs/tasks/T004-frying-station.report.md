## 完成情况

- ❌ 编译与验证：使用本机 Unity 引用程序集编译，零错误、零警告；11 项 NUnit 测试通过（旧 4 项＋新 7 项）。Unity 内连续生成三次、引用检查和 Test Runner 尚未实测。
- ✅ 已实现左上角 1×1 煎锅台及阻挡碰撞，由生成菜单创建。
- ✅ 已实现 E 放入食物、右侧画面、字母和倒计时，以及移动、选格、Q 锁定。
- ✅ 已实现正确按键推进、错误无惩罚、两轮煎制、翻面动画和成品入包。
- ✅ 已实现超时变焦土。
- ✅ 已实现 E／Esc 退出、清空进度、保留生食及空手取回。
- ✅ 已实现替换、溢出掉落、完成时背包满则留锅，以及头顶红色 x。
- ✅ 已实现举起、携带、整数格放置及车内／碰撞检查。
- ✅ 暂定数值已暴露到 Inspector；物品煎制结果和锅背景 Sprite 可配置。

以上玩法项已完成代码实现，仍待 Play 验收。

## 改动文件

路径均相对于 `Assets/_MMF/`：

- `Scripts/Cooking/FryingSequence.cs` — 新增纯 C# 煎制规则。
- `Scripts/Cooking/FryingStation.cs` — 新增锅内物品、参数及携带碰撞控制。
- `Scripts/Player/PlayerInteractor.cs` — 新增交互、替换、结果回收与搬运。
- `Scripts/UI/FryingPanel.cs` — 新增右侧画面与翻面动画。
- `Tests/EditMode/FryingSequenceTests.cs` — 新增 7 项测试。
- `Scripts/Data/ItemDefinition.cs` — 增加煎制结果引用。
- `Scripts/Player/PlayerBackpack.cs` — 增加入包、取出、溢出掉落及 x 表情接口。
- `Scripts/Player/PlayerMovement.cs` — 增加移动锁定和最后移动方向。
- `Scripts/Player/PlayerInputReader.cs` — 转发新增输入并限制操作。
- `Input/MMFControls.inputactions` — 新增交互、取消、举起和煎制动作。
- `Editor/PrototypeSceneBuilder.cs` — 生成锅台、UI、三种新物品及占位图片，沿用保存后重开检查流程。

无删除脚本；临时验证文件已清理。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

- 放锅方向采用四方向；斜向移动按较大分量取向，相等时取纵向。
- 翻面动画结束后开始第二轮计时。
- 新增共 5 个脚本，未手写 Unity YAML 或 `.meta`。

## 用户需要在 Unity 里做的事

- 等待脚本导入，连续运行三次 `MMF → Build Prototype Scene`。
- 在 Test Runner 中运行 EditMode 测试。
- 按任务提供的步骤进行 Play 验收。

## 已知问题 / 未完成

Unity 桌面连接不可用，无法实际操作 Editor。新资源和场景尚未生成，需运行上述菜单；碰撞、UI 布局与完整输入流程尚未经过 Play 实测。