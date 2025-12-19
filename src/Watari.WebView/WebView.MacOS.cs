using System;
using System.Runtime.InteropServices;

namespace Watari.WebView
{
    internal static class WebViewMacOS
    {
        private delegate void NativeMsgCb(IntPtr utf8);

        private static NativeMsgCb? _nativeCb;
        private static IntPtr _handle = IntPtr.Zero;

        [System.Runtime.InteropServices.DllImport("libwkwebview.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_create_window")]
        private static extern IntPtr wk_create_window([MarshalAs(UnmanagedType.LPStr)] string url, NativeMsgCb cb);

        [System.Runtime.InteropServices.DllImport("libwkwebview.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_navigate")]
        private static extern void wk_navigate(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string url);

        [System.Runtime.InteropServices.DllImport("libwkwebview.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_eval")]
        private static extern void wk_eval(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string js);

        [System.Runtime.InteropServices.DllImport("libwkwebview.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_destroy")]
        private static extern void wk_destroy(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("libwkwebview.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_free_string")]
        private static extern void wk_free_string(IntPtr str);

        [System.Runtime.InteropServices.DllImport("libwkwebview.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_set_terminate_on_window_close")]
        private static extern void wk_set_terminate_on_window_close(int val);

        internal static event Action<string>? OnMessage;

        private static void NativeMessageCallback(IntPtr str)
        {
            try
            {
                string? s = Marshal.PtrToStringUTF8(str);
                if (s != null) OnMessage?.Invoke(s);
            }
            finally
            {
                try { wk_free_string(str); } catch { }
            }
        }

        public static bool CreateWindow(string url)
        {
            try
            {
                if (_handle == IntPtr.Zero)
                {
                    _nativeCb = NativeMessageCallback;
                    _handle = wk_create_window(url ?? "about:blank", _nativeCb);
                    return _handle != IntPtr.Zero;
                }
                wk_navigate(_handle, url ?? "about:blank");
                return true;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static void SetTerminateOnWindowClose(bool terminate)
        {
            try
            {
                wk_set_terminate_on_window_close(terminate ? 1 : 0);
            }
            catch { }
        }

        public static bool Navigate(string url)
        {
            try
            {
                if (_handle == IntPtr.Zero) return CreateWindow(url);
                wk_navigate(_handle, url ?? "about:blank");
                return true;
            }
            catch { return false; }
        }

        public static bool Eval(string js)
        {
            try
            {
                if (_handle == IntPtr.Zero) return false;
                wk_eval(_handle, js ?? string.Empty);
                return true;
            }
            catch { return false; }
        }

        public static void Destroy()
        {
            if (_handle != IntPtr.Zero)
            {
                try { wk_destroy(_handle); } catch { }
                _handle = IntPtr.Zero;
            }
        }
    }
}
