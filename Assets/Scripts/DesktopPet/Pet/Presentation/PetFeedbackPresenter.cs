using System;
using DG.Tweening;
using DesktopPet.Events;
using UnityEngine;

namespace DesktopPet.Pet.Presentation
{
    public sealed class PetFeedbackPresenter : MonoBehaviour
    {
        private IDisposable subscription;
        private IDisposable interactionSubscription;
        private IDisposable scaleSubscription;
        private Sequence reaction;
        private Vector3 restingScale;
        [SerializeField, Range(0f, 0.15f)] private float squashAmount = 0.045f;
        [SerializeField, Min(0.05f)] private float pressDuration = 0.1f;
        [SerializeField, Min(0.05f)] private float releaseDuration = 0.28f;
        private string message;
        private float hideAt;
        private bool positive;

        private void OnEnable()
        {
            subscription = GameEventBus.Subscribe<PetFeedbackEvent>(OnFeedback);
            interactionSubscription = GameEventBus.Subscribe<PetInteractionEvent>(OnInteraction);
            // Settings has already applied the new scale before publishing this event.
            scaleSubscription = GameEventBus.Subscribe<PetScaleChangedEvent>(_ => StopReaction(false));
        }

        private void OnDisable()
        {
            subscription?.Dispose(); subscription = null;
            interactionSubscription?.Dispose(); interactionSubscription = null;
            scaleSubscription?.Dispose(); scaleSubscription = null;
            StopReaction(true);
        }
        private void OnFeedback(PetFeedbackEvent item)
        {
            message = item.Message; positive = item.Positive; hideAt = Time.unscaledTime + 2.5f;
        }

        private void OnInteraction(PetInteractionEvent item)
        {
            if (item.InteractionId != "click") return;
            StopReaction(true);
            message = null;
            restingScale = transform.localScale;
            var squashed = Vector3.Scale(restingScale, new Vector3(1f + squashAmount, 1f - squashAmount, 1f + squashAmount));
            reaction = DOTween.Sequence().SetUpdate(true);
            reaction.Append(transform.DOScale(squashed, pressDuration).SetEase(Ease.OutQuad));
            reaction.Append(transform.DOScale(restingScale, releaseDuration).SetEase(Ease.OutBack));
            reaction.OnComplete(() => reaction = null);
        }

        private void StopReaction(bool restoreScale)
        {
            if (reaction == null) return;
            reaction.Kill();
            reaction = null;
            if (restoreScale) transform.localScale = restingScale;
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
