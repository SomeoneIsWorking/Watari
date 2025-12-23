import Cocoa
import WebKit

class WebViewConsoleLogger: NSObject, WKScriptMessageHandler {
    var consoleCallback: (@convention(c) (UnsafePointer<CChar>?, UnsafePointer<CChar>?) -> Void)?

    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        if message.name == "consoleLog" {
            if let dict = message.body as? [String: Any],
               let level = dict["level"] as? String,
               let msg = dict["message"] as? String {
                consoleCallback?(level, msg)
            }
        }
    }
}

class WebViewDropHandler: NSObject, WKScriptMessageHandler {
    weak var webView: WKWebView?
    var callbackId: String?
    var id: String?
    var allowedExtensions: String?

    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        if message.name == "setDropZone" {
            if let dict = message.body as? [String: Any] {
                callbackId = dict["callbackId"] as? String
                id = dict["element"] as? String
                allowedExtensions = dict["allowedExtensions"] as? String
            }
        } else if message.name == "removeDropZone" {
            callbackId = nil
            id = nil
            allowedExtensions = nil
        }
    }
}

class MyWKWebView: WKWebView {
    var dropHandler: WebViewDropHandler?

    override func draggingEntered(_ sender: NSDraggingInfo) -> NSDragOperation {
        guard let handler = dropHandler, handler.id != nil else { return [] }
        let pasteboard = sender.draggingPasteboard
        let fileURLs = pasteboard.readObjects(forClasses: [NSURL.self], options: [.urlReadingFileURLsOnly: true]) as? [URL] ?? []
        guard !fileURLs.isEmpty else { return [] }
        var paths = [String]()
        for url in fileURLs {
            if url.isFileURL {
                paths.append(url.path)
            }
        }
        guard !paths.isEmpty else { return [] }
        guard let jsonData = try? JSONSerialization.data(withJSONObject: paths, options: []),
              let jsonString = String(data: jsonData, encoding: .utf8) else { return [] }
        let location = sender.draggingLocation
        let pointInView = convert(location, from: nil)
        let js = "watari._checkAndValidateDropZone('\(handler.id!)', \(pointInView.x), \(pointInView.y), '\(handler.allowedExtensions ?? "")', '\(jsonString)')"
        evaluateJavaScript(js, completionHandler: nil)
        return .copy
    }

    override func draggingUpdated(_ sender: NSDraggingInfo) -> NSDragOperation {
        draggingEntered(sender)
    }

    override func draggingExited(_ sender: NSDraggingInfo?) {
        guard let _ = sender, let handler = dropHandler, handler.id != nil else { return }
        let clearJs = "watari._clearDropZoneClass('\(handler.id!)')"
        evaluateJavaScript(clearJs, completionHandler: nil)
    }

    override func prepareForDragOperation(_ sender: NSDraggingInfo) -> Bool {
        true
    }

    override func performDragOperation(_ sender: NSDraggingInfo) -> Bool {
        guard let handler = dropHandler, let callbackId = handler.callbackId else { return false }
        let clearJs = "watari._clearDropZoneClass('\(handler.id!)')"
        evaluateJavaScript(clearJs, completionHandler: nil)
        let pasteboard = sender.draggingPasteboard
        let fileURLs = pasteboard.readObjects(forClasses: [NSURL.self], options: [.urlReadingFileURLsOnly: true]) as? [URL] ?? []
        var paths = [String]()
        for url in fileURLs {
            if url.isFileURL {
                paths.append(url.path)
            }
        }
        guard !paths.isEmpty,
              let jsonData = try? JSONSerialization.data(withJSONObject: paths, options: []),
              let jsonString = String(data: jsonData, encoding: .utf8) else { return false }
        let location = sender.draggingLocation
        let pointInView = convert(location, from: nil)
        let js = "watari._handleDrop('\(handler.id!)', \(pointInView.x), \(pointInView.y), '\(jsonString)', '\(callbackId)')"
        evaluateJavaScript(js, completionHandler: nil)
        return true
    }
}

@_cdecl("WebView_Create")
public func WebView_Create(_ callback: (@convention(c) (UnsafePointer<CChar>?, UnsafePointer<CChar>?) -> Void)?) -> CFTypeRef? {
    print("[WebView_Create] creating WKWebView")
    let config = WKWebViewConfiguration()

    // Set up console.log redirection
    let userContentController = WKUserContentController()
    let logger = WebViewConsoleLogger()
    logger.consoleCallback = callback
    userContentController.add(logger, name: "consoleLog")

    // Set up drop zone handler
    let dropHandler = WebViewDropHandler()
    userContentController.add(dropHandler, name: "setDropZone")
    userContentController.add(dropHandler, name: "removeDropZone")

    // Inject script to override console methods
    let consoleOverrideScript = """
    var levels = ['log', 'error', 'warn', 'info', 'debug'];
    levels.forEach(function(level) {
        console[level] = function(...args) {
            window.webkit.messageHandlers.consoleLog.postMessage({level: level, message: args.join(' ')});
        };
    });
    """
    let userScript = WKUserScript(source: consoleOverrideScript, injectionTime: .atDocumentStart, forMainFrameOnly: true)
    userContentController.addUserScript(userScript)

    config.userContentController = userContentController

    let created = MyWKWebView(frame: NSRect(x: 0, y: 0, width: 1024, height: 768), configuration: config)
    print("[WebView_Create] created WKWebView \(created)")

    // Set up drop handler and register for drag types
    dropHandler.webView = created
    created.dropHandler = dropHandler
    created.registerForDraggedTypes([.fileURL])

    return CFBridgingRetain(created)
}

@_cdecl("WebView_Navigate")
public func WebView_Navigate(_ viewHandle: UnsafeMutableRawPointer?, _ url: UnsafePointer<CChar>?) {
    guard let viewHandle = viewHandle, let url = url else { return }
    let webView = Unmanaged<WKWebView>.fromOpaque(viewHandle).takeUnretainedValue()
    let nsurl = String(cString: url)
    guard let u = URL(string: nsurl) else { return }
    print("[WebView_Navigate] created URL: \(u)")
    let req = URLRequest(url: u)
    print("[WebView_Navigate] navigating WKWebView \(webView) to URL: \(nsurl)")
    webView.load(req)
}

@_cdecl("WebView_Eval")
public func WebView_Eval(_ viewHandle: UnsafeMutableRawPointer?, _ js: UnsafePointer<CChar>?) {
    guard let viewHandle = viewHandle, let js = js else { return }
    let webView = Unmanaged<WKWebView>.fromOpaque(viewHandle).takeUnretainedValue()
    let jsString = String(cString: js)
    print("[WebView_Eval] evaluating JavaScript on WKWebView \(webView): \(jsString)")
    webView.evaluateJavaScript(jsString, completionHandler: nil)
}

@_cdecl("WebView_Destroy")
public func WebView_Destroy(_ viewHandle: UnsafeMutableRawPointer?) {
    guard let viewHandle = viewHandle else { return }
    let webView = Unmanaged<WKWebView>.fromOpaque(viewHandle).takeUnretainedValue()
    webView.removeFromSuperview()
}

@_cdecl("WebView_AddUserScript")
public func WebView_AddUserScript(_ viewHandle: UnsafeMutableRawPointer?, _ scriptSource: UnsafePointer<CChar>?, _ injectionTime: Int32, _ forMainFrameOnly: Bool) {
    guard let viewHandle = viewHandle, let scriptSource = scriptSource else { return }
    let webView = Unmanaged<WKWebView>.fromOpaque(viewHandle).takeUnretainedValue()
    let source = String(cString: scriptSource)
    let time = WKUserScriptInjectionTime(rawValue: Int(injectionTime)) ?? .atDocumentStart
    let userScript = WKUserScript(source: source, injectionTime: time, forMainFrameOnly: forMainFrameOnly)
    webView.configuration.userContentController.addUserScript(userScript)
}