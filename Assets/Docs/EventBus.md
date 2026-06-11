# Game Event Bus

## Purpose

`GameEventBus` is the lightweight message layer for desktop pet systems. It lets modules talk through typed events instead of direct references.

Examples:

- input publishes `PetInteractionEvent`
- UI publishes `PanelOpenedEvent`
- window publishes `WindowClickThroughChangedEvent`
- future pet behavior, audio, save, and drop systems can subscribe independently

## Define An Event

Events should be small value types and implement `IGameEvent`.

```csharp
using DesktopPet.Events;

public readonly struct PetMoodChangedEvent : IGameEvent
{
    public readonly float Mood;

    public PetMoodChangedEvent(float mood)
    {
        Mood = mood;
    }
}
```

## Publish

```csharp
GameEventBus.Publish(new PetMoodChangedEvent(80f));
```

## Subscribe

Store the returned subscription and dispose it when the object is disabled or destroyed.

```csharp
using System;
using DesktopPet.Events;
using UnityEngine;

public class MoodView : MonoBehaviour
{
    private IDisposable subscription;

    private void OnEnable()
    {
        subscription = GameEventBus.Subscribe<PetMoodChangedEvent>(OnPetMoodChanged);
    }

    private void OnDisable()
    {
        subscription?.Dispose();
        subscription = null;
    }

    private void OnPetMoodChanged(PetMoodChangedEvent gameEvent)
    {
        Debug.Log(gameEvent.Mood);
    }
}
```

## Current Shared Events

- `WindowClickThroughChangedEvent`
- `WindowAlwaysOnTopChangedEvent`
- `WindowSettingsChangedEvent`
- `WindowDragStateChangedEvent`
- `WindowMovedEvent`
- `PanelOpenedEvent`
- `PanelClosedEvent`
- `PetScaleChangedEvent`
- `PetInteractionEvent`

## Debugging

Attach `GameEventDebugLogger` to a scene object to print common events in the Unity Console.
