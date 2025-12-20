namespace Watari.Bridge.MacOS;

internal static partial class WindowBridge
{
    [System.Runtime.InteropServices.LibraryImport("libwindow.dylib", EntryPoint = "Window_CreateWindow")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial IntPtr WindowBridge_CreateWindow();

    [System.Runtime.InteropServices.LibraryImport("libwindow.dylib", EntryPoint = "Window_SetContent")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void WindowBridge_SetContent(IntPtr windowHandle, IntPtr viewHandle);

    [System.Runtime.InteropServices.LibraryImport("libwindow.dylib", EntryPoint = "Window_Destroy")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void WindowBridge_Destroy(IntPtr windowHandle);
}
