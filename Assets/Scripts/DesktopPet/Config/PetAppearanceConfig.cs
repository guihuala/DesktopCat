using UnityEngine;

namespace DesktopPet.Config
{
    [CreateAssetMenu(menuName = "Desktop Pet/Pet Appearance", fileName = "PetAppearanceConfig")]
    public sealed class PetAppearanceConfig : ScriptableObject
    {
        public Color warmFur = new Color(1f, 0.72f, 0.48f, 1f);
        public Color coolFur = new Color(0.58f, 0.68f, 0.84f, 1f);
    }
}
