#import <Cocoa/Cocoa.h>
#include <Foundation/Foundation.h>
#import <WebKit/WebKit.h>
#import <objc/runtime.h>
#include <stdlib.h>

@interface WebViewConsoleLogger : NSObject <WKScriptMessageHandler>
@end

@implementation WebViewConsoleLogger
- (void)userContentController:(WKUserContentController *)userContentController didReceiveScriptMessage:(WKScriptMessage *)message {
    if ([message.name isEqualToString:@"consoleLog"]) {
        NSLog(@"[Console] %@", message.body);
    }
}
@end

__attribute__((visibility("default"))) CFTypeRef WebView_Create() {
  @autoreleasepool {
    NSLog(@"[WebView_Create] creating WKWebView");
    WKWebViewConfiguration *config = [[WKWebViewConfiguration alloc] init];
    
    // Set up console.log redirection
    WKUserContentController *userContentController = [[WKUserContentController alloc] init];
    WebViewConsoleLogger *logger = [[WebViewConsoleLogger alloc] init];
    [userContentController addScriptMessageHandler:logger name:@"consoleLog"];
    
    // Inject script to override console.log
    NSString *consoleOverrideScript = @"console.log = function(...args) { window.webkit.messageHandlers.consoleLog.postMessage(args.join(' ')); };";
    WKUserScript *userScript = [[WKUserScript alloc] initWithSource:consoleOverrideScript injectionTime:WKUserScriptInjectionTimeAtDocumentStart forMainFrameOnly:YES];
    [userContentController addUserScript:userScript];
    
    [config setUserContentController:userContentController];
    
    WKWebView *created =
        [[WKWebView alloc] initWithFrame:NSMakeRect(0, 0, 1024, 768)
                           configuration:config];
    NSLog(@"[WebView_Create] created WKWebView %p", created);

    return CFBridgingRetain(created);
  }
}

__attribute__((visibility("default"))) void WebView_Navigate(void *viewHandle,
                                                             const char *url) {
  @autoreleasepool {
    WKWebView *webView = (__bridge WKWebView *)viewHandle;
    NSString *nsurl = [NSString stringWithUTF8String:url];
    NSURL *u = [NSURL URLWithString:nsurl];
    NSLog(@"[WebView_Navigate] created NSURL: %@", u);
    NSURLRequest *req = [NSURLRequest requestWithURL:u];
    NSLog(@"[WebView_Navigate] navigating WKWebView %p to URL: %@", webView,
          nsurl);
    [webView loadRequest:req];
  }
}

__attribute__((visibility("default"))) void WebView_Eval(void *viewHandle,
                                                         const char *js) {
  @autoreleasepool {
    WKWebView *webView = (__bridge WKWebView *)viewHandle;
    NSString *jsString = [NSString stringWithUTF8String:js];
    NSLog(@"[WebView_Eval] evaluating JavaScript on WKWebView %p: %@", webView,
          jsString);
    [webView
        evaluateJavaScript:jsString
         completionHandler:^(id result, NSError *error) {
           if (error) {
             NSLog(@"[WebView_Eval] JavaScript evaluation error: %@", error);
           } else {
             NSLog(@"[WebView_Eval] JavaScript evaluation result: %@", result);
           }
         }];
  }
}

__attribute__((visibility("default"))) void WebView_Destroy(void *viewHandle) {
  @autoreleasepool {
    if (!viewHandle)
      return;
    WKWebView *webView = (__bridge WKWebView *)viewHandle;
    NSLog(@"[WebView_Destroy] destroying WKWebView %p", webView);
    // Remove from superview if attached
    [webView removeFromSuperview];
    // Release the webView
    webView = nil;
    NSLog(@"[WebView_Destroy] destroyed WKWebView");
  }
}

__attribute__((visibility("default"))) void WebView_AddUserScript(void *viewHandle, const char *scriptSource, int injectionTime, bool forMainFrameOnly) {
  @autoreleasepool {
    WKWebView *webView = (__bridge WKWebView *)viewHandle;
    NSString *source = [NSString stringWithUTF8String:scriptSource];
    WKUserScriptInjectionTime time = (WKUserScriptInjectionTime)injectionTime;
    WKUserScript *userScript = [[WKUserScript alloc] initWithSource:source injectionTime:time forMainFrameOnly:forMainFrameOnly];
    [webView.configuration.userContentController addUserScript:userScript];
  }
}