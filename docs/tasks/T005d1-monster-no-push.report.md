## 完成情况

- ✅ Runtime、Editor、EditMode 测试程序集编译通过：0 错误、0 警告。
- ❌ 场景生成 3 次及原有 93 个测试：尚未执行。
- ❌ 单怪静止、三怪包围及方向输入：尚未实测位置变化。
- ❌ 玩家不能穿怪：保留实体碰撞，尚未实测。
- ❌ 车头／车尾聚集、迎面避让：原逻辑保留，尚未实测回归。

## 改动文件

- [Monster.cs](</C:/做饭游戏 MMF/Meow Meow Feast/Assets/_MMF/Scripts/Combat/Monster.cs:134>) — 在最终移动前增加针对 Dynamic 刚体的安全间隙检查，覆盖直行、滑动、击退和避让。保留 BOM、CRLF；未改其他文件。

## 疑问（需要确认，没有就写“无”）

- 无设计疑问。

## 决定 / 假设（仅限技术层面）

- 源码判断：原间隙 `0.002` 小于双方 contact offset 之和，Kinematic 怪物仍可能触发接触约束，将 Dynamic 玩家挤走。[Unity 官方说明](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/physics2d/defaultcontactoffset)
- 使用双方 offset 加原间隙作为安全余量，仅限制靠近 Dynamic 刚体的移动；复用查询数组，无新增逐帧组件查找。
- **上述原因尚未通过 Unity 对照实验确认。**

## 用户需要在 Unity 里做的事

- 运行 `MMF → Build Prototype Scene`，完成三次生成和 93 项测试。
- Play 后出车站定，让怪追上；再往空方向走，并复验单怪、三怪及原有绕车避让场景。

## 已知问题 / 未完成

- 修复已编译通过，但运行时验收未完成，不能宣称 bug 已验证解决。

工具的应用访问审批拒绝了 Unity 窗口访问，返回 “Computer Use was not approved to use Unity”，因此未能执行编辑器验证。