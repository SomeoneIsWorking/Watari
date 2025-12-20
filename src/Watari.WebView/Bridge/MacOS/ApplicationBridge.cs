namespace Watari.Bridge.MacOS;

internal static partial class AppliationBridge
{
    [System.Runtime.InteropServices.LibraryImport("libapplication.dylib", EntryPoint = "Application_Init")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial IntPtr Init();


    [System.Runtime.InteropServices.LibraryImport("libapplication.dylib", EntryPoint = "Application_RunLoop")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void RunLoop(IntPtr handle);

    [System.Runtime.InteropServices.LibraryImport("libapplication.dylib", EntryPoint = "Application_StopLoop")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void StopLoop(IntPtr handle);

    [System.Runtime.InteropServices.LibraryImport("libapplication.dylib", EntryPoint = "Application_SetMainWindow")]
    [System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void SetMainWindow(IntPtr appHandle, IntPtr windowHandle);
}
