# Watari.WebView — Developer Guide

This document explains the current structure, how the macOS native bridges work, how to build/run the project, where to add features (request interception, JS API), and debugging tips. It is intentionally small and self-contained so you can continue development from a fresh workspace.

## High level
- The project runs a small .NET executable that drives a native WKWebView on macOS via two separate native dylibs:
  - `native/macos/libwkapp.dylib` — `app_bridge` (NSApplication init, menu, run/stop loop)
  - `native/macos/libwkwebview.dylib` — `webview_bridge` (WKWebView window lifecycle, navigate, eval, messages)
- C# code is a thin facade that calls into these dylibs with `DllImport`:
  - `Application.CrossPlatform.cs` / `Application.MacOS.cs` — initialize/run/stop the Cocoa app
  - `WebView.CrossPlatform.cs` / `WebView.MacOS.cs` — create/navigate/eval/destroy webviews and receive messages
- `Program.cs` initializes the app, creates one webview window, and runs the Cocoa run loop.

## Important files
- Native: `native/macos/app_bridge.m`, `native/macos/webview_bridge.m`, `native/macos/build-dylib.sh`
- Managed: `src/Watari.WebView/Program.cs`, `Application.CrossPlatform.cs`, `Application.MacOS.cs`, `WebView.CrossPlatform.cs`, `WebView.MacOS.cs`, and `Controls/PlatformWebView.cs` (non-UI facade)
- Project: `src/Watari.WebView/Watari.WebView.csproj`

## Build & run (macOS, only command-line tools required)
1. Build native dylibs:
```bash
cd native/macos
chmod +x build-dylib.sh
./build-dylib.sh
```
This produces `libwkapp.dylib` and `libwkwebview.dylib` under `native/macos` and the build copies them into `src/Watari.WebView/bin/Debug/net8.0` during `dotnet build`.

2. Build and run the .NET app (optionally pass a URL):
```bash
dotnet build src/Watari.WebView/Watari.WebView.csproj
DYLD_LIBRARY_PATH=src/Watari.WebView/bin/Debug/net8.0 dotnet run --project src/Watari.WebView -- https://www.example.com
```

## Debugging notes
- Native logs: `NSLog` statements were added in the bridges. Run from a terminal and watch stdout/stderr for tags like `[wk_app_init]`, `[wk_create_window]`, `[wk_run_loop]`, `[wk_navigate]`, `[wk_eval]`.
- If the app seems stuck: check logs to see whether `wk_app_init` completed and whether `wk_run_loop` entered. Avoid calling `dispatch_sync` to main thread from main thread (the bridge protects against that now).
- If the window is invisible: ensure `window` is created with reasonable frame, call `[window center]`, `[window makeKeyAndOrderFront:nil]`, and `[NSApp activateIgnoringOtherApps:YES]`. The webview bridge already does these; try adding a small `dispatch_after` re-activate step if needed.

## Extension points
- Request interception: implement `WKURLSchemeHandler` in `webview_bridge.m` to handle custom schemes and forward requests/responses to C#.
- JS ↔ Native: page JS can call `window.webkit.messageHandlers.bridge.postMessage(...)` to send messages; `evaluateJavaScript` returns results via the completion handler and is forwarded back to C# as messages.
- Windows support: add `WebView2` host and corresponding `libwkwebview`-like wrapper for Windows or use the WebView2 .NET SDK.

## Removing shims and cleanup
- The project contains a few compatibility shims; remove `PlatformWebViewBridge.cs` / `PlatformApplication.cs` if you migrate callers to the new facades.

## Quick checklist for next dev
- Decide whether you want: (A) in-process native embedding (current approach), or (B) helper process (easier to intercept requests). Add `WKURLSchemeHandler` if keeping in-process.
- Add logging for navigation events (`WKNavigationDelegate`) in `webview_bridge.m` to track loads.

End of document.
