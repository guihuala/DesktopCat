# 3D Cat Desktop Pet Framework Plan

## 1. 项目定位

本项目计划开发为一款 3D 猫猫桌宠游戏。玩家在桌面上饲养一只具有丰富状态的猫，猫会响应玩家的直接交互、桌面输入行为、音频环境等外部刺激，并在一段时间后刷出可收集或可摆放的游戏道具。当前阶段策划案尚未完全定稿，因此优先建设一套桌宠通用框架，让程序、策划、美术可以并行协作，并让后续玩法扩展尽量配置化。

核心目标：

- 建立稳定的桌宠窗口、输入、UI、存档、配置和事件框架。
- 支持猫咪状态、行为、动画、表情、音效和互动反馈的可扩展开发。
- 支持定时刷宝、家具收集、家具摆放等基础循环。
- 提供策划可维护的数据配置结构，减少玩法调整对代码的依赖。
- 为后续接入更完整的 3D 猫模型、动作、AI 行为和桌面感知能力留出接口。

## 2. 当前项目基础

已有基础：

- Unity 项目结构已建立，使用 URP。
- `WindowController` 已实现 Windows 桌宠常用窗口能力：置顶、无边框、透明背景、鼠标穿透、窗口拖拽。
- `UIManager`、`UIPanel`、`SettingsPanel` 已形成基础 UI 面板框架。
- 已接入 DOTween，可用于 UI 动画、猫咪反馈动画和道具生成动效。
- 已有 `CatBodyController` 测试脚本，用于控制 BlendShape。

近期应保留并扩展这些能力，不建议推倒重写。

## 3. 推荐目录结构

建议后续按以下方式整理：

```text
Assets/
  Art/
    Model/
    Sprite/
    Material/
    Animation/
  Audio/
    BGM/
    SFX/
    Voice/
  Docs/
    DesktopPetFrameworkPlan.md
    Design/
    Tech/
    DataTables/
  Prefab/
    Pet/
    Props/
    Furniture/
    UI/
    VFX/
  Resources/
    Config/
  Scenes/
  Scripts/
    DesktopPet/
      Window/
      UI/
      Input/
      Pet/
      Behavior/
      Item/
      Furniture/
      Save/
      Config/
      Audio/
      Debug/
```

说明：

- `Docs/Design` 放策划案、状态表、交互表、道具表说明。
- `Docs/Tech` 放技术方案、接口说明、模块说明。
- `Docs/DataTables` 放给策划看的配置字段说明，真正运行时配置可放 `Resources/Config` 或 Addressables。
- 代码按功能模块拆分，避免所有桌宠逻辑堆在 `DesktopPet` 根目录。

## 4. 框架模块设计

### 4.1 桌宠窗口模块

职责：

- 管理透明背景、窗口置顶、无边框、鼠标穿透。
- 管理拖拽、吸附屏幕边缘、窗口尺寸和缩放。
- 管理 UI 打开时临时取消鼠标穿透。

建议新增：

- `DesktopWindowService`：统一对外暴露窗口能力。
- `WindowDragArea`：支持只在猫咪身体或指定区域拖拽。
- `ScreenBoundsHelper`：限制桌宠不会完全被拖出屏幕。
- `WindowPreset` 配置：开发模式、普通模式、展示模式。

### 4.2 输入与桌面感知模块

职责：

- 处理玩家直接点击、抚摸、拖拽、投喂、摆放家具等交互。
- 检测键盘、鼠标活跃度，用于影响猫咪情绪或行为。
- 可选接入麦克风音量检测，让猫咪响应环境声音。

建议拆分：

- `PetPointerInteractor`：鼠标点击、长按、拖拽、悬停检测。
- `PlayerActivityTracker`：统计输入活跃度、闲置时长、连续工作时长。
- `AudioActivityTracker`：只采集音量强弱，不保存音频内容。
- `InteractionEventBus`：把输入事件转为玩法事件，例如 `PetClicked`、`PlayerIdleStarted`、`LoudSoundDetected`。

隐私原则：

- 麦克风功能必须默认关闭，由玩家主动开启。
- 只读取音量/频段等即时数据，不录音、不落盘。
- 设置面板中明确提供开关。

### 4.3 猫咪状态模块

职责：

- 维护猫咪的基础数值、短期状态、长期成长和临时 Buff。
- 将状态变化广播给行为、动画、UI 和音频模块。

建议基础状态：

- 饥饿值：影响讨食、精神、掉落概率。
- 心情值：影响互动反馈、动作选择、亲密度增长。
- 精力值：影响活跃、睡觉、玩耍。
- 清洁值：后续可扩展洗澡或打理玩法。
- 亲密度：长期成长线，解锁动作、表情、家具互动。
- 好奇心：影响刷宝、探索桌面、玩家具。

建议状态类型：

- `PetStat`：数值型状态，例如 `Mood = 70`。
- `PetCondition`：标签型状态，例如 `Sleeping`、`Hungry`、`Excited`。
- `PetTrait`：长期特质，例如 `Lazy`、`Clingy`、`Playful`。

### 4.4 行为决策模块

职责：

- 根据当前状态、外部事件、冷却时间和权重选择猫咪行为。
- 让策划可以通过配置调整行为触发条件和优先级。

建议先使用轻量级 Utility AI，不急着引入复杂行为树。

行为例子：

- 待机：坐着、趴着、舔毛、打哈欠。
- 互动：被摸头、被戳、被拖动、被投喂。
- 情绪：撒娇、生气、惊吓、困倦、兴奋。
- 自主：睡觉、走动、看鼠标、玩家具、叼来道具。
- 特殊：检测到玩家久未输入时陪伴，检测到频繁输入时观察。

核心接口建议：

```csharp
public interface IPetBehavior
{
    string Id { get; }
    bool CanEnter(PetContext context);
    float GetScore(PetContext context);
    void Enter(PetContext context);
    void Tick(PetContext context, float deltaTime);
    void Exit(PetContext context);
}
```

### 4.5 动画与表现模块

职责：

- 将行为结果映射到 Animator、BlendShape、VFX、音效和小气泡。
- 隔离“玩法逻辑”和“表现资源”，方便美术替换资源。

建议结构：

- `PetAnimationController`：封装 Animator 参数和动画切换。
- `PetExpressionController`：封装 BlendShape、眼睛、嘴型、耳朵等表情控制。
- `PetFeedbackController`：播放爱心、问号、惊叹号、掉落闪光等反馈。
- `PetBubblePresenter`：显示短句、拟声词、状态提示。

策划协作方式：

- 行为只输出表现意图，例如 `HappySmall`、`SleepLoop`、`FoundItem`。
- 表现模块再将意图绑定到具体动画片段、BlendShape 和音效。

### 4.6 道具与刷宝模块

职责：

- 按时间、状态和事件刷出道具。
- 管理道具拾取、展示、收藏、消耗或转化为家具。

初期道具范围：

- 家具：猫窝、垫子、抓板、小球、食盆、装饰物。
- 临时奖励：小鱼干、铃铛、毛线球。

刷出规则建议：

- 基础时间间隔：例如 10 到 30 分钟随机一次。
- 条件修正：心情高、亲密度高、玩家活跃或闲置都会影响权重。
- 保底机制：长时间未刷出时提高概率。
- 反刷屏机制：限制同一时间桌面上存在的道具数量。

建议配置字段：

```text
ItemId
DisplayName
Category
PrefabPath
Rarity
BaseWeight
MinPetLevel
RequiredConditions
CooldownSeconds
MaxOwnedCount
CanPlaceAsFurniture
Description
```

### 4.7 家具模块

职责：

- 管理家具解锁、摆放、收起、交互和对猫咪状态的影响。
- 允许猫咪主动使用家具。

建议能力：

- 家具槽位。
- 家具提供状态修正，例如猫窝提高睡眠恢复，小球提高玩耍概率。
- 家具提供行为入口，例如猫会去抓板磨爪、去食盆讨食。

建议配置字段：

```text
FurnitureId
DisplayName
PrefabPath
SlotType
UnlockItemId
MoodModifier
EnergyModifier
BehaviorTags
InteractionIds
Description
```

### 4.8 配置与数据模块

职责：

- 支持策划通过表格或 ScriptableObject 管理数据。
- 程序负责加载、校验、热重载或编辑器工具。

推荐路线：

第一阶段使用 ScriptableObject，字段类型安全，适合 Unity 内协作。

第二阶段如果策划更习惯表格，可引入 CSV/Google Sheets 导出流程，再自动生成 ScriptableObject 或 JSON。

建议配置类型：

- `PetStatConfig`
- `PetBehaviorConfig`
- `InteractionConfig`
- `ItemConfig`
- `FurnitureConfig`
- `DropTableConfig`
- `DialogueConfig`
- `AudioCueConfig`

必须配套：

- 配置 ID 唯一性检查。
- 空 Prefab、空动画、非法数值范围检查。
- 一键导出配置报告，方便策划确认当前版本数据。

### 4.9 存档模块

职责：

- 保存猫咪状态、玩家设置、道具库存、家具摆放和上次在线时间。
- 支持离线时间结算。

建议存档内容：

```text
SaveVersion
PetName
PetStats
PetTraits
Inventory
PlacedFurniture
UnlockedContent
LastExitTimeUtc
Settings
```

建议实现：

- 本地 JSON 存档，放在 `Application.persistentDataPath`。
- 加入版本号和迁移逻辑。
- 写入时使用临时文件替换，避免异常退出损坏存档。

### 4.10 UI 与调试模块

职责：

- 提供轻量、不遮挡桌面的面板。
- 给开发和策划提供调试工具。

玩家 UI：

- 设置面板：音量、麦克风开关、鼠标穿透、置顶、缩放。
- 猫咪信息面板：名字、状态、亲密度。
- 背包面板：道具、家具。
- 家具摆放面板：选择、放置、收起。

开发 UI：

- 状态调试面板：实时修改饥饿、心情、精力。
- 行为调试面板：查看当前行为、候选行为分数、强制触发行为。
- 刷宝调试面板：查看掉落表、立即刷出、清理道具。
- 存档调试面板：保存、读取、清档、模拟离线时间。

## 5. 程序与策划协作规范

### 5.1 文档分层

- `DesktopPetFrameworkPlan.md`：总体框架和开发路线。
- `Design/CoreLoop.md`：核心循环和玩家体验。
- `Design/PetStats.md`：猫咪状态定义和数值规则。
- `Design/Interactions.md`：交互行为表。
- `Design/ItemsAndFurniture.md`：道具、家具、刷宝规则。
- `Tech/ModuleInterfaces.md`：程序接口说明。
- `DataTables/*.md`：配置字段说明。

### 5.2 配置协作原则

- 策划调整数值和触发条件，不直接改业务代码。
- 程序新增字段时，必须同步更新字段说明。
- 每个配置项必须有稳定 ID，ID 不随显示名变化。
- 删除配置前先标记废弃，确认无存档引用后再移除。

### 5.3 资源命名建议

```text
Pet_Cat_Default.prefab
Anim_Cat_Sleep_Loop.anim
BS_Cat_Happy
Item_FishSnack.asset
Furniture_CatBed.prefab
SFX_Cat_Meow_01.wav
UI_InventoryPanel.prefab
```

## 6. 开发里程碑

### M0：整理项目骨架

目标：让项目目录、命名和基础服务清晰。

任务：

- 建立 `Docs`、`Audio`、`Prefab/Pet`、`Prefab/Furniture` 等目录。
- 整理 `DesktopPet` 代码子目录。
- 修复当前中文文档和脚本注释乱码。
- 写明 UI 框架和窗口框架使用方式。

### M1：桌宠基础框架

目标：形成可运行的桌宠外壳。

任务：

- 完善窗口透明、置顶、穿透、拖拽、缩放。
- 完成 UI 面板栈和设置面板。
- 加入统一事件总线。
- 加入本地设置保存。

验收：

- 打包后能作为透明桌宠运行在 Windows 桌面。
- UI 打开时可正常点击，关闭后可恢复鼠标穿透。

### M2：猫咪状态与互动

目标：猫有基础状态，并能响应玩家行为。

任务：

- 实现 `PetStateModel`。
- 实现点击、长按、拖动、悬停互动。
- 接入基础动画/表情/音效反馈。
- 实现状态随时间变化。

验收：

- 猫咪会根据心情、精力、饥饿切换基础表现。
- 玩家点击或抚摸会产生明确反馈。

### M3：行为决策与配置

目标：策划可以配置猫咪行为。

任务：

- 实现轻量 Utility AI。
- 建立行为配置 ScriptableObject。
- 加入行为调试面板。
- 建立行为与动画表现映射。

验收：

- 能通过配置调整行为权重、冷却和触发条件。
- 调试面板可查看当前行为选择原因。

### M4：刷宝与背包

目标：形成“间隔刷出道具”的核心循环。

任务：

- 实现道具配置和掉落表。
- 实现定时刷宝、保底和数量限制。
- 实现桌面道具拾取。
- 实现背包面板。

验收：

- 等待一段时间后会刷出道具。
- 玩家可拾取并在背包中看到道具。

### M5：家具系统

目标：道具可以转化为可摆放家具，猫会与家具互动。

任务：

- 实现家具配置。
- 实现家具槽位摆放。
- 实现家具对状态和行为权重的影响。
- 加入猫咪主动使用家具的行为。

验收：

- 玩家可摆放至少 3 种家具。
- 猫会根据家具触发对应行为。

### M6：桌面感知扩展

目标：猫对玩家输入活跃度和可选音频环境有反应。

任务：

- 实现输入活跃度统计。
- 实现闲置、工作中、频繁输入等事件。
- 可选实现麦克风音量检测。
- 在设置中加入隐私说明和开关。

验收：

- 玩家长时间闲置或频繁输入时，猫会进入不同反馈。
- 麦克风关闭时不访问音频输入。

## 7. 首批建议实现清单

优先顺序：

1. 修复当前中文乱码，保证文档和脚本注释可读。
2. 建立 `DesktopPet/Window`、`DesktopPet/UI`、`DesktopPet/Pet`、`DesktopPet/Config`、`DesktopPet/Save` 目录。
3. 把现有窗口和 UI 脚本移动到对应目录，并保持命名空间稳定。
4. 新增 `GameEventBus`，让输入、猫咪状态、UI、刷宝之间解耦。
5. 新增 `PetStateModel` 和 `PetStateController`。
6. 新增 `PetInteractionController`，先支持点击、长按、拖拽三类事件。
7. 新增 `ItemConfig`、`DropTableConfig`、`FurnitureConfig` 的 ScriptableObject。
8. 新增 `SaveManager`，保存设置、猫咪状态和库存。
9. 新增开发调试面板，便于策划测试行为和掉落。

## 8. 主要风险与处理方式

- Windows 桌面透明和鼠标穿透在编辑器内无法完整验证：需要定期打包测试。
- 猫咪 3D 动画和行为数量会快速膨胀：必须用表现意图和配置映射隔离代码。
- 策划案未定导致返工：框架先做通用能力，具体数值和内容全部配置化。
- 麦克风和输入检测涉及隐私：必须默认关闭、只采集必要数据、设置中明确说明。
- 桌面道具过多会干扰玩家：需要数量上限、自动收纳或过期机制。

## 9. 下一步

建议下一次开发从 M0 和 M1 开始，先完成项目骨架、修复乱码、整理现有窗口/UI 框架，并补上基础存档和事件总线。这样后续猫咪状态、刷宝、家具和桌面感知都能接在同一套架构上，不会形成互相引用的临时代码。
