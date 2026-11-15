## 完成情况

- ❌ 完整编译与生成验收：包含新增脚本的 Runtime、Editor 编译通过，0 错误、0 警告；场景生成 3 次及现有 27 项测试尚未执行。
- ✅ 已实现 5 只紫色方块、直线移动、碰撞停止及门洞阻挡；待 Play 验证。
- ✅ 已接入玩家忽略门洞挡板，怪物保留实体碰撞；待试玩验证。
- ✅ 已实现 `TakeHit(int damage, Vector2 direction)`、闪白、击退、死亡事件及原点重生；待运行验证。
- ✅ 所有标黄数值均放入 `MonsterDefinition`。
- ❌ 未新增建议的 EditMode 测试，测试文件不在允许修改列表内。

## 改动文件

- `Assets/_MMF/Scripts/Data/MonsterDefinition.cs` — 新增怪物数值 SO。
- `Assets/_MMF/Scripts/Combat/Monster.cs` — 新增移动、受伤、死亡逻辑。
- `Assets/_MMF/Scripts/Combat/MonsterSpawner.cs` — 新增重生计时与玩家碰撞忽略。
- `Assets/_MMF/Scripts/Truck/TruckArea.cs` — 增加门洞挡板布局。
- `Assets/_MMF/Editor/PrototypeSceneBuilder.cs` — 生成数据、出生点、怪物和挡板并接线。

## 疑问（需要确认，没有就写“无”）

- 无设计疑问。若需新增持久化测试，需要允许修改测试文件。

## 决定 / 假设（仅限技术层面）

- 使用运动学刚体与碰撞扫掠限制移动、击退。
- 复用隐藏的怪物对象完成重生。
- 重建场景保留已有怪物数据资源的 Inspector 调整。

## 用户需要在 Unity 里做的事

- 等待脚本导入，运行 `MMF → Build Prototype Scene` 三次。
- 运行现有 EditMode 测试，再按 Play 检查移动、门洞和玩家碰撞。
- 通过代码调用 `TakeHit(1, direction)` 三次，检查死亡及约 20 秒后的重生。

## 已知问题 / 未完成

- Unity 控制工具返回“Computer Use was not approved to use Unity”，因此未生成 `CursedDumpling.asset`、未重建 `Prototype.unity`，也未完成运行验收。运行生成菜单会创建这些内容。