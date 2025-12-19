using System;

namespace Watari.WebView;

class Program
{
    public static int Main(string[] args)
    {
        string url = args.Length > 0 ? args[0] : "https://www.example.com";

        // Initialize application (menus, Dock, activation)
        ApplicationCrossPlatform.Init();

        if (!WebViewCrossPlatform.CreateWindow(url))
        {
            Console.Error.WriteLine("Native WK bridge not available. Build native/macos/libwkapp.dylib and native/macos/libwkwebview.dylib and retry.");
            return 1;
        }

        WebViewCrossPlatform.OnMessage += (m) => Console.WriteLine("[webview] " + m);

        ApplicationCrossPlatform.RunLoop();

        WebViewCrossPlatform.Destroy();
        return 0;
    }
}