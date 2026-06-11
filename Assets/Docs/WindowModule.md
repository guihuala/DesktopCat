# Desktop Pet Window Module

## Purpose

The window module owns desktop-pet specific window behavior:

- borderless transparent window
- always-on-top mode
- click-through mode
- drag movement
- screen bounds clamping
- edge snapping
- simple runtime presets

The main component is `WindowController`.

## Components

### WindowController

Attach this to a scene object that is active when the desktop pet starts.

Important inspector fields:

- `Apply On Start`: applies the current window options on startup.
- `Always On Top`: keeps the pet above normal desktop windows.
- `Borderless`: removes the normal Windows frame.
- `Transparent Background`: uses a color key to make the background transparent.
- `Click Through`: lets mouse input pass through the pet window.
- `Allow Drag`: allows window movement.
- `Drag Mode`: choose whether the whole non-UI window can drag or only manual drag areas can.
- `Keep Inside Screen`: prevents the window from being dragged completely off screen.
- `Snap To Screen Edge`: snaps the window when released near an edge.
- `Screen Padding`: distance kept from screen edges.
- `Snap Distance`: distance threshold for edge snapping.

Public APIs commonly used by other modules:

```csharp
windowController.SetClickThrough(true);
windowController.SetAlwaysOnTop(true);
windowController.SetBorderless(true);
windowController.SetTransparentBackground(true);
windowController.SetAllowDrag(true);
windowController.CenterOnScreen();
windowController.SetWindowPosition(100, 100);
windowController.MoveWindowBy(new Vector2Int(20, 0));
windowController.ApplyPreset(WindowController.WindowPreset.DesktopPet);
```

### WindowDragArea

Attach this to a UI object or clickable pet hit area when `WindowController.Drag Mode` is set to `Manual Drag Area Only`.

This is useful when designers want only the cat body, a handle, or a transparent UI region to drag the desktop pet.

## Presets

`DesktopPet`:

- always on top
- borderless
- transparent
- keeps current click-through state
- drag enabled

`DebugWindow`:

- normal window
- not click-through
- drag enabled

`Presentation`:

- always on top
- borderless
- transparent
- not click-through
- drag disabled

## Unity Editor Notes

Most native window behavior only runs in Windows standalone builds. In the Unity Editor, the component keeps the same serialized settings and API surface, but native transparency, click-through, and window movement are not fully applied.

Regularly test this module with a Windows standalone build because desktop transparency and click-through cannot be fully validated in play mode.
