using System;
using DesktopPet.Pet.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public sealed class PetAppearancePanel : UIPanel
    {
        [SerializeField] private Button warmFurButton;
        [SerializeField] private Button coolFurButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Text selectionText;
        [SerializeField] private Image warmSwatch;
        [SerializeField] private Image coolSwatch;
        private PetAppearanceController appearance;
        private Action confirmed;
        private int furStyle;

        public void Initialize(PetAppearanceController controller, Action onConfirmed)
        {
            appearance = controller;
            confirmed = onConfirmed;
            furStyle = appearance != null ? appearance.FurStyle : 0;
            Bind(warmFurButton, () => SelectFur(0));
            Bind(coolFurButton, () => SelectFur(1));
            Bind(confirmButton, Confirm);
            ApplyFont();
            Refresh();
        }

        private void SelectFur(int value) { furStyle = value; Preview(); }

        private void Preview()
        {
            ResolveAppearance();
            appearance?.Preview(furStyle);
            Refresh();
        }

        private void Confirm()
        {
            ResolveAppearance();
            appearance?.Confirm(furStyle);
            confirmed?.Invoke();
        }

        private void ResolveAppearance()
        {
            if (appearance != null) return;
            appearance = FindObjectOfType<PetAppearanceController>();
            if (appearance != null) return;
            var pet = GameObject.Find("Cat");
            if (pet != null) appearance = pet.GetComponent<PetAppearanceController>();
        }

        private void Refresh()
        {
            selectionText.text = furStyle == 0 ? "暖橘猫咪" : "雾蓝灰猫咪";
            SetSelected(warmFurButton, furStyle == 0); SetSelected(coolFurButton, furStyle == 1);
            if (warmSwatch != null) warmSwatch.color = new Color(1f, 0.72f, 0.48f, 1f);
            if (coolSwatch != null) coolSwatch.color = new Color(0.62f, 0.70f, 0.82f, 1f);
        }

        private static void SetSelected(Button button, bool selected)
        {
            if (button != null && button.targetGraphic != null)
                button.targetGraphic.color = selected ? new Color(0.98f, 0.69f, 0.38f, 1f) : new Color(0.19f, 0.22f, 0.28f, 1f);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void ApplyFont()
        {
            var font = Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 18);
            foreach (var text in GetComponentsInChildren<Text>(true)) text.font = font;
        }
    }
}
