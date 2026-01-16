using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WindowUtils
{
    // --- Windows API 引入 ---
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    // --- 结构体定义 ---
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    // --- 常量定义 ---
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_EX_TOPMOST = 0x00000008; // 置顶
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020; // 鼠标穿透核心标志

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    // --- 公开方法 ---

    /// <summary>
    /// 初始化桌宠窗口（去边框 + 透明背景）
    /// </summary>
    public static void SetupWindow()
    {
#if !UNITY_EDITOR
        IntPtr hWnd = GetActiveWindow();

        // 1. 去除窗口边框
        // 这里的逻辑是保留 Popup 属性但移除原来的边框样式
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // 2. 扩展透明区域 (实现真·Alpha透明)
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // 3. 初始设置为置顶
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0x0002 | 0x0001); // SWP_NOMOVE | SWP_NOSIZE
#endif
    }

    /// <summary>
    /// 设置鼠标是否可以穿透窗口
    /// </summary>
    /// <param name="isClickable">true=可以点击游戏; false=鼠标穿透点到桌面</param>
    public static void SetClickable(bool isClickable)
    {
#if !UNITY_EDITOR
        IntPtr hWnd = GetActiveWindow();
        uint exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

        if (isClickable)
        {
            // 移除穿透属性，允许点击
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);
        }
        else
        {
            // 添加穿透属性，鼠标会直接点到壁纸
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
        }
#endif
    }
}