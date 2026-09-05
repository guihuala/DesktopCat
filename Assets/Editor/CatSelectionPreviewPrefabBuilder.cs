#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DesktopPet.Editor
{
    [InitializeOnLoad]
    public static class CatSelectionPreviewPrefabBuilder
    {
        private const string ModelPath = "Assets/Art/Model/cat02.fbx";
        private const string FolderPath = "Assets/Resources/Pet";
        private const string PrefabPath = FolderPath + "/CatSelectionPreview.prefab";

        static CatSelectionPreviewPrefabBuilder()
        {
            EditorApplication.delayCall += BuildIfMissing;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += BuildIfMissing;
            };
        }

        private static void BuildIfMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) Build();
        }

        [MenuItem("Desktop Pet/重建选猫预览 Prefab")]
        public static void Rebuild()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
                AssetDatabase.DeleteAsset(PrefabPath);
            Build();
        }

        private static void Build()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                Debug.LogWarning("找不到用于选择界面的猫咪模型：" + ModelPath);
                return;
            }
            if (!AssetDatabase.IsValidFolder(FolderPath))
                AssetDatabase.CreateFolder("Assets/Resources", "Pet");

            var preview = Object.Instantiate(model);
            preview.name = "CatSelectionPreview";
            RemoveGameplayComponents(preview);
            PrefabUtility.SaveAsPrefabAsset(preview, PrefabPath);
            Object.DestroyImmediate(preview);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RemoveGameplayComponents(GameObject root)
        {
            foreach (var body in root.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(body);
            foreach (var collider in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            foreach (var audio in root.GetComponentsInChildren<AudioSource>(true)) Object.DestroyImmediate(audio);
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(behaviour);
        }
    }
}
#endif
