namespace Watari.Bridge.Linux;

internal static partial class ApplicationBridge
{
    public delegate void MainThreadCallback();

    [System.Runtime.InteropServices.LibraryImport("libgtk-3.so.0", EntryPoint = "gtk_init")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void gtk_init(IntPtr argc, IntPtr argv);

    [System.Runtime.InteropServices.LibraryImport("libgtk-3.so.0", EntryPoint = "gtk_main")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void gtk_main();

    [System.Runtime.InteropServices.LibraryImport("libgtk-3.so.0", EntryPoint = "gtk_main_quit")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void gtk_main_quit();

    public delegate int IdleCallback(IntPtr data);

    [System.Runtime.InteropServices.LibraryImport("libgtk-3.so.0", EntryPoint = "g_idle_add")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial uint g_idle_add(IdleCallback callback, IntPtr data);
}