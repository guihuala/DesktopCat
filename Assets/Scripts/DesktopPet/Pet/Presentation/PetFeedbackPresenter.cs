using System;
using DG.Tweening;
using DesktopPet.Events;
using UnityEngine;
using UnityEngine.UI;

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
        private Canvas canvas;
        private RectTransform bubbleRoot;
        private CanvasGroup bubbleGroup;
        private Image bubbleBackground;
        private Text bubbleText;
        private Renderer[] petRenderers;

        private static readonly Color PositiveColor = new Color(1f, 0.86f, 0.9f, 0.96f);
        private static readonly Color NeutralColor = new Color(0.84f, 0.92f, 1f, 0.96f);

        private void OnEnable()
        {
            petRenderers = GetComponentsInChildren<Renderer>();
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
            if (bubbleRoot != null) Destroy(bubbleRoot.gameObject);
        }
        private void OnFeedback(PetFeedbackEvent item)
        {
            EnsureBubble();
            if (bubbleRoot == null) return;
            message = item.Message;
            positive = item.Positive;
            hideAt = Time.unscaledTime + 2.5f;
            bubbleText.text = message;
            bubbleBackground.color = positive ? PositiveColor : NeutralColor;
            bubbleGroup.alpha = 1f;
            bubbleRoot.gameObject.SetActive(true);
            bubbleRoot.localScale = Vector3.one * 0.86f;
            bubbleRoot.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack).SetUpdate(true);
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

        private void LateUpdate()
        {
            if (bubbleRoot == null || !bubbleRoot.gameObject.activeSelf) return;
            if (string.IsNullOrEmpty(message) || Time.unscaledTime >= hideAt)
            {
                bubbleRoot.gameObject.SetActive(false);
                return;
            }

            var camera = Camera.main;
            if (camera == null) return;
            var worldPoint = GetBubbleWorldPoint();
            var screenPoint = camera.WorldToScreenPoint(worldPoint);
            if (screenPoint.z <= 0f)
            {
                bubbleRoot.gameObject.SetActive(false);
                return;
            }

            var canvasRect = (RectTransform)canvas.transform;
            var eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, eventCamera, out var localPoint)) return;
            var halfWidth = bubbleRoot.rect.width * 0.5f;
            localPoint.x = Mathf.Clamp(localPoint.x, canvasRect.rect.xMin + halfWidth, canvasRect.rect.xMax - halfWidth);
            localPoint.y = Mathf.Clamp(localPoint.y, canvasRect.rect.yMin + 16f, canvasRect.rect.yMax - bubbleRoot.rect.height - 16f);
            bubbleRoot.anchoredPosition = localPoint;
        }

        private void EnsureBubble()
        {
            if (bubbleRoot != null) return;
            canvas = FindObjectOfType<Canvas>();
            var prefab = Resources.Load<GameObject>("UI/PetFeedbackBubble");
            if (canvas == null || prefab == null)
            {
                Debug.LogError("Pet feedback bubble needs a Canvas and Resources/UI/PetFeedbackBubble prefab.", this);
                return;
            }

            var instance = Instantiate(prefab, canvas.transform, false);
            bubbleRoot = instance.GetComponent<RectTransform>();
            bubbleGroup = instance.GetComponent<CanvasGroup>();
            bubbleBackground = instance.GetComponent<Image>();
            bubbleText = instance.GetComponentInChildren<Text>();
            instance.SetActive(false);
        }

        private Vector3 GetBubbleWorldPoint()
        {
            if (petRenderers == null || petRenderers.Length == 0)
                return transform.position + Vector3.up * 1.2f;

            var bounds = petRenderers[0].bounds;
            for (var i = 1; i < petRenderers.Length; i++)
                if (petRenderers[i] != null && petRenderers[i].enabled) bounds.Encapsulate(petRenderers[i].bounds);
            return new Vector3(bounds.center.x, bounds.max.y + 0.15f, bounds.center.z);
        }
    }
}
