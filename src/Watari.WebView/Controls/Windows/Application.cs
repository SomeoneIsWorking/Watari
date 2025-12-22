using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Watari.Controls.Interfaces;

namespace Watari.Controls.Windows;

internal class Application : IApplication
{
    private const uint WM_USER = 0x0400;
    private const uint WM_RUN_ON_MAIN_THREAD = WM_USER + 1;

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll")]
    private static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private readonly uint _threadId;

    public IntPtr Handle { get; } = IntPtr.Zero;

    public Application()
    {
        _threadId = GetCurrentThreadId();
    }

    public void SetMainWindow(IWindow window)
    {
        // No op
    }

    public void RunLoop()
    {
        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            if (msg.message == WM_RUN_ON_MAIN_THREAD)
            {
                if (_mainThreadActions.TryDequeue(out var action))
                {
                    action();
                }
            }
            else
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
    }

    public void StopLoop()
    {
        PostQuitMessage(0);
    }

    public void RunOnMainThread(Action action)
    {
        _mainThreadActions.Enqueue(action);
        PostThreadMessage(_threadId, WM_RUN_ON_MAIN_THREAD, IntPtr.Zero, IntPtr.Zero);
    }

    public void AddMenuItem(string title)
    {
        // No op for Windows
    }
}