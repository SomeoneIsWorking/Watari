namespace Watari.Bridge.MacOS;

internal static partial class ApplicationBridge
{
    public delegate void MainThreadCallback();

    [System.Runtime.InteropServices.LibraryImport("native/macos/libapplication.dylib", EntryPoint = "Application_Init")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial IntPtr Init();


    [System.Runtime.InteropServices.LibraryImport("native/macos/libapplication.dylib", EntryPoint = "Application_RunLoop")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void RunLoop(IntPtr handle);

    [System.Runtime.InteropServices.LibraryImport("native/macos/libapplication.dylib", EntryPoint = "Application_StopLoop")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void StopLoop(IntPtr handle);

    [System.Runtime.InteropServices.LibraryImport("native/macos/libapplication.dylib", EntryPoint = "Application_SetMainWindow")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void SetMainWindow(IntPtr appHandle, IntPtr windowHandle);

    [System.Runtime.InteropServices.LibraryImport("native/macos/libapplication.dylib", EntryPoint = "Application_RunOnMainThread")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void RunOnMainThread(IntPtr handle, MainThreadCallback callback);

    [System.Runtime.InteropServices.LibraryImport("native/macos/libapplication.dylib", EntryPoint = "Application_AddMenuItem")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void AddMenuItem(IntPtr handle, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPUTF8Str)] string title);
}
