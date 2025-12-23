// Window-only bridge. Creates an NSWindow and allows setting a content view.

import Cocoa

class WindowDelegate: NSObject, NSWindowDelegate {
    func windowShouldClose(_ sender: NSWindow) -> Bool {
        print("[WindowDelegate] windowShouldClose called")
        if sender.isMainWindow {
            sender.orderOut(nil)
        }
        return !sender.isMainWindow
    }
}

@_cdecl("Window_CreateWindow")
public func Window_CreateWindow() -> CFTypeRef? {
    print("[Window_CreateWindow] creating NSWindow")
    let frame = NSRect(x: 100, y: 100, width: 1024, height: 768)
    print("[WindowBridge_CreateWindow] frame: \(frame)")
    let window = NSWindow(contentRect: frame,
                          styleMask: [.titled, .resizable, .closable],
                          backing: .buffered,
                          defer: false)
    print("[WindowBridge_CreateWindow] window initialized")
    let delegate = WindowDelegate()
    window.delegate = delegate
    objc_setAssociatedObject(window, "WindowDelegate", delegate, .OBJC_ASSOCIATION_RETAIN_NONATOMIC)
    print("[WindowBridge_CreateWindow] created window \(window)")
    return CFBridgingRetain(window)
}

@_cdecl("Window_SetContent")
public func Window_SetContent(_ windowHandle: UnsafeMutableRawPointer?, _ viewHandle: UnsafeMutableRawPointer?) {
    guard let windowHandle = windowHandle, let viewHandle = viewHandle else { return }
    print("[Window_SetContent] setting content view")
    let window = Unmanaged<NSWindow>.fromOpaque(windowHandle).takeUnretainedValue()
    print("[Window_SetContent] got window \(window)")
    let view = Unmanaged<NSView>.fromOpaque(viewHandle).takeUnretainedValue()
    print("[Window_SetContent] got view \(view)")
    let contentRect = window.contentView?.frame ?? .zero
    print("[Window_SetContent] content rect: \(contentRect)")
    view.frame = contentRect
    print("[Window_SetContent] set view frame to content rect")
    window.contentView = view
    print("[Window_SetContent] set view \(view) into window \(window)")
}

@_cdecl("Window_Move")
public func Window_Move(_ windowHandle: UnsafeMutableRawPointer?, _ x: Int32, _ y: Int32) {
    guard let windowHandle = windowHandle else { return }
    let window = Unmanaged<NSWindow>.fromOpaque(windowHandle).takeUnretainedValue()
    let origin = NSPoint(x: CGFloat(x), y: CGFloat(y))
    window.setFrameOrigin(origin)
    print("[Window_Move] moved window \(window) to (\(x), \(y))")
}

@_cdecl("Window_GetPosition")
public func Window_GetPosition(_ windowHandle: UnsafeMutableRawPointer?, _ x: UnsafeMutablePointer<Int32>?, _ y: UnsafeMutablePointer<Int32>?) {
    guard let windowHandle = windowHandle, let x = x, let y = y else { return }
    let window = Unmanaged<NSWindow>.fromOpaque(windowHandle).takeUnretainedValue()
    let frame = window.frame
    x.pointee = Int32(frame.origin.x)
    y.pointee = Int32(frame.origin.y)
    print("[Window_GetPosition] window \(window) at (\(x.pointee), \(y.pointee))")
}

@_cdecl("Window_Destroy")
public func Window_Destroy(_ windowHandle: UnsafeMutableRawPointer?) {
    guard let windowHandle = windowHandle else { return }
    let window = Unmanaged<NSWindow>.fromOpaque(windowHandle).takeUnretainedValue()
    window.orderOut(nil)
    print("[WindowBridge_Destroy] destroyed window \(window)")
}