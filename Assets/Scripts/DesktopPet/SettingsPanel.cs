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

    }
}
