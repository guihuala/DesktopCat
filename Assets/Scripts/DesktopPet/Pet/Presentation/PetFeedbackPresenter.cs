using System;
using System.Collections;
using DesktopPet.Events;
using UnityEngine;

namespace DesktopPet.Pet.Presentation
{
    public sealed class PetFeedbackPresenter : MonoBehaviour
    {
        private IDisposable subscription;
        private string message;
        private float hideAt;
        private bool positive;

        private void OnEnable() => subscription = GameEventBus.Subscribe<PetFeedbackEvent>(OnFeedback);
        private void OnDisable() { subscription?.Dispose(); subscription = null; }
        private Coroutine pulse;
        private void OnFeedback(PetFeedbackEvent item)
        {
            message = item.Message; positive = item.Positive; hideAt = Time.unscaledTime + 2.5f;
            if (item.Positive) { if (pulse != null) StopCoroutine(pulse); pulse = StartCoroutine(Pulse()); }
        }

        private IEnumerator Pulse()
        {
            var original = transform.localScale;
            var enlarged = original * 1.04f;
            for (var elapsed = 0f; elapsed < 0.12f; elapsed += Time.unscaledDeltaTime)
            { transform.localScale = Vector3.Lerp(original, enlarged, elapsed / 0.12f); yield return null; }
            for (var elapsed = 0f; elapsed < 0.16f; elapsed += Time.unscaledDeltaTime)
            { transform.localScale = Vector3.Lerp(enlarged, original, elapsed / 0.16f); yield return null; }
            transform.localScale = original; pulse = null;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(message) || Time.unscaledTime >= hideAt) return;
            var camera = Camera.main;
            if (camera == null) return;
            var point = camera.WorldToScreenPoint(transform.position + Vector3.up * 1.2f);
            if (point.z <= 0f) return;
            var rect = new Rect(point.x - 90f, Screen.height - point.y - 45f, 180f, 36f);
            var old = GUI.color; GUI.color = positive ? new Color(1f, 0.85f, 0.9f) : new Color(0.85f, 0.9f, 1f);
            GUI.Box(rect, message); GUI.color = old;
        }
    }
}
