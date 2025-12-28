using System.Runtime.InteropServices;

namespace Watari.Bridge.Linux;

internal static partial class WebViewBridge
{
    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_web_view_new")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr Create();

    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_web_view_load_uri")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Navigate(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);

    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_web_view_run_javascript")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Eval(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string js, IntPtr cancellable, IntPtr callback, IntPtr user_data);

    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "gtk_widget_destroy")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void Destroy(IntPtr handle);

    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_web_view_get_user_content_manager")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr webkit_web_view_get_user_content_manager(IntPtr web_view);

    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_user_script_new")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr webkit_user_script_new([MarshalAs(UnmanagedType.LPUTF8Str)] string source, int injection_time, int injected_frames, IntPtr allowlist, IntPtr blocklist);

    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_user_content_manager_add_script")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void webkit_user_content_manager_add_script(IntPtr manager, IntPtr script);

    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_web_view_get_configuration")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr webkit_web_view_get_configuration(IntPtr web_view);
    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_web_view_configuration_get_preferences")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr webkit_web_view_configuration_get_preferences(IntPtr configuration);
    
    [LibraryImport("libwebkit2gtk-4.1.so.0", EntryPoint = "webkit_preferences_set_developer_extras_enabled")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void webkit_preferences_set_developer_extras_enabled(IntPtr preferences, [MarshalAs(UnmanagedType.Bool)] bool enable);

}