#if UNITY_EDITOR
using DesktopPet.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.Editor
{
    [InitializeOnLoad]
    public static class FurnitureExchangePanelPrefabBuilder
    {
        private const string Path = "Assets/Resources/UI/FurnitureExchangePanel.prefab";
        private static readonly Color Panel = new Color(0.12f, 0.14f, 0.18f, 0.98f);
        private static readonly Color Card = new Color(0.19f, 0.22f, 0.28f, 1f);
        private static readonly Color Accent = new Color(0.98f, 0.69f, 0.38f, 1f);
        private static readonly Color TextColor = new Color(0.94f, 0.95f, 0.97f, 1f);
        private static readonly Color Muted = new Color(0.68f, 0.72f, 0.78f, 1f);

        static FurnitureExchangePanelPrefabBuilder()
        {
            EditorApplication.delayCall += BuildIfMissing;
            EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += BuildIfMissing; };
        }

        private static void BuildIfMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (prefab == null || prefab.transform.Find("ExchangeLayoutV1") == null) Rebuild();
        }

        [MenuItem("Desktop Pet/重建家具兑换面板 Prefab")]
        public static void Rebuild() { if (AssetDatabase.LoadAssetAtPath<GameObject>(Path) != null) AssetDatabase.DeleteAsset(Path); Build(); }

        private static void Build()
        {
            var root = Box("FurnitureExchangePanel", null, Panel); root.layer = 5; Rect(root, -18, -18, 460, 550, Vector2.one);
            var panel = root.AddComponent<FurnitureExchangePanel>(); root.AddComponent<UIDraggablePanel>();
            var marker = new GameObject("ExchangeLayoutV1"); marker.transform.SetParent(root.transform, false);
            Rect(Label("家具兑换", root.transform, 25, TextColor, true), 24, -18, 260, 38);
            var close = Button("收起", root.transform, Card); Rect(close, -20, -18, 72, 34, Vector2.one);
            var rule = Label("消耗同款家具 ×3，随机获得 1 件家具", root.transform, 14, Muted); Rect(rule, 24, -66, 412, 42);
            var preview = Box("家具预览", root.transform, new Color(0.35f, 0.38f, 0.44f, 1f)); Rect(preview, 24, -120, 130, 180);
            var name = Label("暂时没有可兑换的家具", root.transform, 21, TextColor, true); Rect(name, 174, -120, 262, 38);
            var rarity = Label("当前档位：普通", root.transform, 15, Accent); Rect(rarity, 174, -166, 262, 30);
            var count = Label("拥有 0　已摆 0　可用 0", root.transform, 15, Muted); Rect(count, 174, -204, 262, 34);
            var previous = Button("‹ 上一件", root.transform, Card); Rect(previous, 174, -258, 122, 42);
            var next = Button("下一件 ›", root.transform, Card); Rect(next, 314, -258, 122, 42);
            var result = Label(string.Empty, root.transform, 15, Accent, true, TextAnchor.MiddleCenter); Rect(result, 24, -320, 412, 42);
            var exchange = Button("使用 3 件兑换", root.transform, Accent); Rect(exchange, 24, -372, 412, 50);
            var back = Button("返回家具布置", root.transform, Card); Rect(back, 24, -434, 412, 40);
            var debugGrant = Button("测试：补充 3 件柔软猫窝", root.transform, Card); Rect(debugGrant, 24, -486, 412, 38);

            var so = new SerializedObject(panel);
            Assign(so, "closeButton", close.GetComponent<Button>()); Assign(so, "backButton", back.GetComponent<Button>());
            Assign(so, "previousButton", previous.GetComponent<Button>()); Assign(so, "nextButton", next.GetComponent<Button>());
            Assign(so, "exchangeButton", exchange.GetComponent<Button>()); Assign(so, "furnitureNameText", name);
            Assign(so, "debugGrantButton", debugGrant.GetComponent<Button>());
            Assign(so, "countText", count); Assign(so, "rarityText", rarity); Assign(so, "ruleText", rule);
            Assign(so, "resultText", result); Assign(so, "previewImage", preview.GetComponent<Image>());
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Path); Object.DestroyImmediate(root); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }

        private static GameObject Box(string name, Transform parent, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); if (parent) go.transform.SetParent(parent, false); go.GetComponent<Image>().color = color; return go; }
        private static Text Label(string value, Transform parent, int size, Color color, bool bold = false, TextAnchor alignment = TextAnchor.MiddleLeft) { var go = new GameObject(string.IsNullOrEmpty(value) ? "Text" : value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false); var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontSize = size; text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; text.color = color; text.alignment = alignment; return text; }
        private static GameObject Button(string value, Transform parent, Color color) { var go = Box(value, parent, color); var button = go.AddComponent<Button>(); button.targetGraphic = go.GetComponent<Image>(); var text = Label(value, go.transform, 16, TextColor, false, TextAnchor.MiddleCenter); text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one; text.rectTransform.offsetMin = Vector2.zero; text.rectTransform.offsetMax = Vector2.zero; return go; }
        private static void Assign(SerializedObject so, string property, Object value) => so.FindProperty(property).objectReferenceValue = value;
        private static void Rect(Component item, float x, float y, float w, float h, Vector2? anchor = null) => Rect(item.gameObject, x, y, w, h, anchor);
        private static void Rect(GameObject item, float x, float y, float w, float h, Vector2? anchor = null) { var a = anchor ?? new Vector2(0, 1); var rect = item.GetComponent<RectTransform>(); rect.anchorMin = a; rect.anchorMax = a; rect.pivot = a; rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(w, h); }
    }
}
#endif
