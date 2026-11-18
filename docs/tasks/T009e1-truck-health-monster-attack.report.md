## 完成情况

- ❌ 运行时、编辑器和 EditMode 测试程序集均已独立编译，无报错或警告。Unity 窗口控制请求被拒绝，三次场景生成、引用检查和 93 个测试的运行结果仍待验证。
- ✅ 已实现餐车开局 100 血。怪物在分配到的车头或车尾复用现有的蓄力、闪烁、扑击和冷却，默认命中扣 1 血。
- ✅ 打车要求怪物中心位于指定端墙外，且高度在端墙范围内。绕经上下车门时不会触发打车。
- ✅ 玩家靠近时，已开始的打车攻击继续，冷却结束后恢复追玩家优先。
- ✅ 车血最低为 0。归零后，怪物取消尚未扑出的打车蓄力，也不会再次开始打车。
- ✅ 已注册数值窗口的「餐车」组，以及「怪物」组的「打车每下伤害」。窗口显示待 Unity 验证。
- ❌ 原有打玩家的命中、扑空和碰撞移动逻辑已保留，T009a～d 的完整回归仍待验证。

## 改动文件

- `Assets/_MMF/Scripts/Data/TruckTuning.cs` — 新增餐车最大血量数值。
- `Assets/_MMF/Scripts/Data/MonsterDefinition.cs` — 新增打车伤害数值。
- `Assets/_MMF/Scripts/Truck/TruckHealth.cs` — 新增餐车血量、扣血及 `Changed` 事件。
- `Assets/_MMF/Scripts/Combat/MonsterTargeting.cs` — 暴露追玩家状态和分配的车头或车尾。
- `Assets/_MMF/Scripts/Combat/MonsterAttack.cs` — 共用攻击流程，移除重复的追踪判断。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 接好餐车和怪物的引用，仅在数值资产缺失时创建。

## 疑问（需要确认，没有就写"无"）

- 无。

## 决定 / 假设（仅限技术层面）

- 开局读取最大血量，编辑后下次 Play 生效。打车伤害在命中时读取。
- `Changed` 会报告归零时的血量变化。专用归零事件留待 T009f 接入时添加。

## 用户需要在 Unity 里做的事

- 连续运行三次 `MMF → Build Prototype Scene`，检查车的数值引用和五只怪物的餐车血量引用，再用 Test Runner 运行全部 93 个测试。
- 按 Play 后留在车内，观察怪物绕过车门并在车头或车尾闪烁、水平扑击。可用脚本读取 `TruckHealth.Current` 核对扣血。再在怪物闪烁时出车，验证它先完成打车，冷却后再追玩家。

## 已知问题 / 未完成

- `Prototype.unity` 和 `TruckTuning.asset` 仍待 Unity 生成器更新或创建。场景引用和运行时行为尚未实测。