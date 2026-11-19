## 完成情况

- ❌ 运行时、编辑器和 EditMode 测试程序集均用 Unity 自带的 C# 编译器编译通过，无报错或警告。场景连续生成 3 次、引用检查及 84 项测试运行尚未验证。
- ❌ 已实现煎熟食物命中粉色顾客后被消耗。顾客立即停止移动和攻击，默认用 1 秒变白并消失，随后释放出生名额。待 Play 验证。
- ❌ 生蘑菇、焦土和平底锅命中未进食的顾客时走原有弹开逻辑。待 Play 验证。
- ❌ 顾客进食后立即关闭碰撞和受击，玩家、其他怪物及投掷物可穿过。待 Play 验证。
- ❌ 煎熟食物命中普通紫色团子时走原有弹开逻辑。待 Play 验证。
- ❌ 已在「顾客」数值组加入「吃完几秒消失」，默认 1 秒。待打开数值窗口确认显示。

## 改动文件

- `Assets/_MMF/Scripts/Data/ItemDefinition.cs` — 新增只读的喂食开关，默认关闭。
- `Assets/_MMF/Scripts/Data/CustomerTuning.cs` — 新增进食后消失的时长。
- `Assets/_MMF/Scripts/Combat/Customer.cs` — 实现一次性喂食、停止攻击和碰撞、渐变至白色及净化事件。
- `Assets/_MMF/Scripts/Combat/CustomerSpawner.cs` — 净化和死亡共用清理路径。
- `Assets/_MMF/Scripts/Player/ThrownItem.cs` — 在扫掠命中时尝试喂食，成功后消耗食物。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 为四种煎熟食物设置开关，包括已存在的物品资源，并配置顾客模板引用。

## 疑问（需要确认，没有就写“无”）

无。

## 决定 / 假设（仅限技术层面）

- 喂食时停用现有怪物行为组件，取消正在蓄力的攻击。净化完成时由生成器释放名额并销毁顾客。

## 用户需要在 Unity 里做的事

1. 连续运行 3 次 `MMF → Build Prototype Scene`，更新 `Prototype.unity` 和四种 `Fried*.asset`，检查顾客模板及其数值、怪物定义引用。
2. 在 Test Runner 中运行全部 84 项测试，在 `MMF → 数值` 中确认「吃完几秒消失」。
3. Play 后打团子捡蘑菇或怪肉，进车拖进锅煎熟。出车选中煎熟食物，按住右键朝粉色顾客蓄力，松开投掷。观察食物消耗、顾客停下变白并消失，以及后续补位。
4. 复测生蘑菇、焦土和平底锅的弹开，进食后顾客的穿透和免受击，以及煎熟食物命中紫色团子的弹开。

## 已知问题 / 未完成

- Unity 窗口操作被 Computer Use 权限拒绝，当前场景和物品资源尚未重新生成，Editor 测试及试玩尚未执行。请先重新生成场景再按 Play。