using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DesktopPet.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DesktopPet
{
    public class WindowController : MonoBehaviour
    {
        public enum WindowPreset
        {
            DesktopPet,
            DebugWindow,
            Presentation
        }

        public enum DragMode
        {
            AnywhereExceptUI,
            ManualDragAreaOnly
        }

        [Header("Window")]
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private bool alwaysOnTop = true;
        [SerializeField] private bool borderless = true;
        [SerializeField] private bool transparentBackground = true;
        [SerializeField] private bool clickThrough = false;
        [SerializeField] private Color transparentColor = Color.black;

        [Header("Drag")]
        [SerializeField] private bool allowDrag = true;
        [SerializeField] private DragMode dragMode = DragMode.AnywhereExceptUI;
        [SerializeField] private int dragMouseButton = 0;
        [SerializeField] private bool keepInsideScreen = true;
        [SerializeField] private bool snapToScreenEdge = true;
        [SerializeField] private int screenPadding = 8;
        [SerializeField] private int snapDistance = 24;

        [Header("Debug")]
        [SerializeField] private KeyCode toggleClickThroughKey = KeyCode.F8;
        [SerializeField] private KeyCode toggleAlwaysOnTopKey = KeyCode.F9;
        [SerializeField] private KeyCode centerWindowKey = KeyCode.F10;

        private IntPtr windowHandle;
        private bool isDragging;
        private Vector2Int dragMouseStart;
        private RectInt dragWindowStart;

        public bool IsClickThrough => clickThrough;
        public bool IsAlwaysOnTop => alwaysOnTop;
        public bool IsBorderless => borderless;
        public bool IsTransparentBackground => transparentBackground;
        public bool AllowDrag => allowDrag;
        public bool IsDragging => isDragging;

        private void Start()
        {
            if (applyOnStart)
            {
                ApplyWindowSettings();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleClickThroughKey))
            {
                ToggleClickThrough();
            }

            if (Input.GetKeyDown(toggleAlwaysOnTopKey))
            {
                SetAlwaysOnTop(!alwaysOnTop);
            }

            if (Input.GetKeyDown(centerWindowKey))
            {
                CenterOnScreen();
            }

            if (!allowDrag || clickThrough)
            {
                return;
            }

            UpdateDrag();
        }

        public void ApplyPreset(WindowPreset preset)
        {
            switch (preset)
            {
                case WindowPreset.DesktopPet:
                    SetWindowOptions(true, true, true, clickThrough, true);
                    break;
                case WindowPreset.DebugWindow:
                    SetWindowOptions(false, false, false, false, true);
                    break;
                case WindowPreset.Presentation:
                    SetWindowOptions(true, true, true, false, false);
                    break;
            }
        }

        public void SetWindowOptions(
            bool newAlwaysOnTop,
            bool newBorderless,
            bool newTransparentBackground,
            bool newClickThrough,
            bool newAllowDrag)
        {
            alwaysOnTop = newAlwaysOnTop;
            borderless = newBorderless;
            transparentBackground = newTransparentBackground;
            clickThrough = newClickThrough;
            allowDrag = newAllowDrag;
            ApplyWindowSettings();
            PublishWindowSettingsChanged();
        }

        public void SetClickThrough(bool enabled)
        {
            if (clickThrough == enabled)
            {
                return;
            }

            clickThrough = enabled;
            ApplyWindowStyles();
            GameEventBus.Publish(new WindowClickThroughChangedEvent(clickThrough));
            PublishWindowSettingsChanged();
        }

        public void ToggleClickThrough()
        {
            SetClickThrough(!clickThrough);
        }

        public void SetAlwaysOnTop(bool enabled)
        {
            if (alwaysOnTop == enabled)
            {
                return;
            }

            alwaysOnTop = enabled;
            ApplyWindowStyles();
            GameEventBus.Publish(new WindowAlwaysOnTopChangedEvent(alwaysOnTop));
            PublishWindowSettingsChanged();
        }

        public void SetBorderless(bool enabled)
        {
            if (borderless == enabled)
            {
                return;
            }

            borderless = enabled;
            ApplyWindowStyles();
            PublishWindowSettingsChanged();
        }

        public void SetTransparentBackground(bool enabled)
        {
            if (transparentBackground == enabled)
            {
                return;
            }

            transparentBackground = enabled;
            ApplyWindowSettings();
            PublishWindowSettingsChanged();
        }

        public void SetAllowDrag(bool enabled)
        {
            if (allowDrag == enabled)
            {
                return;
            }

            allowDrag = enabled;
            if (!allowDrag)
            {
                isDragging = false;
            }

            PublishWindowSettingsChanged();
        }

        public void CenterOnScreen()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!TryGetWindowRect(out var rect))
            {
                return;
            }

            var screen = GetVirtualScreenRect();
            var x = screen.x + (screen.width - rect.width) / 2;
            var y = screen.y + (screen.height - rect.height) / 2;
            SetWindowPosition(x, y);
#else
            var width = Mathf.Max(Screen.width, 1);
            var height = Mathf.Max(Screen.height, 1);
            Screen.SetResolution(width, height, false);
#endif
        }

        public void SetWindowPosition(int x, int y)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!TryGetWindowRect(out var rect))
            {
                return;
            }

            var target = new RectInt(x, y, rect.width, rect.height);
            if (keepInsideScreen)
            {
                target = ClampToScreen(target);
            }

            MoveNativeWindow(target.x, target.y);
#endif
        }

        public void MoveWindowBy(Vector2Int delta)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!TryGetWindowRect(out var rect))
            {
                return;
            }

            SetWindowPosition(rect.x + delta.x, rect.y + delta.y);
#endif
        }

        public void BeginDrag()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!allowDrag || clickThrough)
            {
                return;
            }

            if (!TryGetWindowRect(out var rect))
            {
                return;
            }

            isDragging = true;
            dragMouseStart = GetCursorPosition();
            dragWindowStart = rect;
            GameEventBus.Publish(new WindowDragStateChangedEvent(true));
#endif
        }

        public void EndDrag()
        {
            if (!isDragging)
            {
                return;
            }

            if (snapToScreenEdge)
            {
                SnapWindowToScreenEdge();
            }

            isDragging = false;
            GameEventBus.Publish(new WindowDragStateChangedEvent(false));
        }

        public void ApplyWindowSettings()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            windowHandle = ResolveWindowHandle();
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            Application.runInBackground = true;
            ApplyWindowStyles();

            if (transparentBackground)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = transparentColor;
            }
#endif
        }

        private void ApplyWindowStyles()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            windowHandle = ResolveWindowHandle();

            var style = GetWindowLong(windowHandle, GWL_STYLE);
            if (borderless)
            {
                style &= ~WS_CAPTION;
                style &= ~WS_THICKFRAME;
                style &= ~WS_MINIMIZEBOX;
                style &= ~WS_MAXIMIZEBOX;
                style &= ~WS_SYSMENU;
            }

            SetWindowLong(windowHandle, GWL_STYLE, style);

            var exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
            if (transparentBackground)
            {
                exStyle |= WS_EX_LAYERED;
            }
            else
            {
                exStyle &= ~WS_EX_LAYERED;
            }

            if (clickThrough)
            {
                exStyle |= WS_EX_TRANSPARENT;
            }
            else
            {
                exStyle &= ~WS_EX_TRANSPARENT;
            }

            SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle);

            if (transparentBackground)
            {
                var key = ColorToColorRef(transparentColor);
                SetLayeredWindowAttributes(windowHandle, key, 0, LWA_COLORKEY);
            }

            var insertAfter = alwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST;
            SetWindowPos(windowHandle, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);
#endif
        }

        private void UpdateDrag()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (dragMode == DragMode.AnywhereExceptUI && Input.GetMouseButtonDown(dragMouseButton))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                BeginDrag();
            }

            if (Input.GetMouseButtonUp(dragMouseButton))
            {
                EndDrag();
            }

            if (!isDragging)
            {
                return;
            }

            var currentMouse = GetCursorPosition();
            var delta = currentMouse - dragMouseStart;
            var target = new RectInt(
                dragWindowStart.x + delta.x,
                dragWindowStart.y + delta.y,
                dragWindowStart.width,
                dragWindowStart.height);

            if (keepInsideScreen)
            {
                target = ClampToScreen(target);
            }

            MoveNativeWindow(target.x, target.y);
#endif
        }

        private void SnapWindowToScreenEdge()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!TryGetWindowRect(out var rect))
            {
                return;
            }

            var screen = GetVirtualScreenRect();
            var left = screen.x + screenPadding;
            var top = screen.y + screenPadding;
            var right = screen.xMax - rect.width - screenPadding;
            var bottom = screen.yMax - rect.height - screenPadding;
            var targetX = rect.x;
            var targetY = rect.y;

            if (Mathf.Abs(rect.x - left) <= snapDistance)
            {
                targetX = left;
            }
            else if (Mathf.Abs(rect.x - right) <= snapDistance)
            {
                targetX = right;
            }

            if (Mathf.Abs(rect.y - top) <= snapDistance)
            {
                targetY = top;
            }
            else if (Mathf.Abs(rect.y - bottom) <= snapDistance)
            {
                targetY = bottom;
            }

            MoveNativeWindow(targetX, targetY);
#endif
        }

        private RectInt ClampToScreen(RectInt windowRect)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var screen = GetVirtualScreenRect();
            var minX = screen.x + screenPadding;
            var minY = screen.y + screenPadding;
            var maxX = screen.xMax - windowRect.width - screenPadding;
            var maxY = screen.yMax - windowRect.height - screenPadding;

            if (maxX < minX)
            {
                maxX = minX;
            }

            if (maxY < minY)
            {
                maxY = minY;
            }

            return new RectInt(
                Mathf.Clamp(windowRect.x, minX, maxX),
                Mathf.Clamp(windowRect.y, minY, maxY),
                windowRect.width,
                windowRect.height);
#else
            return windowRect;
#endif
        }

        private bool TryGetWindowRect(out RectInt rect)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            windowHandle = ResolveWindowHandle();
            if (windowHandle == IntPtr.Zero || !GetWindowRect(windowHandle, out var nativeRect))
            {
                rect = default;
                return false;
            }

            rect = new RectInt(
                nativeRect.Left,
                nativeRect.Top,
                nativeRect.Right - nativeRect.Left,
                nativeRect.Bottom - nativeRect.Top);
            return true;
#else
            rect = new RectInt(0, 0, Screen.width, Screen.height);
            return true;
#endif
        }

        private void MoveNativeWindow(int x, int y)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            windowHandle = ResolveWindowHandle();
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            SetWindowPos(
                windowHandle,
                alwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST,
                x,
                y,
                0,
                0,
                SWP_NOSIZE);
            GameEventBus.Publish(new WindowMovedEvent(new Vector2Int(x, y)));
#endif
        }

        private void PublishWindowSettingsChanged()
        {
            GameEventBus.Publish(new WindowSettingsChangedEvent(
                alwaysOnTop,
                borderless,
                transparentBackground,
                clickThrough,
                allowDrag));
        }

        private static RectInt GetVirtualScreenRect()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var x = GetSystemMetrics(SM_XVIRTUALSCREEN);
            var y = GetSystemMetrics(SM_YVIRTUALSCREEN);
            var width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            var height = GetSystemMetrics(SM_CYVIRTUALSCREEN);
            return new RectInt(x, y, width, height);
#else
            return new RectInt(0, 0, Screen.width, Screen.height);
#endif
        }

        private static IntPtr ResolveWindowHandle()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var handle = GetActiveWindow();
            if (handle != IntPtr.Zero)
            {
                return handle;
            }

            handle = Process.GetCurrentProcess().MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                return handle;
            }

            return GetForegroundWindow();
#else
            return IntPtr.Zero;
#endif
        }

        private static Vector2Int GetCursorPosition()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            GetCursorPos(out var point);
            return new Vector2Int(point.X, point.Y);
#else
            return Vector2Int.zero;
#endif
        }

        private static uint ColorToColorRef(Color color)
        {
            var r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255);
            var g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255);
            var b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255);
            return (uint)(r | (g << 8) | (b << 16));
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;

        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_SYSMENU = 0x00080000;

        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        private const uint LWA_COLORKEY = 0x00000001;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out WindowRect lpRect);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
#endif
    }
}
