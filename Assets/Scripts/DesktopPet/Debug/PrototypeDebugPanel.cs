using DesktopPet.Pet.Behavior;
using DesktopPet.Pet.Movement;
using DesktopPet.Pet.State;
using DesktopPet.Presentation;
using DesktopPet.Rewards;
using DesktopPet.Furniture;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet
{
    /// <summary>仅在编辑器和开发包中创建的轻量中文 uGUI 测试面板。</summary>
    public sealed class PrototypeDebugPanel : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.F7;
        [SerializeField] private KeyCode alternateToggleKey = KeyCode.BackQuote;
        private PetStateController state;
        private PetBehaviorBrain brain;
        private PetMovementController movement;
        private DayNightController dayNight;
        private OnlineRewardService onlineReward;
        private FurnitureDropService furnitureDrop;
        private GameObject panel;
        private GameObject openButton;
        private Text behaviourText;
        private Text detailText;
        private Text energyText;
        private Text hungerText;
        private Slider energySlider;
        private Slider hungerSlider;
        private Text rewardText;
        private Text dropResultText;
        private Font font;

        private static readonly Color PanelColor = new Color(0.12f, 0.14f, 0.18f, 0.96f);
        private static readonly Color CardColor = new Color(0.19f, 0.22f, 0.28f, 1f);
        private static readonly Color AccentColor = new Color(0.98f, 0.69f, 0.38f, 1f);
        private static readonly Color TextColor = new Color(0.94f, 0.95f, 0.97f, 1f);
        private static readonly Color MutedColor = new Color(0.68f, 0.72f, 0.78f, 1f);

        private void Awake()
        {
            state = GetComponent<PetStateController>();
            brain = GetComponent<PetBehaviorBrain>();
            movement = GetComponent<PetMovementController>();
            dayNight = FindObjectOfType<DayNightController>();
            onlineReward = FindObjectOfType<OnlineRewardService>();
            furnitureDrop = FindObjectOfType<FurnitureDropService>();
            BuildUi();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(alternateToggleKey)) SetVisible(!panel.activeSelf);
            Refresh();
        }

        private void OnDestroy()
        {
            if (panel != null) Destroy(panel.transform.root.gameObject);
        }

        private void BuildUi()
        {
            font = Font.CreateDynamicFontFromOSFont(
                new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 18);
            var canvasObject = new GameObject("PrototypeDebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            panel = CreateBox("猫咪调试面板", canvasObject.transform, PanelColor);
            SetRect(panel.GetComponent<RectTransform>(), new Vector2(18f, -18f), new Vector2(390f, 690f), new Vector2(0f, 1f));
            var title = CreateText("状态测试", panel.transform, 24, TextColor, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetRect(title.rectTransform, new Vector2(24f, -18f), new Vector2(260f, 36f), new Vector2(0f, 1f));
            var close = CreateButton("收起", panel.transform, () => SetVisible(false), CardColor);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(-20f, -18f), new Vector2(72f, 34f), new Vector2(1f, 1f));

            behaviourText = CreateText("猫咪正在：待机", panel.transform, 22, AccentColor, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetRect(behaviourText.rectTransform, new Vector2(24f, -70f), new Vector2(342f, 38f), new Vector2(0f, 1f));
            detailText = CreateText("正在读取状态……", panel.transform, 15, MutedColor, FontStyle.Normal, TextAnchor.UpperLeft);
            SetRect(detailText.rectTransform, new Vector2(24f, -110f), new Vector2(342f, 44f), new Vector2(0f, 1f));

            energyText = CreateText("精力 0", panel.transform, 16, TextColor, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(energyText.rectTransform, new Vector2(24f, -165f), new Vector2(342f, 26f), new Vector2(0f, 1f));
            energySlider = CreateSlider(panel.transform, new Vector2(24f, -195f));
            energySlider.onValueChanged.AddListener(value => state.SetStats(value, state.Hunger));
            hungerText = CreateText("饥饿 0", panel.transform, 16, TextColor, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(hungerText.rectTransform, new Vector2(24f, -230f), new Vector2(342f, 26f), new Vector2(0f, 1f));
            hungerSlider = CreateSlider(panel.transform, new Vector2(24f, -260f));
            hungerSlider.onValueChanged.AddListener(value => state.SetStats(state.Energy, value));

            var hint = CreateText("点击按钮，马上确认对应状态是否正常", panel.transform, 15, MutedColor, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(hint.rectTransform, new Vector2(24f, -305f), new Vector2(342f, 28f), new Vector2(0f, 1f));
            CreateStateButton("散步", "Wander", 24f, -344f);
            CreateStateButton("打盹", "Nap", 201f, -344f);
            CreateStateButton("睡觉", "Sleep", 24f, -392f);
            CreateStateButton("吃饭", "Eat", 201f, -392f);
            CreateStateButton("靠近镜头", "ApproachCamera", 24f, -440f);
            CreateStateButton("恢复待机", "Idle", 201f, -440f);

            var speedLabel = CreateText("观察速度", panel.transform, 15, MutedColor, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(speedLabel.rectTransform, new Vector2(24f, -493f), new Vector2(100f, 26f), new Vector2(0f, 1f));
            CreateSmallButton("正常", 112f, () => Time.timeScale = 1f);
            CreateSmallButton("快进 ×5", 196f, () => Time.timeScale = 5f);
            CreateSmallButton("快进 ×20", 284f, () => Time.timeScale = 20f);

            var lightLabel = CreateText("昼夜效果", panel.transform, 15, MutedColor, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(lightLabel.rectTransform, new Vector2(24f, -540f), new Vector2(100f, 26f), new Vector2(0f, 1f));
            CreateSmallButton("跟随系统", 112f, -537f, () => SetDayNight(DayNightMode.FollowSystem));
            CreateSmallButton("白天", 210f, -537f, () => SetDayNight(DayNightMode.Day));
            CreateSmallButton("夜晚", 294f, -537f, () => SetDayNight(DayNightMode.Night));

            var rewardLabel = CreateText("家具计时", panel.transform, 15, MutedColor, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(rewardLabel.rectTransform, new Vector2(24f, -582f), new Vector2(90f, 26f), new Vector2(0f, 1f));
            rewardText = CreateText("正在读取……", panel.transform, 15, TextColor, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(rewardText.rectTransform, new Vector2(104f, -582f), new Vector2(150f, 26f), new Vector2(0f, 1f));
            CreateSmallButton("增加10分钟", 250f, -579f, () => AddRewardMinutes(10));
            CreateSmallButton("增加30分钟", 24f, -620f, () => AddRewardMinutes(30));
            CreateSmallButton("预览一次掉落", 126f, -620f, TestSingleDrop);
            CreateSmallButton("模拟1000次", 228f, -620f, TestDropDistribution);
            dropResultText = CreateText("掉落预览不会生成可领取家具", panel.transform, 14, MutedColor, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(dropResultText.rectTransform, new Vector2(24f, -657f), new Vector2(342f, 24f), new Vector2(0f, 1f));

            openButton = CreateButton("打开状态测试", canvasObject.transform, () => SetVisible(true), AccentColor);
            SetRect(openButton.GetComponent<RectTransform>(), new Vector2(18f, -18f), new Vector2(150f, 42f), new Vector2(0f, 1f));
            openButton.SetActive(false);
        }

        private void Refresh()
        {
            if (panel == null || !panel.activeSelf || state == null || brain == null) return;
            behaviourText.text = $"猫咪正在：{BehaviourName(brain.CurrentBehaviourId)}";
            var moving = movement != null && movement.IsMoving ? "正在移动" : "没有移动";
            var locked = state.IsUninterruptible ? "暂时不能打断" : "可以切换状态";
            detailText.text = $"已持续 {brain.CurrentBehaviourDuration:0} 秒 · {moving}\n{locked} · 鼠标状态：{ActivityName(brain.ActivityLevel.ToString())}";
            energyText.text = $"精力  {state.Energy:0} / 100";
            hungerText.text = $"饥饿  {state.Hunger:0} / 100";
            energySlider.SetValueWithoutNotify(state.Energy);
            hungerSlider.SetValueWithoutNotify(state.Hunger);
            if (onlineReward == null) onlineReward = FindObjectOfType<OnlineRewardService>();
            if (rewardText != null && onlineReward != null)
            {
                var remaining = onlineReward.SecondsUntilNext;
                rewardText.text = onlineReward.PendingRewards >= onlineReward.MaxPendingRewards
                    ? $"待领取 {onlineReward.PendingRewards}/{onlineReward.MaxPendingRewards} · 已满"
                    : $"待领取 {onlineReward.PendingRewards}/{onlineReward.MaxPendingRewards} · 还需 {remaining / 60d:0.0} 分钟";
            }
        }

        private void SetVisible(bool visible)
        {
            panel.SetActive(visible);
            openButton.SetActive(!visible);
        }

        private void CreateStateButton(string label, string behaviourId, float x, float y)
        {
            var button = CreateButton(label, panel.transform, () => brain.ForceBehaviour(behaviourId), CardColor);
            SetRect(button.GetComponent<RectTransform>(), new Vector2(x, y), new Vector2(165f, 40f), new Vector2(0f, 1f));
        }

        private void CreateSmallButton(string label, float x, UnityEngine.Events.UnityAction action)
        {
            CreateSmallButton(label, x, -490f, action);
        }

        private void CreateSmallButton(string label, float x, float y, UnityEngine.Events.UnityAction action)
        {
            var button = CreateButton(label, panel.transform, action, CardColor);
            SetRect(button.GetComponent<RectTransform>(), new Vector2(x, y), new Vector2(label.Length > 4 ? 92f : 78f, 32f), new Vector2(0f, 1f));
        }

        private void SetDayNight(DayNightMode mode)
        {
            if (dayNight == null) dayNight = FindObjectOfType<DayNightController>();
            if (dayNight != null) dayNight.SetMode(mode);
        }

        private void AddRewardMinutes(int minutes)
        {
            if (onlineReward == null) onlineReward = FindObjectOfType<OnlineRewardService>();
            if (onlineReward != null) onlineReward.AddDebugSeconds(minutes * 60d);
        }

        private void TestSingleDrop()
        {
            if (furnitureDrop == null) furnitureDrop = FindObjectOfType<FurnitureDropService>();
            var item = furnitureDrop != null ? furnitureDrop.DrawOne() : null;
            dropResultText.text = item != null
                ? $"抽到：{item.displayName}（{RarityName(item.rarity)}）"
                : "抽取失败，请检查家具配置";
        }

        private void TestDropDistribution()
        {
            if (furnitureDrop == null) furnitureDrop = FindObjectOfType<FurnitureDropService>();
            dropResultText.text = furnitureDrop != null
                ? furnitureDrop.Simulate(1000, 20260903).ToString()
                : "模拟失败，请检查家具配置";
        }

        private static string RarityName(FurnitureRarity rarity)
        {
            switch (rarity)
            {
                case FurnitureRarity.Rare: return "稀有";
                case FurnitureRarity.Collectible: return "珍藏";
                default: return "普通";
            }
        }

        private Slider CreateSlider(Transform parent, Vector2 position)
        {
            var root = CreateBox("数值滑条", parent, CardColor);
            SetRect(root.GetComponent<RectTransform>(), position, new Vector2(342f, 18f), new Vector2(0f, 1f));
            var fill = CreateBox("当前值", root.transform, AccentColor);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            var slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.fillRect = fillRect;
            slider.targetGraphic = fill.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private GameObject CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action, Color color)
        {
            var root = CreateBox(label, parent, color);
            var button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            button.onClick.AddListener(action);
            var text = CreateText(label, root.transform, 16, TextColor, FontStyle.Normal, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return root;
        }

        private GameObject CreateBox(string name, Transform parent, Color color)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            item.GetComponent<Image>().color = color;
            return item;
        }

        private Text CreateText(string value, Transform parent, int size, Color color, FontStyle style, TextAnchor alignment)
        {
            var item = new GameObject(value, typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            var text = item.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.supportRichText = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static string BehaviourName(string id)
        {
            switch (id)
            {
                case "Wander": return "散步";
                case "Nap": return "打盹";
                case "Sleep": return "睡觉";
                case "Eat": return "吃饭";
                case "ApproachCamera": return "靠近镜头";
                default: return "待机";
            }
        }

        private static string ActivityName(string id)
        {
            switch (id)
            {
                case "Active": return "活跃";
                case "Idle": return "暂时离开";
                default: return "普通";
            }
        }
    }
}
