## 完成情况

以下 ✅ 表示已实现；Unity 试玩尚未验收。

- ❌ 完整验证：独立编译通过，无新增警告；场景生成三次、引用检查和原有 93 项测试未执行。
- ✅ 箭头按蓄力进度从 1 到 3；保留原有取消与蓄力条件。
- ✅ 蓄满出手速度 12，每次移除选中格 1 个；摩擦积分校验距离约 6 格。
- ✅ 短按出手速度约 5；摩擦积分校验距离约 1.0417 格。
- ✅ 连续检测墙面碰撞、按法线反射并衰减速度；9 个几何用例及斜向反射检查通过。
- ✅ 仅检测餐车墙和地图墙，门口保持通路。
- ✅ 不检测怪物、玩家、锅台、门口怪物阻挡墙或其他物品；没有添加物理碰撞体。
- ✅ 滑行中不注册拾取；停下生成普通掉落物，重新开始计时，保留平底锅不消失规则。
- ✅ 松手时选中空格，只结束蓄力。
- ✅ Q 仍使用原位置、原拾取延迟和原寿命。
- ✅ 数值组改为蓄力时间、两个速度、摩擦和反弹；摩擦与反弹实时读取。

## 改动文件

- `Assets/_MMF/Scripts/Data/ThrowTuning.cs` — 删除距离字段，新增速度、摩擦、反弹。
- `Assets/_MMF/Scripts/Player/PlayerThrow.cs` — 计算蓄力与速度，释放时投掷一个物品。
- `Assets/_MMF/Scripts/Player/ThrowArrow.cs` — 新增外观长度字段，按蓄力调整长度。
- `Assets/_MMF/Scripts/Player/ThrownItem.cs` — 新增滑行、碰撞反弹和停止转掉落物。
- `Assets/_MMF/Scripts/Player/ThrownItem.cs.meta` — Unity 自动生成。
- `Assets/_MMF/Scripts/Player/PlayerBackpack.cs` — 扩展指定位置的掉落物创建入口，复用外观与计时规则。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 填入十段墙体引用。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

- 用物品显示半尺寸扩张墙体包围盒，再计算连续射线交点和法线；同帧可处理多次反弹。
- 每帧最多处理 32 次碰撞，达到上限后舍弃剩余时间，避免穿墙。
- 保留已有文件换行符及 BOM；新脚本使用 LF、无 BOM。原 `ThrowArrow.cs` 实际为 CRLF，未转换。

## 用户需要在 Unity 里做的事

- 连续运行三次 `MMF/Build Prototype Scene`，再运行原有 93 项测试。
- Play 后测试蓄力投掷、撞墙反弹、穿门，以及停下 2 秒后的自动拾取。

## 已知问题 / 未完成

- `Prototype.unity` 尚未重新生成，**需先运行生成菜单才能填入墙体引用**。
- Unity 试玩未执行；独立编译出现两条来自未改动文件的既有 `CS0649` 警告。

自动批准检查拒绝了 Unity 界面操作，返回“Computer Use was not approved to use Unity”，因此无法代跑场景生成和 Test Runner。