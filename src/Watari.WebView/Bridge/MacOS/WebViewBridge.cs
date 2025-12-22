using System.Runtime.InteropServices;

namespace Watari.Bridge.MacOS;

internal static partial class WebViewBridge
{
    public delegate void ConsoleCallbackDelegate([MarshalAs(UnmanagedType.LPUTF8Str)] string level, [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [LibraryImport("native/macos/libwebview.dylib", EntryPoint = "WebView_Create")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr Create(IntPtr callback);

    [LibraryImport("native/macos/libwebview.dylib", EntryPoint = "WebView_Navigate")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Navigate(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);

    [LibraryImport("native/macos/libwebview.dylib", EntryPoint = "WebView_Eval")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Eval(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string js);

    [LibraryImport("native/macos/libwebview.dylib", EntryPoint = "WebView_Destroy")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Destroy(IntPtr handle);

    [LibraryImport("native/macos/libwebview.dylib", EntryPoint = "WebView_AddUserScript")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void AddUserScript(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string scriptSource, int injectionTime, [MarshalAs(UnmanagedType.Bool)] bool forMainFrameOnly);
}
