import Cocoa
import WebKit
import UniformTypeIdentifiers

func commonJSCompletionHandler(webView: WKWebView, callbackId: String? = nil) -> (Any?, Error?) -> Void {
    return { result, error in
        if let error = error {
            print("[WebView] JavaScript evaluation error: \(error.localizedDescription)")
            let nsError = error as NSError 
            print("[WebView] Error domain: \(nsError.domain), code: \(nsError.code)")
            print("[WebView] Error userInfo: \(nsError.userInfo)")
            if callbackId != nil {
                let escapedError = error.localizedDescription.replacingOccurrences(of: "'", with: "\\'")
                let errorJs = "watari.callbackError('\(callbackId!)', '\(escapedError)')"
                webView.evaluateJavaScript(errorJs, completionHandler: nil)
            }
        }
    }
}

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

class WebViewFileDialogHandler: NSObject, WKScriptMessageHandler {
    weak var webView: WKWebView?

    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        if message.name == "openFileDialog" {
            if let dict = message.body as? [String: Any],
               let callbackId = dict["callbackId"] as? String,
               let allowedExtensions = dict["allowedExtensions"] as? String {
                openFileDialog(callbackId: callbackId, allowedExtensions: allowedExtensions)
            }
        }
    }

    func openFileDialog(callbackId: String, allowedExtensions: String) {
        let extensions = allowedExtensions.split(separator: ",").map { String($0).trimmingCharacters(in: .whitespaces) }
        
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
        var result: String? = nil
        if response == .OK, let url = panel.url {
            result = url.path
        }
        print("[WebViewFileDialogHandler] selected file: \(result ?? "none")")
        let js: String
        if let result = result {
            let escapedResult = result.replacingOccurrences(of: "'", with: "\\'")
            js = "watari.callbacks['\(callbackId)']('\(escapedResult)')"
        } else {
            js = "watari.callbacks['\(callbackId)'](null)"
        }
        webView?.evaluateJavaScript(js, completionHandler: commonJSCompletionHandler(webView: webView!, callbackId: callbackId))
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
        evaluateJavaScript(js, completionHandler: commonJSCompletionHandler(webView: self))
        return .copy
    }

    override func draggingUpdated(_ sender: NSDraggingInfo) -> NSDragOperation {
        draggingEntered(sender)
    }

    override func draggingExited(_ sender: NSDraggingInfo?) {
        guard let handler = dropHandler, handler.id != nil else { return }
        let clearJs = "watari._clearDropZoneClass('\(handler.id!)')"
        evaluateJavaScript(clearJs, completionHandler: commonJSCompletionHandler(webView: self))
    }

    override func prepareForDragOperation(_ sender: NSDraggingInfo) -> Bool {
        true
    }

    override func performDragOperation(_ sender: NSDraggingInfo) -> Bool {
        guard let handler = dropHandler, let callbackId = handler.callbackId else { return false }
        let clearJs = "watari._clearDropZoneClass('\(handler.id!)')"
        evaluateJavaScript(clearJs, completionHandler: commonJSCompletionHandler(webView: self))
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
        evaluateJavaScript(js, completionHandler: commonJSCompletionHandler(webView: self, callbackId: callbackId))
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

    // Set up file dialog handler
    let fileDialogHandler = WebViewFileDialogHandler()
    userContentController.add(fileDialogHandler, name: "openFileDialog")
    config.userContentController = userContentController

    let created = MyWKWebView(frame: NSRect(x: 0, y: 0, width: 1024, height: 768), configuration: config)
    print("[WebView_Create] created WKWebView \(created)")

    // Set up drop handler and register for drag types
    dropHandler.webView = created
    created.dropHandler = dropHandler
    created.registerForDraggedTypes([.fileURL])

    // Set webView for file dialog handler
    fileDialogHandler.webView = created

    return CFBridgingRetain(created)
}

@_cdecl("WebView_SetEnableDevTools")
public func WebView_SetEnableDevTools(_ viewHandle: UnsafeMutableRawPointer?, _ enable: Bool) {
    guard let viewHandle = viewHandle else { return }
    let webView = Unmanaged<WKWebView>.fromOpaque(viewHandle).takeUnretainedValue()
    webView.configuration.preferences.setValue(enable, forKey: "developerExtrasEnabled")
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
    webView.evaluateJavaScript(jsString, completionHandler: commonJSCompletionHandler(webView: webView))
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