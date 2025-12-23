import Cocoa
import Darwin
import UniformTypeIdentifiers

func sigintHandler(_ signal: Int32) {
    print("[ApplicationBridge] SIGINT received, terminating application")
    NSApplication.shared.terminate(nil)
}

class ApplicationDelegate: NSObject, NSApplicationDelegate {
    var mainWindow: NSWindow?

    func applicationShouldHandleReopen(_ sender: NSApplication, hasVisibleWindows flag: Bool) -> Bool {
        print("[ApplicationDelegate] applicationShouldHandleReopen called")
        mainWindow?.makeKeyAndOrderFront(nil)
        return true
    }

    @objc func dummyAction(_ sender: Any?) {
        // Dummy action for menu items
    }
}

@_cdecl("Application_Init")
public func Application_Init() -> CFTypeRef? {
    print("[ApplicationBridge] initializing NSApp")
    let app = NSApplication.shared
    print("[ApplicationBridge] NSApplication created")
    app.setActivationPolicy(.regular)

    let menubar = NSMenu()
    let appMenuItem = NSMenuItem()
    menubar.addItem(appMenuItem)

    let appMenu = NSMenu(title: "")
    let appName = ProcessInfo.processInfo.processName
    let quitItem = NSMenuItem(title: "Quit \(appName)",
                              action: #selector(NSApplication.terminate(_:)),
                              keyEquivalent: "q")
    appMenu.addItem(quitItem)
    appMenuItem.submenu = appMenu
    app.mainMenu = menubar

    let delegate = ApplicationDelegate()
    app.delegate = delegate
    objc_setAssociatedObject(app, "ApplicationDelegate", delegate, .OBJC_ASSOCIATION_RETAIN_NONATOMIC)

    print("[ApplicationBridge] main menu created")
    return CFBridgingRetain(app)
}

@_cdecl("Application_RunLoop")
public func Application_RunLoop(_ app: UnsafeMutableRawPointer?) {
    guard let app = app else { return }
    let application = Unmanaged<NSApplication>.fromOpaque(app).takeUnretainedValue()
    signal(SIGINT, sigintHandler)
    print("[ApplicationBridge runLoop] enter")
    application.run()
    print("[ApplicationBridge runLoop] exit")
}

@_cdecl("Application_StopLoop")
public func Application_StopLoop(_ app: UnsafeMutableRawPointer?) {
    guard let app = app else { return }
    let application = Unmanaged<NSApplication>.fromOpaque(app).takeUnretainedValue()
    print("[ApplicationBridge stopLoop] stopping")
    application.stop(nil)
}

@_cdecl("Application_SetMainWindow")
public func Application_SetMainWindow(_ app: UnsafeMutableRawPointer?, _ window: UnsafeMutableRawPointer?) {
    guard let app = app, let window = window else { return }
    print("[ApplicationBridge setKeyWindow] setting key window")
    let application = Unmanaged<NSApplication>.fromOpaque(app).takeUnretainedValue()
    let nsWindow = Unmanaged<NSWindow>.fromOpaque(window).takeUnretainedValue()
    nsWindow.makeKeyAndOrderFront(nil)
    if let delegate = application.delegate as? ApplicationDelegate {
        delegate.mainWindow = nsWindow
    }
    for win in application.windows {
        win.isReleasedWhenClosed = win.isKeyWindow
    }
    print("[ApplicationBridge setKeyWindow] set key window \(nsWindow)")
}

@_cdecl("Application_RunOnMainThread")
public func Application_RunOnMainThread(_ app: UnsafeMutableRawPointer?, _ callback: (@convention(c) () -> Void)?) {
    guard let callback = callback else { return }
    DispatchQueue.main.async {
        callback()
    }
}

@_cdecl("Application_AddMenuItem")
public func Application_AddMenuItem(_ app: UnsafeMutableRawPointer?, _ title: UnsafePointer<CChar>?) {
    guard let app = app, let title = title else { return }
    let application = Unmanaged<NSApplication>.fromOpaque(app).takeUnretainedValue()
    guard let menubar = application.mainMenu,
          let appMenuItem = menubar.item(at: 0),
          let appMenu = appMenuItem.submenu else { return }
    let titleString = String(cString: title)
    let newItem = NSMenuItem(title: titleString, action: #selector(ApplicationDelegate.dummyAction(_:)), keyEquivalent: "")
    appMenu.addItem(newItem)
}

@_cdecl("Application_OpenFileDialog")
public func Application_OpenFileDialog(_ app: UnsafeMutableRawPointer?, _ allowedExtensions: UnsafePointer<CChar>?) -> UnsafePointer<CChar>? {
    guard let allowedExtensions = allowedExtensions else { return nil }
    
    let extensionsString = String(cString: allowedExtensions)
    let extensions = extensionsString.split(separator: ",").map { String($0).trimmingCharacters(in: .whitespaces) }
    
    let panel = NSOpenPanel()
    panel.canChooseFiles = true
    panel.canChooseDirectories = false
    panel.allowsMultipleSelection = false
    if #available(macOS 12.0, *) {
        panel.allowedContentTypes = extensions.compactMap { UTType(filenameExtension: $0) }
    } else {
        panel.allowedFileTypes = extensions
    }
    
    let response = panel.runModal()
    if response == .OK, let url = panel.url {
        let path = url.path
        return UnsafePointer(strdup(path))
    }
    return nil
}