using System;
using UnityEngine;
using UnityEngine.UI;
using DesktopPet.Events;

namespace DesktopPet
{
    public class SettingsPanel : UIPanel
    {
        [SerializeField] private Slider scaleSlider;
        [SerializeField] private Text scaleValueText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetButton;

        private Transform petRoot;
        private float defaultScale;
        private IDisposable petScaleSubscription;

        public void Initialize(Transform targetPetRoot, float minScale, float maxScale, float initialScale, float resetScale)
        {
            petRoot = targetPetRoot;
            defaultScale = resetScale;

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
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(ResetScale);
                resetButton.onClick.AddListener(ResetScale);
            }

            petScaleSubscription?.Dispose();
            petScaleSubscription = GameEventBus.Subscribe<PetScaleChangedEvent>(OnExternalPetScaleChanged);
            SetPetScale(initialScale);
        }

        private void OnDestroy()
        {
            petScaleSubscription?.Dispose();
            petScaleSubscription = null;
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
    }
}
