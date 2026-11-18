## 完成情况

- ❌ Unity 自带的 C# 编译器已编译运行时、编辑器和 EditMode 测试程序集，零错误、零警告。场景生成 3 次、引用检查和 93 个测试仍待在编辑器内验证。
- ✅ 车外血量归零时，立即停止玩家操作，保留致命一击的弹开。怪物停止追击和攻击，转向车头或车尾。已实现，待试玩。
- ✅ 默认 3 秒后在车正中间复活，速度归零，按当时的最大血量回满并刷新 HUD，恢复操作及受击无敌。倒地期间暂停背包自动拾取。已实现，待试玩。
- ✅ 倒地时取消投掷蓄力，箭头随蓄力结束而消失，物品留在背包里。正在煎东西时走原有取消流程。已实现，待试玩。
- ✅ 举着锅台倒地时，先用现有规则检查倒地格。车外格不符合放置条件，锅台回到本次举起前的位置。已实现，待试玩。
- ✅ 复活后出车，怪物按原有距离规则恢复追击和攻击。已实现，待试玩。
- ✅ 「玩家血量」组已新增「倒地几秒」，默认 3 秒。Play 中修改后，下次倒地读取新值。数值窗口待实机确认。
- ❌ 非倒地流程沿用原有逻辑，回归测试待执行。

## 改动文件

- `Assets/_MMF/Scripts/Data/PlayerHealthTuning.cs` — 新增倒地时长数值。
- `Assets/_MMF/Scripts/Player/PlayerHealth.cs` — 增加倒地状态、倒地与复活事件，以及满血复活。
- `Assets/_MMF/Scripts/Player/PlayerDowned.cs` — 接管倒地期间的操作限制、计时和复活位置。
- `Assets/_MMF/Scripts/Player/PlayerInteractor.cs` — 记录锅台举起前的位置，复用格子放置检查。
- `Assets/_MMF/Scripts/Player/PlayerHitReaction.cs` — 保留致命一击的弹开，调整倒地期间的闪烁和怪物碰撞。
- `Assets/_MMF/Scripts/Combat/MonsterTargeting.cs` — 将倒地玩家视为不可追击。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 添加倒地组件及引用接线。

## 疑问

- 无。

## 决定 / 假设

- 计时放在 `PlayerDowned`，每次倒地时读取时长。若时长短于弹开过程，等弹开结束再复活。
- 致命一击后倒地期间不闪烁。弹开期间保留穿怪，如果仍与怪物重叠则延续到分离。复活时启动常规受击无敌。
- 锅台沿用现有碰撞检查，玩家占住倒地格时也会触发原位回退。

## 用户需要在 Unity 里做的事

- 等待 Unity 导入新脚本，连续运行三次 **MMF → Build Prototype Scene**，检查玩家和怪物引用，再在 Test Runner 运行全部 93 个测试。
- Play 后分别验证车外倒地、蓄力时倒地和举锅台倒地。复活后再次出车，确认怪物恢复追击和攻击。

## 已知问题 / 未完成

- Computer Use 未获准访问 Unity 窗口，Unity CLI 也未找到可连接的 Pipeline 实例。`Prototype.unity` 尚未由生成器更新，Unity 内的引用校验、测试和试玩尚未执行。