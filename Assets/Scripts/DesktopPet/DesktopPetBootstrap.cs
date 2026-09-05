using DesktopPet.Config;
using DesktopPet.Pet.Behavior;
using DesktopPet.Pet.Movement;
using DesktopPet.Pet.Presentation;
using DesktopPet.Pet.State;
using UnityEngine;
using DesktopPet.Activity;
using DesktopPet.Pet.Interaction;
using DesktopPet.UI;
using DesktopPet.Presentation;
using DesktopPet.Rewards;
using DesktopPet.Furniture;
using DesktopPet.Save;

namespace DesktopPet
{
    [DefaultExecutionOrder(-200)]
    public sealed class DesktopPetBootstrap : MonoBehaviour
    {
        [SerializeField] private string petRootName = "Cat";
        [SerializeField] private PetTuningConfig tuning;
        private GameObject pet;
        private bool gameplayStarted;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (FindObjectOfType<DesktopPetBootstrap>() != null) return;
            new GameObject("DesktopPetBootstrap").AddComponent<DesktopPetBootstrap>();
        }

        private void Start()
        {
            pet = GameObject.Find(petRootName);
            if (pet == null) { Debug.LogError($"Desktop pet bootstrap could not find pet root '{petRootName}'."); return; }
            if (tuning == null) tuning = Resources.Load<PetTuningConfig>("Config/PetTuningConfig");
            if (tuning == null) tuning = PetTuningConfig.CreateRuntimeDefaults();
            var appearance = GetOrAdd<PetAppearanceController>(pet);
            appearance.Initialize();

            InitializeSharedServices();
            var savedAppearance = SaveManager.Data != null ? SaveManager.Data.appearance : null;
            if (savedAppearance != null && savedAppearance.hasChosenPet) StartPetGameplay();
            else Debug.Log("Cat selection preview initialized; pet gameplay is waiting for confirmation.");
        }

        private void InitializeSharedServices()
        {
            GetOrAdd<DayNightController>(gameObject);
            GetOrAdd<OnlineRewardService>(gameObject);
            GetOrAdd<FurnitureDropService>(gameObject);
            GetOrAdd<FurnitureInventory>(gameObject);
            GetOrAdd<FurnitureExchangeService>(gameObject);
            GetOrAdd<FurnitureRewardClaimService>(gameObject);
            GetOrAdd<FurniturePlacementController>(gameObject);
        }

        public void StartPetGameplay()
        {
            if (gameplayStarted) return;
            if (pet == null) pet = GameObject.Find(petRootName);
            if (pet == null) { Debug.LogError($"Desktop pet bootstrap could not find pet root '{petRootName}'."); return; }
            if (tuning == null) tuning = Resources.Load<PetTuningConfig>("Config/PetTuningConfig");
            if (tuning == null) tuning = PetTuningConfig.CreateRuntimeDefaults();

            gameplayStarted = true;
            ConfigurePhysics(pet);
            var state = GetOrAdd<PetStateController>(pet);
            GetOrAdd<PetMovementController>(pet);
            GetOrAdd<PetPresentationController>(pet);
            GetOrAdd<PlayerActivityTracker>(gameObject);
            var brain = GetOrAdd<PetBehaviorBrain>(pet);
            brain.Initialize(tuning);
            var savedPet = SaveManager.Data != null ? SaveManager.Data.pet : null;
            if (savedPet != null && savedPet.hasRuntimeStats)
                state.SetStats(savedPet.energy, savedPet.hunger);
            var interaction = GetOrAdd<PetInteractionController>(pet);
            interaction.Initialize(brain, tuning);
            GetOrAdd<PetFeedbackPresenter>(pet);
            var tray = FindObjectOfType<PhoneTrayController>();
            if (tray != null) tray.Initialize(interaction);
            else Debug.LogError("PhoneTrayController is missing from the scene HUD.");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GetOrAdd<PrototypeDebugPanel>(pet);
#endif
            Debug.Log("Desktop pet gameplay initialized after cat selection.");
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static void ConfigurePhysics(GameObject pet)
        {
            var body = GetOrAdd<Rigidbody>(pet);
            body.useGravity = true;
            body.isKinematic = false;
            body.mass = 1f;
            body.drag = 1f;
            body.angularDrag = 5f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }
}
