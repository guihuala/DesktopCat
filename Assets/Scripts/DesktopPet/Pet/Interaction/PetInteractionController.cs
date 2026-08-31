using DesktopPet.Config;
using DesktopPet.Events;
using DesktopPet.Pet.Behavior;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DesktopPet.Pet.Interaction
{
    public sealed class PetInteractionController : MonoBehaviour
    {
        private PetBehaviorBrain brain;
        private PetTuningConfig tuning;
        private WindowController windowController;
        private float nextClickTime;

        public void Initialize(PetBehaviorBrain targetBrain, PetTuningConfig config)
        {
            brain = targetBrain;
            tuning = config;
            windowController = FindObjectOfType<WindowController>();
            EnsureCollider();
        }

        private void Update()
        {
            if (!UnityEngine.Input.GetMouseButtonDown(0) || Time.unscaledTime < nextClickTime) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (windowController != null && windowController.IsDragging) return;
            var camera = Camera.main;
            if (camera == null) return;
            var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit) || !hit.transform.IsChildOf(transform) && hit.transform != transform) return;
            nextClickTime = Time.unscaledTime + tuning.clickCooldown;
            GameEventBus.Publish(new PetInteractionEvent("click", hit.point));
            GameEventBus.Publish(new PetFeedbackEvent("被你发现啦！", true));
        }

        public void RequestFeed() { brain.RequestFeed(); GameEventBus.Publish(new PetFeedbackEvent("食物准备好了", false)); }
        public void RequestCall() => brain.RequestCall(false);

        private void EnsureCollider()
        {
            if (GetComponentInChildren<Collider>() != null) return;
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.center = transform.InverseTransformPoint(bounds.center);
            var localSize = transform.InverseTransformVector(bounds.size);
            collider.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        }
    }
}
