# Desktop Pet UI Framework

桌宠 UI 和普通游戏 UI 的重点不同：它应该轻、短时出现、少遮挡桌面，并且和窗口穿透/拖动配合。

## Core Ideas

- `UIManager` 是入口，负责实例化 panel prefab、打开/关闭面板、Esc 关闭最上层面板。
- `UIPanel` 是所有面板的基类，提供 `panelId`、互斥打开、是否阻止鼠标穿透、热键字段。
- `SettingsPanel` 是一个具体面板，只处理设置逻辑，外观来自 `Assets/Prefab/UI/SettingsPanel.prefab`。
- 常驻入口按钮应该放在场景或 prefab 中，不在代码里生成，方便美术和交互调位置。

## Desktop Pet UI Traits

- 常驻 UI 要少：桌宠主体永远优先，按钮和面板只在需要时出现。
- 面板要可关闭：支持按钮关闭，也支持 `Esc` 关闭最上层面板。
- 面板打开时要接管鼠标：如果窗口处于鼠标穿透，UI 打开时会临时关闭穿透，面板关闭后恢复。
- 面板默认互斥：设置、动作、信息等面板通常一次只开一个，避免挡住桌面。
- UI 不应抢桌宠拖动：窗口拖动脚本会忽略 UI 点击，滑条和按钮可以正常操作。
- 外观放 prefab：颜色、大小、布局、文案尽量在 prefab 里调，代码只做状态和数据绑定。

## Add A Panel

1. 创建一个 panel prefab。
2. 根节点挂 `UIPanel` 或继承自 `UIPanel` 的脚本。
3. 在 `UIManager` 的 `panelPrefabs` 里添加 prefab。
4. 场景按钮调用 `UIManager.OpenPanel(id)` 或 `UIManager.TogglePanel(id)`。
