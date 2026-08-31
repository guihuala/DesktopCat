using DesktopPet.Events;
using UnityEngine;

namespace DesktopPet.Input
{
    public enum PlayerActivityLevel { Idle, Normal, Active }

    public sealed class PlayerActivityTracker : MonoBehaviour
    {
        [SerializeField] private float sampleWindow = 8f;
        [SerializeField] private float idleAfter = 12f;
        [SerializeField] private float activeScore = 8f;
        private Vector3 lastMousePosition;
        private float score;
        private float lastInputTime;
        private PlayerActivityLevel level;

        public PlayerActivityLevel Level => level;

        private void Awake() { lastMousePosition = UnityEngine.Input.mousePosition; lastInputTime = Time.unscaledTime; }

        private void Update()
        {
            if (!Application.isFocused) return;
            var mouse = UnityEngine.Input.mousePosition;
            var moved = (mouse - lastMousePosition).sqrMagnitude > 4f;
            var clicked = UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonDown(1);
            if (moved || clicked)
            {
                score += clicked ? 2f : 1f;
                lastInputTime = Time.unscaledTime;
            }
            lastMousePosition = mouse;
            score = Mathf.MoveTowards(score, 0f, Time.unscaledDeltaTime * activeScore / Mathf.Max(1f, sampleWindow));
            var next = Time.unscaledTime - lastInputTime >= idleAfter ? PlayerActivityLevel.Idle : score >= activeScore ? PlayerActivityLevel.Active : PlayerActivityLevel.Normal;
            if (next == level) return;
            level = next;
            GameEventBus.Publish(new PlayerActivityChangedEvent(level.ToString()));
        }
    }
}
