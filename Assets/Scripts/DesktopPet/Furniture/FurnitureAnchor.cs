using UnityEngine;

namespace DesktopPet.Furniture
{
    public sealed class FurnitureAnchor : MonoBehaviour
    {
        [SerializeField] private FurnitureAnchorType anchorType;
        [SerializeField] private Transform contentRoot;
        public FurnitureAnchorType AnchorType => anchorType;
        public Transform ContentRoot => contentRoot != null ? contentRoot : transform;
    }
}
