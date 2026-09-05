using UnityEngine;

namespace DesktopPet.Furniture
{
    [CreateAssetMenu(menuName = "Desktop Pet/Furniture Exchange", fileName = "FurnitureExchangeConfig")]
    public sealed class FurnitureExchangeConfig : ScriptableObject
    {
        [Min(2)] public int requiredCopies = 3;
        [Range(0f, 1f)] public float commonUpgradeChance = 0.2f;
        [Range(0f, 1f)] public float rareUpgradeChance = 0.1f;
    }
}
