using DesktopPet.Config;
using DesktopPet.Pet.Behavior;
using DesktopPet.Pet.Movement;
using DesktopPet.Pet.Presentation;
using DesktopPet.Pet.State;
using UnityEngine;

namespace DesktopPet
{
    [DefaultExecutionOrder(-200)]
    public sealed class DesktopPetBootstrap : MonoBehaviour
    {
        [SerializeField] private string petRootName = "Monkey";
        [SerializeField] private PetTuningConfig tuning;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (FindObjectOfType<DesktopPetBootstrap>() != null) return;
            new GameObject("DesktopPetBootstrap").AddComponent<DesktopPetBootstrap>();
        }

        private void Start()
        {
            var pet = GameObject.Find(petRootName);
            if (pet == null) { Debug.LogError($"Desktop pet bootstrap could not find pet root '{petRootName}'."); return; }
            if (tuning == null) tuning = Resources.Load<PetTuningConfig>("Config/PetTuningConfig");
            if (tuning == null) tuning = PetTuningConfig.CreateRuntimeDefaults();
            GetOrAdd<PetStateController>(pet);
            GetOrAdd<PetMovementController>(pet);
            GetOrAdd<PetPresentationController>(pet);
            var brain = GetOrAdd<PetBehaviorBrain>(pet);
            brain.Initialize(tuning);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GetOrAdd<PrototypeDebugPanel>(pet);
#endif
            Debug.Log("Desktop pet runtime initialized.");
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
