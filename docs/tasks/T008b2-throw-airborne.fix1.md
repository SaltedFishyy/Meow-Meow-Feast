# T008b2 补充 1 — 去掉升高和影子，改成贴地匀速平移

> 给 Codex：T008b2 你刚写完的改动还没提交（工作区里）。用户试玩后改了设计（见 `docs/GDD.md` §背包"右键蓄力丢"）：
> **保留"先匀速走一段、再越滑越慢"的过程，但这一段不升高、没有影子，东西一直贴着地面平移。**

## 要做的事
1. **删掉** `Assets/_MMF/Scripts/Player/ThrownItemVisual.cs` 和它的 `.meta`（整个影子 / 高度显示都不要了）。
2. `ThrownItem`：
   - 去掉 `[RequireComponent(typeof(ThrownItemVisual))]` 和对它的调用。
   - 显示改回 T008b1 的做法：自己的 `SpriteRenderer` 设好贴图、颜色、大小（`transform.localScale`），`_halfSize` 用 renderer 的大小算。
   - 匀速段（现在叫空中阶段）的移动、反弹、计时、转入滑行**都保留不变**。
   - 现在只给显示脚本用的公开属性（`IsAirborne`、`AirProgress`、`AirTime` 等），如果已经没人用了就删掉或改成私有；名字里的 "Air" 可以改成更贴切的说法（例如 `Glide`），你决定，回报里写清楚。
3. `ThrowTuning`：两个字段的中文名改成 `没蓄力时匀速平移几秒`、`蓄满时匀速平移几秒`。**字段名、默认值不变**（避免已有的数值文件丢值）。
4. 其他文件不动。

## 验收标准
- [ ] 编译无报错无警告；93 个测试全部通过
- [ ] 扔出去的东西从头到尾贴着地面，没有影子、不升高
- [ ] 蓄满时先匀速走约 0.5 秒再越滑越慢；距离和补充之前完全一样
- [ ] 没有用不到的代码留下（显示脚本、没人用的属性）

## 要改的文件（只能动这些）
| 文件 | 新增 / 修改 | 改哪部分 |
|---|---|---|
| `Assets/_MMF/Scripts/Player/ThrownItemVisual.cs`（+ `.meta`） | 删除 | — |
| `Assets/_MMF/Scripts/Player/ThrownItem.cs` | 修改 | 只改显示和不再需要的属性 |
| `Assets/_MMF/Scripts/Data/ThrowTuning.cs` | 修改 | 只改两个中文名 |

## 范围限制
- 只动上面清单里的文件；不改换行符（LF）、不加 / 删 BOM
- 有不清楚的地方写进回报的"疑问"，不要自己决定
