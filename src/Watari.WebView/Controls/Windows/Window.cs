using System.Runtime.InteropServices;
using Watari.Controls.Interfaces;

namespace Watari.Controls.Windows;

public class Window : IWindow
{
    [DllImport("user32.dll")]
    private static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool RegisterClassEx(ref WNDCLASSEX lpwcx);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public IntPtr Handle { get; private set; }

    public Window()
    {
        WNDCLASSEX wndClass = new()
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
            style = 0,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(new WndProc(DefWindowProc)),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = IntPtr.Zero,
            hIcon = IntPtr.Zero,
            hCursor = IntPtr.Zero,
            hbrBackground = IntPtr.Zero,
            lpszMenuName = null!,
            lpszClassName = "WatariWindow",
            hIconSm = IntPtr.Zero
        };
        RegisterClassEx(ref wndClass);
        Handle = CreateWindowEx(0, "WatariWindow", "Watari", 0x00CF0000 | 0x00080000, 100, 100, 800, 600, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        ShowWindow(Handle, 5); // SW_SHOW
    }

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public void SetContent(IWebView webview)
    {
        if (webview is not WebView windowsWebView)
        {
            throw new ArgumentException("webview must be of type Windows.WebView");
        }
        windowsWebView.SetParentHandle(Handle);
    }

    public void Move(int x, int y)
    {
        // Get current size
        GetWindowRect(Handle, out RECT rect);
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        MoveWindow(Handle, x, y, width, height, true);
    }

    public (int x, int y) GetPosition()
    {
        GetWindowRect(Handle, out RECT rect);
        return (rect.Left, rect.Top);
    }
}