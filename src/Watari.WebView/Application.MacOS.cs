using System.Runtime.InteropServices;

namespace Watari.WebView
{
    internal static class ApplicationMacOS
    {
        [System.Runtime.InteropServices.DllImport("libwkapp.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_app_init")]
        private static extern void wk_app_init();

        [System.Runtime.InteropServices.DllImport("libwkapp.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_run_loop")]
        private static extern void wk_run_loop();

        [System.Runtime.InteropServices.DllImport("libwkapp.dylib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "wk_stop_loop")]
        private static extern void wk_stop_loop();

        public static void Init()
        {
            try { wk_app_init(); } catch { }
        }

        public static void RunLoop()
        {
            try { wk_run_loop(); } catch { }
        }

        public static void StopLoop()
        {
            try { wk_stop_loop(); } catch { }
        }
    }
}
