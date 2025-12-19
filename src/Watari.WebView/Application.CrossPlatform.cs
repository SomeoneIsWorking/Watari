using System;
using System.Runtime.InteropServices;

namespace Watari.WebView
{
    internal static class ApplicationCrossPlatform
    {
        public static void RunLoop()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ApplicationMacOS.RunLoop();
                return;
            }

            // No-op on other platforms
        }

        public static void StopLoop()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ApplicationMacOS.StopLoop();
                return;
            }
        }
        public static void Init()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ApplicationMacOS.Init();
                return;
            }
        }
    }
}
