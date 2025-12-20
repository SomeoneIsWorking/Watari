using System.Runtime.InteropServices;

namespace Watari.Bridge.MacOS;

internal static partial class WebViewBridge
{
    [LibraryImport("native/macos/libwebview.dylib", EntryPoint = "WebView_Create")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr Create();

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
