namespace Watari.Bridge.Linux;

internal static partial class WindowBridge
{
    [System.Runtime.InteropServices.LibraryImport("libgtk-3.so.0", EntryPoint = "gtk_window_new")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial IntPtr gtk_window_new(int type);

    [System.Runtime.InteropServices.LibraryImport("libgtk-3.so.0", EntryPoint = "gtk_widget_show")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void gtk_widget_show(IntPtr widget);

    [System.Runtime.InteropServices.LibraryImport("libgtk-3.so.0", EntryPoint = "gtk_container_add")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void gtk_container_add(IntPtr container, IntPtr widget);
}