using UnityEngine;

namespace DesktopPet.Furniture
{
    [CreateAssetMenu(menuName = "Desktop Pet/Furniture Drop Config", fileName = "FurnitureDropConfig")]
    public sealed class FurnitureDropConfig : ScriptableObject
    {
        [Range(0f, 100f)] public float commonWeight = 88f;
        [Range(0f, 100f)] public float rareWeight = 10f;
        [Range(0f, 100f)] public float collectibleWeight = 2f;

        public float TotalWeight => commonWeight + rareWeight + collectibleWeight;

        private void OnValidate()
        {
            if (TotalWeight <= 0f) commonWeight = 100f;
        }
    }
}
