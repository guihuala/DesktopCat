using UnityEngine;
using UnityEngine.UI;

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

            SetPetScale(initialScale);
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
        }
    }
}
