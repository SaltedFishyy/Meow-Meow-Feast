## 完成情况

- ❌ 编译无警告、93 项测试通过：未能验证，Unity 窗口操作权限被工具拒绝。
- ✅ 已移除升高和影子显示，恢复自身 SpriteRenderer 的贴图、颜色、缩放及碰撞尺寸计算。
- ✅ 保留匀速阶段、反弹、计时和减速逻辑；满蓄力默认仍为 0.5 秒，实际距离待试玩确认。
- ✅ 删除显示脚本和无用公开属性；修改文件保持 LF、无 BOM，差异空白检查通过。

## 改动文件

- `Assets/_MMF/Scripts/Player/ThrownItemVisual.cs` — 删除。
- `Assets/_MMF/Scripts/Player/ThrownItemVisual.cs.meta` — 删除；删除前确认无资产引用。
- `Assets/_MMF/Scripts/Player/ThrownItem.cs` — 恢复贴地显示，清理显示专用接口。
- `Assets/_MMF/Scripts/Data/ThrowTuning.cs` — 仅修改两个中文标签，字段名和默认值不变。

## 疑问（需要确认，没有就写"无"）

无。

## 决定 / 假设（仅限技术层面）

- 内部阶段改名为 `_glideTime`、`_glideElapsed` 和私有 `IsGliding`；删除 `AirProgress`。
- 保留初始化参数 `airTime`，兼容现有调用。

## 用户需要在 Unity 里做的事

- 等待编译，检查 Console，运行全部 93 项测试。
- 试玩轻按、满蓄力及撞墙，确认贴地显示和投掷距离。

## 已知问题 / 未完成

- 编辑器编译、测试和视觉验收未完成。工具拒绝操作 Unity，提示：`Computer Use was not approved to use Unity`。