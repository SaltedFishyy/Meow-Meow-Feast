## 完成情况

- ❌ Runtime、Editor 和 EditMode 测试程序集使用项目的 Unity 编译配置通过编译，零错误、零警告。场景连续生成三次、引用检查和 105 项测试仍待验收。
- ❌ 已接入 `FryingStation.InRange`，玩家靠近时显示小锅，走远时隐藏。往返显示待 Play 验证。
- ❌ 已实现熟度进度环、「翻！」与食物脉动、焦前闪红冒烟，以及两次翻面动画。蘑菇的完整流程待 Claude 用 `TryPlace` / `TryFlip` 验证。
- ❌ 世界空间 Canvas 挂在锅台下并绑定主镜头，搬运时的跟随效果待 Play 验证。
- ❌ 旧版 E 键和 WASD 待 Play 回归验证。
- ❌ 已在「煎锅」数值组添加「快焦提示提前几秒」，默认 2 秒。数值窗口的显示待检查。

## 改动文件

- [PanCookingView.cs](<C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Scripts/UI/PanCookingView.cs>) — 新增小锅显示、距离判断和四种提示。
- [CookingTuning.cs](<C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Scripts/Data/CookingTuning.cs>) — 添加快焦提示时长。
- [PrototypeSceneBuilder.cs](<C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Editor/PrototypeSceneBuilder.cs>) — 生成世界空间 Canvas、显示元素和引用。

## 疑问（需要确认，没有就写“无”）

- `ReadySecond → Empty` 同时用于第二次翻面和直接取回。现有 `StateChanged` 无法区分原因，画面会在两种情况下播放翻面动画。接入 T014c 的取回操作时，需要确认是否增加事件信息来区分。

## 决定 / 假设（仅限技术层面）

- 用一个视图脚本管理显隐和提示。隐藏的是 Canvas，锅台上的模型继续计时。
- 沿用旧版锅图、背包栏字体和煎制结果颜色。模型清空时，视图暂存食物引用，让第二次翻面播完再清除旧画面。

## 用户需要在 Unity 里做的事

- 连续运行三次 **MMF → Build Prototype Scene**，检查新视图的引用，并运行全部 105 项测试。
- 打开 **MMF → 数值**，检查「快焦提示提前几秒」。进入 Play，靠近、离开并再次靠近车内左上角的锅台，再检查搬运跟随和旧版 E 键、WASD。
- 由 Claude 用 script eval 放入蘑菇，分别验证两次翻面和等到变焦的画面。

## 已知问题 / 未完成

- 自动审批拒绝了 Unity 界面操作（`Computer Use was not approved to use Unity`）。`Prototype.unity` 尚未由生成器更新，编辑器内验收仍待完成。
## 审查修正

- 审查已确认进度环、「翻！」提示、翻面变深和快焦闪红冒烟正常。
- [PanCookingView.cs](../../Assets/_MMF/Scripts/UI/PanCookingView.cs) 在镜头更新后按当帧的可见范围定位。默认仍在锅台正上方 2 格，画面为 3×3 世界单位。超出边缘时，只在越界的轴上移到刚好放得下。背包栏顶边作为可用区域下界。可用宽度或高度不足时，等比缩小到刚好放下。定位过程每帧复用引用，只使用值类型。
- [PrototypeSceneBuilder.cs](../../Assets/_MMF/Editor/PrototypeSceneBuilder.cs) 把背包栏引用接入小锅视图，并为小锅 Canvas 配置 `dynamicPixelsPerUnit = 4`，提高「翻！」的字形栅格分辨率，保留原字号和布局。
- 三个 Unity 程序集离线编译通过，零错误、零警告。定位公式验证覆盖了顶部越界、空间充足、右边缘与背包栏、窄画面、镜头拉近和镜头移动六种情况。
- 本轮场景未重新生成。运行 **MMF → Build Prototype Scene** 后进入 Play，检查车内左上角、搬运经过镜头边缘和进出车缩放，并在可翻面时确认「翻！」的清晰度。
- 直接取回触发翻面动画的问题留到 T014c。
