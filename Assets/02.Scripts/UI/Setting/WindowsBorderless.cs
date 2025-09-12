using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WindowsBorderless
{
#if UNITY_STANDALONE_WIN
    private const int GWL_STYLE = -16;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_SYSMENU = 0x00080000;

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;

    [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static int _savedStyle = 0;

    public static void MakeBorderless(int x, int y, int width, int height)
    {
        IntPtr hWnd = GetActiveWindow();
        if (hWnd == IntPtr.Zero)
        {
            return;
        }

        if (_savedStyle == 0)
        {
            _savedStyle = GetWindowLong(hWnd, GWL_STYLE);
        }

        int style = GetWindowLong(hWnd, GWL_STYLE);
        style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
        SetWindowLong(hWnd, GWL_STYLE, style);

        SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, SWP_FRAMECHANGED | SWP_NOZORDER);
    }

    public static void RestoreStandardWindow()
    {
        IntPtr hWnd = GetActiveWindow();
        if (hWnd == IntPtr.Zero || _savedStyle == 0)
        {
            return;
        }
        SetWindowLong(hWnd, GWL_STYLE, _savedStyle);
        SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOZORDER | SWP_NOSIZE | SWP_NOMOVE);
    }
#else
    public static void MakeBorderless(int x, int y, int width, int height)
    {
        Debug.LogWarning("WindowsBorderless.MakeBorderless is only supported on Windows standalone builds.");
    }
    public static void RestoreStandardWindow() {}
#endif
}


