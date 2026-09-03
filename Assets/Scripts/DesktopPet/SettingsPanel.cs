using System;
using UnityEngine;
using UnityEngine.UI;
using DesktopPet.Events;

namespace DesktopPet
{
    public class SettingsPanel : UIPanel
    {
        [Header("Pet")]
        [SerializeField] private Slider scaleSlider;
        [SerializeField] private Text scaleValueText;

        [Header("Window")]
        [SerializeField] private Toggle alwaysOnTopToggle;
        [SerializeField] private Toggle clickThroughToggle;
        [SerializeField] private Toggle allowDragToggle;
        [SerializeField] private Toggle transparentBackgroundToggle;
        [SerializeField] private Toggle borderlessToggle;
        [SerializeField] private Button centerWindowButton;

        [Header("Actions")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetButton;

        private WindowController windowController;
        private Transform petRoot;
        private float defaultScale;
        private Action closeRequested;
        private IDisposable petScaleSubscription;
        private IDisposable windowSettingsSubscription;

        public void Initialize(
            Transform targetPetRoot,
            WindowController targetWindowController,
            float minScale,
            float maxScale,
            float initialScale,
            float resetScale,
            Action onCloseRequested = null)
        {
            ApplyFriendlyVisualStyle();
            petRoot = targetPetRoot;
            windowController = targetWindowController;
            defaultScale = resetScale;
            closeRequested = onCloseRequested;

            if (scaleSlider != null)
            {
                scaleSlider.minValue = minScale;
                scaleSlider.maxValue = maxScale;
                scaleSlider.wholeNumbers = false;
                scaleSlider.onValueChanged.RemoveListener(SetPetScale);
                scaleSlider.onValueChanged.AddListener(SetPetScale);
                scaleSlider.SetValueWithoutNotify(initialScale);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(RequestClose);
                closeButton.onClick.AddListener(RequestClose);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(ResetScale);
                resetButton.onClick.AddListener(ResetScale);
            }

            BindWindowControls();

            petScaleSubscription?.Dispose();
            petScaleSubscription = GameEventBus.Subscribe<PetScaleChangedEvent>(OnExternalPetScaleChanged);
            windowSettingsSubscription?.Dispose();
            windowSettingsSubscription = GameEventBus.Subscribe<WindowSettingsChangedEvent>(OnWindowSettingsChanged);

            SetPetScale(initialScale);
            RefreshWindowControls();
        }

        private void OnDestroy()
        {
            petScaleSubscription?.Dispose();
            petScaleSubscription = null;
            windowSettingsSubscription?.Dispose();
            windowSettingsSubscription = null;
        }

        private void ResetScale()
        {
            if (scaleSlider != null)
            {
                scaleSlider.value = defaultScale;
            }
            else
            {
                SetPetScale(defaultScale);
            }
        }

        private void RequestClose()
        {
            if (closeRequested != null)
            {
                closeRequested.Invoke();
            }
            else
            {
                Close();
            }
        }

        private void SetPetScale(float value)
        {
            if (petRoot != null)
            {
                petRoot.localScale = Vector3.one * value;
            }

            if (scaleValueText != null)
            {
                scaleValueText.text = $"{value:0.00}x";
            }

            GameEventBus.Publish(new PetScaleChangedEvent(value));
        }

        private void OnExternalPetScaleChanged(PetScaleChangedEvent gameEvent)
        {
            if (scaleSlider != null && !Mathf.Approximately(scaleSlider.value, gameEvent.Scale))
            {
                scaleSlider.SetValueWithoutNotify(gameEvent.Scale);
            }

            if (scaleValueText != null)
            {
                scaleValueText.text = $"{gameEvent.Scale:0.00}x";
            }
        }

        private void BindWindowControls()
        {
            BindToggle(alwaysOnTopToggle, SetAlwaysOnTop);
            BindToggle(clickThroughToggle, SetClickThrough);
            BindToggle(allowDragToggle, SetAllowDrag);
            BindToggle(transparentBackgroundToggle, SetTransparentBackground);
            BindToggle(borderlessToggle, SetBorderless);

            if (centerWindowButton != null)
            {
                centerWindowButton.onClick.RemoveListener(CenterWindow);
                centerWindowButton.onClick.AddListener(CenterWindow);
            }
        }

        private static void BindToggle(Toggle toggle, UnityEngine.Events.UnityAction<bool> action)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.onValueChanged.RemoveListener(action);
            toggle.onValueChanged.AddListener(action);
        }

        private void SetAlwaysOnTop(bool enabled)
        {
            if (windowController != null)
            {
                windowController.SetAlwaysOnTop(enabled);
            }
        }

        private void SetClickThrough(bool enabled)
        {
            if (windowController != null)
            {
                if (enabled && IsOpen)
                {
                    RequestClose();
                }

                windowController.SetClickThrough(enabled);
            }
        }

        private void SetAllowDrag(bool enabled)
        {
            if (windowController != null)
            {
                windowController.SetAllowDrag(enabled);
            }
        }

        private void SetTransparentBackground(bool enabled)
        {
            if (windowController != null)
            {
                windowController.SetTransparentBackground(enabled);
            }
        }

        private void SetBorderless(bool enabled)
        {
            if (windowController != null)
            {
                windowController.SetBorderless(enabled);
            }
        }

        private void CenterWindow()
        {
            if (windowController != null)
            {
                windowController.CenterOnScreen();
            }
        }

        private void OnWindowSettingsChanged(WindowSettingsChangedEvent gameEvent)
        {
            SetToggleWithoutNotify(alwaysOnTopToggle, gameEvent.AlwaysOnTop);
            SetToggleWithoutNotify(clickThroughToggle, gameEvent.ClickThrough);
            SetToggleWithoutNotify(allowDragToggle, gameEvent.AllowDrag);
            SetToggleWithoutNotify(transparentBackgroundToggle, gameEvent.TransparentBackground);
            SetToggleWithoutNotify(borderlessToggle, gameEvent.Borderless);
        }

        private void RefreshWindowControls()
        {
            if (windowController == null)
            {
                return;
            }

            SetToggleWithoutNotify(alwaysOnTopToggle, windowController.IsAlwaysOnTop);
            SetToggleWithoutNotify(clickThroughToggle, windowController.IsClickThrough);
            SetToggleWithoutNotify(allowDragToggle, windowController.AllowDrag);
            SetToggleWithoutNotify(transparentBackgroundToggle, windowController.IsTransparentBackground);
            SetToggleWithoutNotify(borderlessToggle, windowController.IsBorderless);
        }

        private static void SetToggleWithoutNotify(Toggle toggle, bool value)
        {
            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(value);
            }
        }

        private void ApplyFriendlyVisualStyle()
        {
            var font = Font.CreateDynamicFontFromOSFont(
                new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 18);
            var panelColor = new Color(0.12f, 0.14f, 0.18f, 0.97f);
            var cardColor = new Color(0.19f, 0.22f, 0.28f, 1f);
            var accentColor = new Color(0.98f, 0.69f, 0.38f, 1f);
            var textColor = new Color(0.94f, 0.95f, 0.97f, 1f);

            var rootRect = transform as RectTransform;
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.one;
                rootRect.anchorMax = Vector2.one;
                rootRect.pivot = Vector2.one;
                rootRect.anchoredPosition = new Vector2(-18f, -18f);
                rootRect.sizeDelta = new Vector2(390f, 410f);
            }
            var rootImage = GetComponent<Image>();
            if (rootImage != null) rootImage.color = panelColor;

            foreach (var text in GetComponentsInChildren<Text>(true))
            {
                text.font = font;
                text.color = textColor;
            }
            SetText("Title", "桌宠设置", 24, FontStyle.Bold);
            SetText("ScaleLabel", "猫咪大小", 17, FontStyle.Normal);
            SetText("AlwaysOnTopToggle/Label", "保持在最前", 16, FontStyle.Normal);
            SetText("ClickThroughToggle/Label", "鼠标穿透", 16, FontStyle.Normal);
            SetText("AllowDragToggle/Label", "允许拖动", 16, FontStyle.Normal);
            SetText("TransparentBackgroundToggle/Label", "透明背景", 16, FontStyle.Normal);
            SetText("BorderlessToggle/Label", "隐藏窗口边框", 16, FontStyle.Normal);
            SetText("CenterWindowButton/Label", "窗口居中", 16, FontStyle.Normal);
            SetText("ResetButton/Label", "恢复默认大小", 15, FontStyle.Normal);
            SetText("CloseButton/Label", "×", 22, FontStyle.Normal);

            SetTopLeft("ScaleLabel", 24f, -72f, 150f, 30f);
            SetTopRight("ScaleValue", -24f, -72f, 90f, 30f);
            StretchHorizontal("ScaleSlider", 24f, 24f, -112f, 30f);
            SetTopLeft("AlwaysOnTopToggle", 24f, -166f, 165f, 34f);
            SetTopLeft("ClickThroughToggle", 201f, -166f, 165f, 34f);
            SetTopLeft("AllowDragToggle", 24f, -210f, 165f, 34f);
            SetTopLeft("TransparentBackgroundToggle", 201f, -210f, 165f, 34f);
            SetTopLeft("BorderlessToggle", 24f, -254f, 165f, 34f);
            SetTopLeft("CenterWindowButton", 201f, -254f, 165f, 34f);
            SetTopRight("CloseButton", -18f, -16f, 36f, 36f);
            SetTopRight("ResetButton", -24f, -330f, 150f, 38f);

            foreach (var toggle in GetComponentsInChildren<Toggle>(true))
            {
                if (toggle.targetGraphic != null) toggle.targetGraphic.color = cardColor;
                if (toggle.graphic != null) toggle.graphic.color = accentColor;
            }
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                var image = button.targetGraphic as Image;
                if (image != null) image.color = cardColor;
            }
            if (scaleSlider != null)
            {
                if (scaleSlider.fillRect != null && scaleSlider.fillRect.TryGetComponent<Image>(out var fill)) fill.color = accentColor;
                if (scaleSlider.handleRect != null && scaleSlider.handleRect.TryGetComponent<Image>(out var handle)) handle.color = accentColor;
            }
        }

        private void SetText(string path, string value, int size, FontStyle style)
        {
            var target = transform.Find(path);
            if (target == null) return;
            var text = target.GetComponent<Text>();
            if (text == null) return;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
        }

        private void SetTopLeft(string path, float x, float y, float width, float height)
        {
            SetRect(path, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(width, height));
        }

        private void SetTopRight(string path, float x, float y, float width, float height)
        {
            SetRect(path, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(x, y), new Vector2(width, height));
        }

        private void StretchHorizontal(string path, float left, float right, float y, float height)
        {
            SetRect(path, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2((left - right) * 0.5f, y), new Vector2(-(left + right), height));
        }

        private void SetRect(string path, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var target = transform.Find(path) as RectTransform;
            if (target == null) return;
            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.pivot = pivot;
            target.anchoredPosition = position;
            target.sizeDelta = size;
        }

    }
}
