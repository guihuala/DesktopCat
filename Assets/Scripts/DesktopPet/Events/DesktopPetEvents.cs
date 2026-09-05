using UnityEngine;

namespace DesktopPet.Events
{
    public readonly struct WindowClickThroughChangedEvent : IGameEvent
    {
        public readonly bool IsClickThrough;

        public WindowClickThroughChangedEvent(bool isClickThrough)
        {
            IsClickThrough = isClickThrough;
        }
    }

    public readonly struct WindowAlwaysOnTopChangedEvent : IGameEvent
    {
        public readonly bool IsAlwaysOnTop;

        public WindowAlwaysOnTopChangedEvent(bool isAlwaysOnTop)
        {
            IsAlwaysOnTop = isAlwaysOnTop;
        }
    }

    public readonly struct WindowSettingsChangedEvent : IGameEvent
    {
        public readonly bool AlwaysOnTop;
        public readonly bool Borderless;
        public readonly bool TransparentBackground;
        public readonly bool ClickThrough;
        public readonly bool AllowDrag;

        public WindowSettingsChangedEvent(
            bool alwaysOnTop,
            bool borderless,
            bool transparentBackground,
            bool clickThrough,
            bool allowDrag)
        {
            AlwaysOnTop = alwaysOnTop;
            Borderless = borderless;
            TransparentBackground = transparentBackground;
            ClickThrough = clickThrough;
            AllowDrag = allowDrag;
        }
    }

    public readonly struct WindowDragStateChangedEvent : IGameEvent
    {
        public readonly bool IsDragging;

        public WindowDragStateChangedEvent(bool isDragging)
        {
            IsDragging = isDragging;
        }
    }

    public readonly struct WindowMovedEvent : IGameEvent
    {
        public readonly Vector2Int Position;

        public WindowMovedEvent(Vector2Int position)
        {
            Position = position;
        }
    }

    public readonly struct PanelOpenedEvent : IGameEvent
    {
        public readonly string PanelId;

        public PanelOpenedEvent(string panelId)
        {
            PanelId = panelId;
        }
    }

    public readonly struct PanelClosedEvent : IGameEvent
    {
        public readonly string PanelId;

        public PanelClosedEvent(string panelId)
        {
            PanelId = panelId;
        }
    }

    public readonly struct PetScaleChangedEvent : IGameEvent
    {
        public readonly float Scale;

        public PetScaleChangedEvent(float scale)
        {
            Scale = scale;
        }
    }

    public readonly struct PetInteractionEvent : IGameEvent
    {
        public readonly string InteractionId;
        public readonly Vector3 WorldPosition;

        public PetInteractionEvent(string interactionId, Vector3 worldPosition)
        {
            InteractionId = interactionId;
            WorldPosition = worldPosition;
        }
    }

    public readonly struct PetStatsChangedEvent : IGameEvent
    {
        public readonly float Energy;
        public readonly float Hunger;
        public PetStatsChangedEvent(float energy, float hunger) { Energy = energy; Hunger = hunger; }
    }

    public readonly struct PetBehaviourChangedEvent : IGameEvent
    {
        public readonly string BehaviourId;
        public readonly bool IsUninterruptible;
        public PetBehaviourChangedEvent(string behaviourId, bool isUninterruptible)
        { BehaviourId = behaviourId; IsUninterruptible = isUninterruptible; }
    }

    public enum FeedbackPriority { Ambient = 0, Normal = 1, Important = 2 }

    public readonly struct PetFeedbackEvent : IGameEvent
    {
        public readonly string Message;
        public readonly bool Positive;
        public readonly FeedbackPriority Priority;
        public readonly float Duration;

        public PetFeedbackEvent(string message, bool positive,
            FeedbackPriority priority = FeedbackPriority.Normal, float duration = 2.5f)
        {
            Message = message;
            Positive = positive;
            Priority = priority;
            Duration = duration;
        }
    }

    public readonly struct PlayerActivityChangedEvent : IGameEvent
    {
        public readonly string Level;
        public PlayerActivityChangedEvent(string level) { Level = level; }
    }

    public readonly struct DayNightModeChangedEvent : IGameEvent
    {
        public readonly int Mode;
        public DayNightModeChangedEvent(int mode) { Mode = mode; }
    }

    public readonly struct OnlineRewardProgressChangedEvent : IGameEvent
    {
        public readonly double ElapsedSeconds;
        public readonly double IntervalSeconds;
        public readonly int PendingRewards;
        public readonly int MaxPendingRewards;

        public OnlineRewardProgressChangedEvent(double elapsedSeconds, double intervalSeconds, int pendingRewards, int maxPendingRewards)
        {
            ElapsedSeconds = elapsedSeconds;
            IntervalSeconds = intervalSeconds;
            PendingRewards = pendingRewards;
            MaxPendingRewards = maxPendingRewards;
        }
    }

    public readonly struct FurnitureInventoryChangedEvent : IGameEvent
    {
        public readonly string FurnitureId;
        public readonly int TotalOwned;
        public readonly int PlacedCount;
        public readonly bool FirstDiscovery;

        public FurnitureInventoryChangedEvent(string furnitureId, int totalOwned, int placedCount, bool firstDiscovery)
        {
            FurnitureId = furnitureId;
            TotalOwned = totalOwned;
            PlacedCount = placedCount;
            FirstDiscovery = firstDiscovery;
        }
    }
}
