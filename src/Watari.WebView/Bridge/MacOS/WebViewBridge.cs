using System.Runtime.InteropServices;

namespace Watari.Bridge.MacOS;

internal static partial class WebViewBridge
{
    [LibraryImport("libwebview.dylib", EntryPoint = "WebView_Create")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr Create();

    [LibraryImport("libwebview.dylib", EntryPoint = "WebView_Navigate")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Navigate(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);

    [LibraryImport("libwebview.dylib", EntryPoint = "WebView_Eval")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Eval(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string js);

    [LibraryImport("libwebview.dylib", EntryPoint = "WebView_Destroy")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Destroy(IntPtr handle);
}
