using System;
using System.Runtime.InteropServices;

namespace Watari.WebView
{
    internal static class WebViewCrossPlatform
    {
        internal static event Action<string>? OnMessage;

        public static bool CreateWindow(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                WebViewMacOS.OnMessage += (m) => OnMessage?.Invoke(m);
                return WebViewMacOS.CreateWindow(url);
            }

            return false;
        }

        public static bool Navigate(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return WebViewMacOS.Navigate(url);
            return false;
        }

        public static bool Eval(string js)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return WebViewMacOS.Eval(js);
            return false;
        }

        public static void Destroy()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                WebViewMacOS.Destroy();
        }
        
        public static void SetTerminateOnWindowClose(bool terminate)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                WebViewMacOS.SetTerminateOnWindowClose(terminate);
        }
    }
}
