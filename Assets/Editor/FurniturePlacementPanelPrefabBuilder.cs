#if UNITY_EDITOR
using DesktopPet.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.Editor
{
    [InitializeOnLoad]
    public static class FurniturePlacementPanelPrefabBuilder
    {
        private const string Path = "Assets/Resources/UI/FurniturePlacementPanel.prefab";
        private static readonly Color Panel = new Color(0.12f, 0.14f, 0.18f, 0.98f);
        private static readonly Color Card = new Color(0.19f, 0.22f, 0.28f, 1f);
        private static readonly Color Accent = new Color(0.98f, 0.69f, 0.38f, 1f);
        private static readonly Color Text = new Color(0.94f, 0.95f, 0.97f, 1f);
        private static readonly Color Muted = new Color(0.68f, 0.72f, 0.78f, 1f);

        static FurniturePlacementPanelPrefabBuilder()
        {
            EditorApplication.delayCall += EnsureV3;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += EnsureV3;
            };
        }

        private static void EnsureV3()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (prefab == null || prefab.transform.Find("LayoutV3") == null) Rebuild();
        }

        [MenuItem("Desktop Pet/重建家具摆放面板 Prefab")]
        public static void Rebuild()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Path) != null) AssetDatabase.DeleteAsset(Path);
            Build();
        }

        private static void Build()
        {
            var root = Box("FurniturePlacementPanel", null, Panel);
            root.layer = 5;
            Rect(root, -18, -18, 520, 650, Vector2.one);
            var panel = root.AddComponent<FurniturePlacementPanel>();
            root.AddComponent<UIDraggablePanel>();
            var marker = new GameObject("LayoutV3");
            marker.transform.SetParent(root.transform, false);

            Rect(Label("布置房间", root.transform, 25, Text, true), 24, -16, 330, 40);
            var close = Button("收起", root.transform, Card); Rect(close, -20, -18, 72, 34, Vector2.one);
            Rect(Label("① 选择摆放位置", root.transform, 15, Muted), 24, -66, 300, 26);
            var bed = Tab("猫窝", root.transform, 24); var bowl = Tab("食盆", root.transform, 122);
            var rug = Tab("地毯", root.transform, 220); var desktop = Tab("桌面", root.transform, 318); var toy = Tab("玩具", root.transform, 416);

            Rect(Label("② 选择家具", root.transform, 15, Muted), 24, -132, 220, 26);
            var anchorTitle = Label("正在布置：猫窝", root.transform, 19, Accent, true); Rect(anchorTitle, 220, -130, 276, 28);
            var card = Box("家具预览卡", root.transform, Card); Rect(card, 24, -168, 472, 270);
            var preview = Box("家具颜色预览", card.transform, new Color(0.35f, 0.38f, 0.44f, 1f)); Rect(preview, 18, -18, 150, 180);
            var glyph = Label("猫窝", preview.transform, 23, Text, true, TextAnchor.MiddleCenter); Stretch(glyph);
            var placedStatus = Label("○ 当前位置为空", card.transform, 14, Accent); Rect(placedStatus, 18, -208, 150, 28);
            var furnitureName = Label("还没有这类家具", card.transform, 20, Text, true); Rect(furnitureName, 188, -20, 262, 36);
            var count = Label("领取家具后就能在这里摆放", card.transform, 15, Muted); Rect(count, 188, -62, 262, 28);
            var description = Label(string.Empty, card.transform, 15, Text, false, TextAnchor.UpperLeft); Rect(description, 188, -100, 262, 76);
            var previous = Button("‹ 上一件", card.transform, Panel); Rect(previous, 188, -210, 120, 38);
            var next = Button("下一件 ›", card.transform, Panel); Rect(next, -18, -210, 120, 38, Vector2.one);

            Rect(Label("③ 确认摆放", root.transform, 15, Muted), 24, -456, 200, 26);
            var message = Label(string.Empty, root.transform, 15, Accent); Rect(message, 170, -454, 326, 28);
            var place = Button("摆放", root.transform, Accent); Rect(place, 24, -494, 472, 50);
            var remove = Button("收回当前家具", root.transform, Card); Rect(remove, 24, -556, 226, 42);
            var exchange = Button("兑换重复家具", root.transform, Card); Rect(exchange, 270, -556, 226, 42);
            Rect(Label("彩色几何体是原型家具，正式模型可直接替换 prefab。", root.transform, 13, Muted, false, TextAnchor.MiddleCenter), 24, -610, 472, 22);

            var so = new SerializedObject(panel);
            Assign(so, "closeButton", close.GetComponent<Button>()); Assign(so, "catBedButton", bed.GetComponent<Button>());
            Assign(so, "foodBowlButton", bowl.GetComponent<Button>()); Assign(so, "rugButton", rug.GetComponent<Button>());
            Assign(so, "desktopButton", desktop.GetComponent<Button>()); Assign(so, "toyButton", toy.GetComponent<Button>());
            Assign(so, "anchorTitleText", anchorTitle); Assign(so, "furnitureNameText", furnitureName);
            Assign(so, "countText", count); Assign(so, "descriptionText", description); Assign(so, "messageText", message);
            Assign(so, "previewImage", preview.GetComponent<Image>()); Assign(so, "previewGlyphText", glyph); Assign(so, "placedStatusText", placedStatus);
            Assign(so, "previousButton", previous.GetComponent<Button>()); Assign(so, "nextButton", next.GetComponent<Button>());
            Assign(so, "placeButton", place.GetComponent<Button>()); Assign(so, "placeButtonText", place.GetComponentInChildren<Text>());
            Assign(so, "removeButton", remove.GetComponent<Button>());
            Assign(so, "exchangeButton", exchange.GetComponent<Button>());
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Path);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }

        private static GameObject Tab(string text, Transform parent, float x) { var go = Button(text, parent, Card); Rect(go, x, -94, 82, 36); return go; }
        private static GameObject Box(string name, Transform parent, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); if (parent) go.transform.SetParent(parent, false); go.GetComponent<Image>().color = color; return go; }
        private static Text Label(string value, Transform parent, int size, Color color, bool bold = false, TextAnchor align = TextAnchor.MiddleLeft) { var go = new GameObject(string.IsNullOrEmpty(value) ? "Text" : value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false); var t = go.GetComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text = value; t.fontSize = size; t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; t.color = color; t.alignment = align; return t; }
        private static GameObject Button(string value, Transform parent, Color color) { var go = Box(value, parent, color); var b = go.AddComponent<Button>(); b.targetGraphic = go.GetComponent<Image>(); var t = Label(value, go.transform, 16, Text, false, TextAnchor.MiddleCenter); Stretch(t); return go; }
        private static void Stretch(Text text) { text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one; text.rectTransform.offsetMin = Vector2.zero; text.rectTransform.offsetMax = Vector2.zero; }
        private static void Assign(SerializedObject so, string property, Object value) => so.FindProperty(property).objectReferenceValue = value;
        private static void Rect(Component item, float x, float y, float w, float h, Vector2? anchor = null) => Rect(item.gameObject, x, y, w, h, anchor);
        private static void Rect(GameObject item, float x, float y, float w, float h, Vector2? anchor = null) { var a = anchor ?? new Vector2(0, 1); var r = item.GetComponent<RectTransform>(); r.anchorMin = a; r.anchorMax = a; r.pivot = a; r.anchoredPosition = new Vector2(x, y); r.sizeDelta = new Vector2(w, h); }
    }
}
#endif
