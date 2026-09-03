#if UNITY_EDITOR
using DesktopPet.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.Editor
{
    [InitializeOnLoad]
    public static class RewardClaimPanelPrefabBuilder
    {
        private const string PrefabPath = "Assets/Resources/UI/RewardClaimPanel.prefab";
        private static readonly Color Panel = new Color(0.12f, 0.14f, 0.18f, 0.97f);
        private static readonly Color Card = new Color(0.19f, 0.22f, 0.28f, 1f);
        private static readonly Color Accent = new Color(0.98f, 0.69f, 0.38f, 1f);
        private static readonly Color MainText = new Color(0.94f, 0.95f, 0.97f, 1f);
        private static readonly Color MutedText = new Color(0.68f, 0.72f, 0.78f, 1f);

        static RewardClaimPanelPrefabBuilder()
        {
            EditorApplication.delayCall += BuildIfMissing;
        }

        private static void BuildIfMissing()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) Build();
            else if (prefab.transform.Find("布置房间") == null) Rebuild();
        }

        [MenuItem("Desktop Pet/重建家具领取面板 Prefab")]
        public static void Rebuild()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) AssetDatabase.DeleteAsset(PrefabPath);
            Build();
        }

        private static void Build()
        {
            var root = Box("RewardClaimPanel", null, Panel);
            root.layer = 5;
            var rootRect = root.GetComponent<RectTransform>();
            SetRect(rootRect, new Vector2(-18f, -18f), new Vector2(390f, 475f), Vector2.one);
            var panel = root.AddComponent<RewardClaimPanel>();
            root.AddComponent<UIDraggablePanel>();

            var title = Label("家具礼物", root.transform, 24, MainText, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetRect(title.rectTransform, new Vector2(24f, -18f), new Vector2(250f, 38f), new Vector2(0f, 1f));
            var close = Button("收起", root.transform, Card);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(-20f, -18f), new Vector2(72f, 34f), Vector2.one);

            var pending = Label("待领取家具  0 件", root.transform, 22, Accent, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetRect(pending.rectTransform, new Vector2(24f, -76f), new Vector2(342f, 36f), new Vector2(0f, 1f));
            var progress = Label("下一件还需 30.0 分钟", root.transform, 15, MutedText, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(progress.rectTransform, new Vector2(24f, -116f), new Vector2(342f, 28f), new Vector2(0f, 1f));

            var resultCard = Box("领取结果卡片", root.transform, Card);
            SetRect(resultCard.GetComponent<RectTransform>(), new Vector2(24f, -162f), new Vector2(342f, 172f), new Vector2(0f, 1f));
            var result = Label("积攒在线时间后，家具会出现在这里。", resultCard.transform, 16, MainText, FontStyle.Normal, TextAnchor.UpperLeft);
            result.horizontalOverflow = HorizontalWrapMode.Wrap;
            result.verticalOverflow = VerticalWrapMode.Truncate;
            result.rectTransform.anchorMin = Vector2.zero;
            result.rectTransform.anchorMax = Vector2.one;
            result.rectTransform.offsetMin = new Vector2(16f, 14f);
            result.rectTransform.offsetMax = new Vector2(-16f, -14f);

            var claim = Button("全部领取", root.transform, Accent);
            SetRect(claim.GetComponent<RectTransform>(), new Vector2(24f, -354f), new Vector2(342f, 48f), new Vector2(0f, 1f));
            var placement = Button("布置房间", root.transform, Card);
            SetRect(placement.GetComponent<RectTransform>(), new Vector2(24f, -412f), new Vector2(342f, 38f), new Vector2(0f, 1f));

            var serialized = new SerializedObject(panel);
            serialized.FindProperty("pendingText").objectReferenceValue = pending;
            serialized.FindProperty("progressText").objectReferenceValue = progress;
            serialized.FindProperty("resultText").objectReferenceValue = result;
            serialized.FindProperty("claimButton").objectReferenceValue = claim.GetComponent<Button>();
            serialized.FindProperty("closeButton").objectReferenceValue = close.GetComponent<Button>();
            serialized.FindProperty("placementButton").objectReferenceValue = placement.GetComponent<Button>();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"已生成家具领取面板：{PrefabPath}");
        }

        private static GameObject Box(string name, Transform parent, Color color)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (parent != null) item.transform.SetParent(parent, false);
            item.GetComponent<Image>().color = color;
            return item;
        }

        private static Text Label(string value, Transform parent, int size, Color color, FontStyle style, TextAnchor anchor)
        {
            var item = new GameObject(value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            item.transform.SetParent(parent, false);
            var text = item.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            return text;
        }

        private static GameObject Button(string label, Transform parent, Color color)
        {
            var root = Box(label, parent, color);
            var button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            var text = Label(label, root.transform, 16, MainText, FontStyle.Normal, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return root;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
#endif
