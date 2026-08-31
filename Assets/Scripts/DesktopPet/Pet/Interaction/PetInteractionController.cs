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
        private AudioSource audioSource;
        private AudioClip callClip;

        public void Initialize(PetBehaviorBrain targetBrain, PetTuningConfig config)
        {
            brain = targetBrain;
            tuning = config;
            windowController = FindObjectOfType<WindowController>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
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
        public void RequestCall() { PlayCallSound(); brain.RequestCall(false); }

        private void PlayCallSound()
        {
            if (callClip == null)
            {
                const int sampleRate = 22050;
                const int sampleCount = 3307;
                var samples = new float[sampleCount];
                for (var i = 0; i < sampleCount; i++)
                {
                    var envelope = 1f - i / (float)sampleCount;
                    samples[i] = Mathf.Sin(2f * Mathf.PI * 660f * i / sampleRate) * envelope * 0.15f;
                }
                callClip = AudioClip.Create("DefaultPetCall", sampleCount, 1, sampleRate, false);
                callClip.SetData(samples, 0);
            }
            audioSource.PlayOneShot(callClip);
        }

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
