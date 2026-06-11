# Save System

## Purpose

`SaveManager` stores lightweight desktop pet settings as JSON under `Application.persistentDataPath`.

Current saved data:

- window options: always on top, borderless, transparent background, click-through, drag enabled
- window position
- pet scale
- audio settings placeholders: master volume, SFX volume
- privacy placeholder: microphone enabled

The save file is named:

```text
desktop_pet_save.json
```

## Runtime Behavior

`SaveManager` bootstraps itself automatically after the scene loads. A scene object is optional.

Startup flow:

1. Create or find `SaveManager`.
2. Load JSON from `Application.persistentDataPath`.
3. Find `WindowController` and the pet root.
4. Apply saved window settings, position, and pet scale.
5. Subscribe to game events.

Runtime save flow:

1. Window, UI, or pet systems publish typed events.
2. `SaveManager` updates the in-memory `DesktopPetSaveData`.
3. A short debounce writes JSON to disk.
4. Application pause and quit force a save.

## Events Used

- `WindowSettingsChangedEvent`
- `WindowMovedEvent`
- `PetScaleChangedEvent`

## Extending

Add new fields to `DesktopPetSaveData`, then subscribe to the event that owns that state.

Recommended future groups:

- pet stats
- inventory
- placed furniture
- unlocked content
- last exit time for offline settlement

Keep stable save field names where possible. When fields must change, increment `saveVersion` and migrate older data during `Load`.
