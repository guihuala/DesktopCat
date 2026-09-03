using UnityEngine;

namespace DesktopPet.Furniture
{
    public sealed class FurnitureAnchor : MonoBehaviour
    {
        [SerializeField] private FurnitureAnchorType anchorType;
        [SerializeField] private Transform contentRoot;
        public FurnitureAnchorType AnchorType => anchorType;
        public Transform ContentRoot => contentRoot != null ? contentRoot : transform;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.98f, 0.69f, 0.38f, 0.9f);
            Gizmos.DrawWireSphere(ContentRoot.position, 0.08f);
            Gizmos.DrawLine(ContentRoot.position, ContentRoot.position + Vector3.up * 0.25f);
        }
#endif
    }
}
