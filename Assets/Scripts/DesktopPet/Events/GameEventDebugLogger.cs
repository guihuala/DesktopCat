using System;
using UnityEngine;

namespace DesktopPet.Events
{
    public class GameEventDebugLogger : MonoBehaviour
    {
        [SerializeField] private bool logWindowEvents = true;
        [SerializeField] private bool logPanelEvents = true;
        [SerializeField] private bool logPetEvents = true;

        private readonly CompositeSubscription subscriptions = new CompositeSubscription();

        private void OnEnable()
        {
            if (logWindowEvents)
            {
                subscriptions.Add(GameEventBus.Subscribe<WindowClickThroughChangedEvent>(OnWindowClickThroughChanged));
                subscriptions.Add(GameEventBus.Subscribe<WindowAlwaysOnTopChangedEvent>(OnWindowAlwaysOnTopChanged));
                subscriptions.Add(GameEventBus.Subscribe<WindowDragStateChangedEvent>(OnWindowDragStateChanged));
                subscriptions.Add(GameEventBus.Subscribe<WindowMovedEvent>(OnWindowMoved));
            }

            if (logPanelEvents)
            {
                subscriptions.Add(GameEventBus.Subscribe<PanelOpenedEvent>(OnPanelOpened));
                subscriptions.Add(GameEventBus.Subscribe<PanelClosedEvent>(OnPanelClosed));
            }

            if (logPetEvents)
            {
                subscriptions.Add(GameEventBus.Subscribe<PetScaleChangedEvent>(OnPetScaleChanged));
                subscriptions.Add(GameEventBus.Subscribe<PetInteractionEvent>(OnPetInteraction));
            }
        }

        private void OnDisable()
        {
            subscriptions.Clear();
        }

        private static void OnWindowClickThroughChanged(WindowClickThroughChangedEvent gameEvent)
        {
            Debug.Log($"Window click-through: {gameEvent.IsClickThrough}");
        }

        private static void OnWindowAlwaysOnTopChanged(WindowAlwaysOnTopChangedEvent gameEvent)
        {
            Debug.Log($"Window always-on-top: {gameEvent.IsAlwaysOnTop}");
        }

        private static void OnWindowDragStateChanged(WindowDragStateChangedEvent gameEvent)
        {
            Debug.Log($"Window dragging: {gameEvent.IsDragging}");
        }

        private static void OnWindowMoved(WindowMovedEvent gameEvent)
        {
            Debug.Log($"Window moved: {gameEvent.Position}");
        }

        private static void OnPanelOpened(PanelOpenedEvent gameEvent)
        {
            Debug.Log($"Panel opened: {gameEvent.PanelId}");
        }

        private static void OnPanelClosed(PanelClosedEvent gameEvent)
        {
            Debug.Log($"Panel closed: {gameEvent.PanelId}");
        }

        private static void OnPetScaleChanged(PetScaleChangedEvent gameEvent)
        {
            Debug.Log($"Pet scale changed: {gameEvent.Scale:0.00}");
        }

        private static void OnPetInteraction(PetInteractionEvent gameEvent)
        {
            Debug.Log($"Pet interaction: {gameEvent.InteractionId} at {gameEvent.WorldPosition}");
        }

        private sealed class CompositeSubscription
        {
            private readonly System.Collections.Generic.List<IDisposable> items = new System.Collections.Generic.List<IDisposable>();

            public void Add(IDisposable item)
            {
                if (item != null)
                {
                    items.Add(item);
                }
            }

            public void Clear()
            {
                for (var i = 0; i < items.Count; i++)
                {
                    items[i].Dispose();
                }

                items.Clear();
            }
        }
    }
}
