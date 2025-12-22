#import <Cocoa/Cocoa.h>
#include <Foundation/Foundation.h>
#import <WebKit/WebKit.h>
#import <objc/runtime.h>
#include <stdlib.h>

@interface WebViewConsoleLogger : NSObject <WKScriptMessageHandler>
@property(nonatomic) void (*consoleCallback)
    (const char *level, const char *message);
@end

@implementation WebViewConsoleLogger
- (void)userContentController:(WKUserContentController *)userContentController
      didReceiveScriptMessage:(WKScriptMessage *)message {
  if ([message.name isEqualToString:@"consoleLog"]) {
    NSDictionary *dict = (NSDictionary *)message.body;
    NSString *level = dict[@"level"];
    NSString *msg = dict[@"message"];
    if (self.consoleCallback) {
      self.consoleCallback([level UTF8String], [msg UTF8String]);
    }
  }
}
@end

@interface WebViewDropHandler : NSObject <WKScriptMessageHandler>
@property(nonatomic, assign) WKWebView *webView;
@property(nonatomic, strong) NSString *callbackId;
@property(nonatomic, strong) NSString *selector;
@property(nonatomic, strong) NSString *allowedExtensions;
@end

@implementation WebViewDropHandler
- (void)userContentController:(WKUserContentController *)userContentController
      didReceiveScriptMessage:(WKScriptMessage *)message {
  if ([message.name isEqualToString:@"setDropZone"]) {
    NSDictionary *dict = (NSDictionary *)message.body;
    self.callbackId = dict[@"callbackId"];
    self.selector = dict[@"element"];
    self.allowedExtensions = dict[@"allowedExtensions"];
  } else if ([message.name isEqualToString:@"removeDropZone"]) {
    self.callbackId = nil;
    self.selector = nil;
    self.allowedExtensions = nil;
  }
}
@end

@interface WKWebView (DropSupport) <NSDraggingDestination>
@property(nonatomic) WebViewDropHandler *dropHandler;
@end

@implementation WKWebView (DropSupport)
- (void)setDropHandler:(WebViewDropHandler *)handler {
  objc_setAssociatedObject(self, @selector(dropHandler), handler,
                           OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}
- (WebViewDropHandler *)dropHandler {
  return objc_getAssociatedObject(self, @selector(dropHandler));
}
- (NSDragOperation)draggingEntered:(id<NSDraggingInfo>)sender {
  WebViewDropHandler *handler = self.dropHandler;
  if (!handler || !handler.selector)
    return NSDragOperationNone;
  NSPasteboard *pasteboard = [sender draggingPasteboard];
  NSArray *fileURLs =
      [pasteboard canReadObjectForClasses:@[ [NSURL class] ]
                                  options:@{
                                    NSPasteboardURLReadingFileURLsOnlyKey : @YES
                                  }]
          ? [pasteboard
                readObjectsForClasses:@[ [NSURL class] ]
                              options:@{
                                NSPasteboardURLReadingFileURLsOnlyKey : @YES
                              }]
          : @[];
  BOOL hasFiles = fileURLs.count > 0;
  if (!hasFiles)
    return NSDragOperationNone;

  NSPoint location = [sender draggingLocation];
  NSPoint pointInView = [self convertPoint:location fromView:nil];
  NSString *js = [NSString
      stringWithFormat:@"watari._checkDropZone('%@', %f, %f)", handler.selector,
                       pointInView.x, pointInView.y];
  [self evaluateJavaScript:js
         completionHandler:^(id result, NSError *error) {
           if (error) {
             NSLog(@"[WebView_Drag] JS error: %@", error);
           }
           BOOL isOver =
               [result isKindOfClass:[NSNumber class]] && [result boolValue];
           BOOL allowed = YES;
           if (handler.allowedExtensions &&
               handler.allowedExtensions.length > 0) {
             NSArray *allowedExts =
                 [handler.allowedExtensions componentsSeparatedByString:@","];
             for (NSURL *url in fileURLs) {
               NSString *path = [url path];
               NSString *ext = [path pathExtension];
               if (![allowedExts containsObject:ext]) {
                 allowed = NO;
                 break;
               }
             }
           }
           NSString *js2 = [NSString
               stringWithFormat:@"watari._updateDropZoneClass('%@', %d, %d)",
                                handler.selector, isOver ? 1 : 0,
                                allowed ? 1 : 0];
           [self evaluateJavaScript:js2 completionHandler:nil];
         }];
  return NSDragOperationCopy;
}
- (NSDragOperation)draggingUpdated:(id<NSDraggingInfo>)sender {
  return [self draggingEntered:sender];
}
- (void)draggingExited:(id<NSDraggingInfo>)sender {
  WebViewDropHandler *handler = self.dropHandler;
  if (handler && handler.selector) {
    NSString *clearJs = [NSString
        stringWithFormat:@"watari._clearDropZoneClass('%@')", handler.selector];
    [self evaluateJavaScript:clearJs completionHandler:nil];
  }
}
- (BOOL)prepareForDragOperation:(id<NSDraggingInfo>)sender {
  return YES;
}
- (BOOL)performDragOperation:(id<NSDraggingInfo>)sender {
  WebViewDropHandler *handler = self.dropHandler;
  NSString *clearJs = [NSString
      stringWithFormat:@"watari._clearDropZoneClass('%@')", handler.selector];
  [self evaluateJavaScript:clearJs completionHandler:nil];
  if (!handler.callbackId)
    return NO;
  NSPasteboard *pasteboard = [sender draggingPasteboard];
  NSArray *fileURLs = [pasteboard
      readObjectsForClasses:@[ [NSURL class] ]
                    options:@{NSPasteboardURLReadingFileURLsOnlyKey : @YES}];
  NSMutableArray *paths = [NSMutableArray array];
  for (NSURL *url in fileURLs) {
    if ([url isFileURL]) {
      [paths addObject:[url path]];
    }
  }
  if (paths.count == 0)
    return NO;

  NSPoint location = [sender draggingLocation];
  NSPoint pointInView = [self convertPoint:location fromView:nil];
  NSString *js = [NSString
      stringWithFormat:@"watari._checkDropZone('%@', %f, %f)", handler.selector,
                       pointInView.x, pointInView.y];
  [self evaluateJavaScript:js
         completionHandler:^(id result, NSError *error) {
           if (error) {
             NSLog(@"[WebView_Drag] JS error: %@", error);
           }
           BOOL isOver =
               [result isKindOfClass:[NSNumber class]] && [result boolValue];
           if (isOver) {
             NSError *error;
             NSData *jsonData = [NSJSONSerialization dataWithJSONObject:paths
                                                                options:0
                                                                  error:&error];
             if (jsonData) {
               NSString *jsonString =
                   [[NSString alloc] initWithData:jsonData
                                         encoding:NSUTF8StringEncoding];
               NSString *js2 =
                   [NSString stringWithFormat:@"watari.callbacks[%@] (%@)",
                                              handler.callbackId, jsonString];
               [self evaluateJavaScript:js2 completionHandler:nil];
             }
           }
         }];
  return YES;
}
@end

__attribute__((visibility("default"))) CFTypeRef
WebView_Create(void (*callback)(const char *, const char *)) {
  @autoreleasepool {
    NSLog(@"[WebView_Create] creating WKWebView");
    WKWebViewConfiguration *config = [[WKWebViewConfiguration alloc] init];

    // Set up console.log redirection
    WKUserContentController *userContentController =
        [[WKUserContentController alloc] init];
    WebViewConsoleLogger *logger = [[WebViewConsoleLogger alloc] init];
    logger.consoleCallback = callback;
    [userContentController addScriptMessageHandler:logger name:@"consoleLog"];

    // Set up drop zone handler
    WebViewDropHandler *dropHandler = [[WebViewDropHandler alloc] init];
    [userContentController addScriptMessageHandler:dropHandler
                                              name:@"setDropZone"];
    [userContentController addScriptMessageHandler:dropHandler
                                              name:@"removeDropZone"];

    // Inject script to override console methods
    NSString *consoleOverrideScript =
        @"var levels = ['log', 'error', 'warn', 'info', 'debug']; "
        @"levels.forEach(function(level) { console[level] = function(...args) "
        @"{ window.webkit.messageHandlers.consoleLog.postMessage({level: "
        @"level, message: args.join(' ')}); }; });";
    WKUserScript *userScript = [[WKUserScript alloc]
          initWithSource:consoleOverrideScript
           injectionTime:WKUserScriptInjectionTimeAtDocumentStart
        forMainFrameOnly:YES];
    [userContentController addUserScript:userScript];

    [config setUserContentController:userContentController];

    WKWebView *created =
        [[WKWebView alloc] initWithFrame:NSMakeRect(0, 0, 1024, 768)
                           configuration:config];
    NSLog(@"[WebView_Create] created WKWebView %p", created);

    // Set up drop handler and register for drag types
    dropHandler.webView = created;
    [created setDropHandler:dropHandler];
    [created registerForDraggedTypes:@[ NSPasteboardTypeFileURL ]];

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

__attribute__((visibility("default"))) void
WebView_AddUserScript(void *viewHandle, const char *scriptSource,
                      int injectionTime, bool forMainFrameOnly) {
  @autoreleasepool {
    WKWebView *webView = (__bridge WKWebView *)viewHandle;
    NSString *source = [NSString stringWithUTF8String:scriptSource];
    WKUserScriptInjectionTime time = (WKUserScriptInjectionTime)injectionTime;
    WKUserScript *userScript =
        [[WKUserScript alloc] initWithSource:source
                               injectionTime:time
                            forMainFrameOnly:forMainFrameOnly];
    [webView.configuration.userContentController addUserScript:userScript];
  }
}