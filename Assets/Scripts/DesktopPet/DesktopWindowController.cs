using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DesktopPet
{
    public class DesktopWindowController : MonoBehaviour
    {
        [Header("Window")]
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private bool alwaysOnTop = true;
        [SerializeField] private bool borderless = true;
        [SerializeField] private bool transparentBackground = true;
        [SerializeField] private bool clickThrough = false;
        [SerializeField] private Color transparentColor = Color.black;

        [Header("Drag")]
        [SerializeField] private bool allowDrag = true;
        [SerializeField] private int dragMouseButton = 0;

        [Header("Debug")]
        [SerializeField] private KeyCode toggleClickThroughKey = KeyCode.F8;

        private IntPtr windowHandle;
        private bool isDragging;
        private Vector2Int dragMouseStart;
        private RectInt dragWindowStart;

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

            if (!allowDrag || clickThrough)
            {
                return;
            }

            UpdateDrag();
        }

        public void SetClickThrough(bool enabled)
        {
            clickThrough = enabled;
            ApplyWindowStyles();
        }

        public void ToggleClickThrough()
        {
            SetClickThrough(!clickThrough);
        }

        public void ApplyWindowSettings()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            windowHandle = GetActiveWindow();
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
            if (windowHandle == IntPtr.Zero)
            {
                windowHandle = GetActiveWindow();
            }

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
            if (Input.GetMouseButtonDown(dragMouseButton))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                windowHandle = windowHandle == IntPtr.Zero ? GetActiveWindow() : windowHandle;
                if (windowHandle == IntPtr.Zero || !GetWindowRect(windowHandle, out var rect))
                {
                    return;
                }

                isDragging = true;
                dragMouseStart = GetCursorPosition();
                dragWindowStart = new RectInt(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }

            if (Input.GetMouseButtonUp(dragMouseButton))
            {
                isDragging = false;
            }

            if (!isDragging)
            {
                return;
            }

            var currentMouse = GetCursorPosition();
            var delta = currentMouse - dragMouseStart;
            SetWindowPos(
                windowHandle,
                alwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST,
                dragWindowStart.x + delta.x,
                dragWindowStart.y + delta.y,
                0,
                0,
                SWP_NOSIZE);
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
#endif
    }
}
