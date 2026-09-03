#if UNITY_EDITOR
using DesktopPet.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.Editor
{
    [InitializeOnLoad]
    public static class PetAppearancePanelPrefabBuilder
    {
        private const string Path = "Assets/Resources/UI/PetAppearancePanel.prefab";
        private static readonly Color Panel = new Color(0.12f, 0.14f, 0.18f, 0.98f);
        private static readonly Color Card = new Color(0.19f, 0.22f, 0.28f, 1f);
        private static readonly Color Accent = new Color(0.98f, 0.69f, 0.38f, 1f);
        private static readonly Color TextColor = new Color(0.94f, 0.95f, 0.97f, 1f);
        private static readonly Color Muted = new Color(0.68f, 0.72f, 0.78f, 1f);

        static PetAppearancePanelPrefabBuilder() => EditorApplication.delayCall += BuildIfMissing;
        private static void BuildIfMissing()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (prefab == null) Build();
            else if (prefab.transform.Find("FurOnlyLayout") == null) Rebuild();
        }

        [MenuItem("Desktop Pet/重建猫咪外观面板 Prefab")]
        public static void Rebuild() { if (AssetDatabase.LoadAssetAtPath<GameObject>(Path) != null) AssetDatabase.DeleteAsset(Path); Build(); }

        private static void Build()
        {
            var root = Box("PetAppearancePanel", null, Panel); root.layer = 5; Rect(root, -18, -18, 440, 365, Vector2.one);
            var panel = root.AddComponent<PetAppearancePanel>(); root.AddComponent<UIDraggablePanel>();
            var marker = new GameObject("FurOnlyLayout"); marker.transform.SetParent(root.transform, false);
            Rect(Label("欢迎回家", root.transform, 27, TextColor, true), 24, -20, 392, 42);
            Rect(Label("先选择一只陪伴你的猫咪吧", root.transform, 16, Muted), 24, -64, 392, 28);

            Rect(Label("毛色", root.transform, 16, Muted), 24, -112, 100, 26);
            var warm = Choice("暖橘", root.transform, 24, -144, 190, 76);
            var warmSwatch = Box("暖橘色卡", warm.transform, new Color(1f, 0.72f, 0.48f)); Rect(warmSwatch, 14, -14, 48, 48);
            var cool = Choice("雾蓝灰", root.transform, 226, -144, 190, 76);
            var coolSwatch = Box("雾蓝灰色卡", cool.transform, new Color(0.62f, 0.70f, 0.82f)); Rect(coolSwatch, 14, -14, 48, 48);

            var selection = Label("暖橘猫咪", root.transform, 18, Accent, true, TextAnchor.MiddleCenter);
            Rect(selection, 24, -244, 392, 34);
            var confirm = Button("就是这只猫", root.transform, Accent); Rect(confirm, 24, -294, 392, 50);

            var so = new SerializedObject(panel);
            Assign(so, "warmFurButton", warm.GetComponent<Button>()); Assign(so, "coolFurButton", cool.GetComponent<Button>());
            Assign(so, "confirmButton", confirm.GetComponent<Button>()); Assign(so, "selectionText", selection);
            Assign(so, "warmSwatch", warmSwatch.GetComponent<Image>()); Assign(so, "coolSwatch", coolSwatch.GetComponent<Image>());
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Path); Object.DestroyImmediate(root); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }

        private static GameObject Choice(string value, Transform parent, float x, float y, float w, float h) { var go = Button(value, parent, Card); Rect(go, x, y, w, h); var text = go.GetComponentInChildren<Text>(); var rt = text.rectTransform; rt.offsetMin = new Vector2(68, 0); return go; }
        private static GameObject Box(string name, Transform parent, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); if (parent) go.transform.SetParent(parent, false); go.GetComponent<Image>().color = color; return go; }
        private static Text Label(string value, Transform parent, int size, Color color, bool bold = false, TextAnchor anchor = TextAnchor.MiddleLeft) { var go = new GameObject(value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false); var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontSize = size; text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; text.color = color; text.alignment = anchor; return text; }
        private static GameObject Button(string value, Transform parent, Color color) { var go = Box(value, parent, color); var button = go.AddComponent<Button>(); button.targetGraphic = go.GetComponent<Image>(); var text = Label(value, go.transform, 17, TextColor, false, TextAnchor.MiddleCenter); text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one; text.rectTransform.offsetMin = Vector2.zero; text.rectTransform.offsetMax = Vector2.zero; return go; }
        private static void Assign(SerializedObject so, string property, Object value) => so.FindProperty(property).objectReferenceValue = value;
        private static void Rect(Component item, float x, float y, float w, float h, Vector2? anchor = null) => Rect(item.gameObject, x, y, w, h, anchor);
        private static void Rect(GameObject item, float x, float y, float w, float h, Vector2? anchor = null) { var a = anchor ?? new Vector2(0, 1); var rect = item.GetComponent<RectTransform>(); rect.anchorMin = a; rect.anchorMax = a; rect.pivot = a; rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(w, h); }
    }
}
#endif
