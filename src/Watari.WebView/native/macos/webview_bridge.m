#import <Cocoa/Cocoa.h>
#import <WebKit/WebKit.h>
#include <stdlib.h>

typedef void (*message_cb_t)(const char *);

static BOOL g_terminateOnWindowClose = NO;

@interface WKBridge : NSObject <WKScriptMessageHandler, NSWindowDelegate> {
  WKWebView *webView;
  NSWindow *window;
  message_cb_t cb;
}
- (instancetype)initWithURL:(NSString *)url cb:(message_cb_t)callback;
- (void)navigate:(NSString *)url;
- (void)eval:(NSString *)js;
- (void)destroy;
@end

@implementation WKBridge

- (instancetype)initWithURL:(NSString *)url cb:(message_cb_t)callback {
  self = [super init];
  if (self) {
    cb = callback;
    dispatch_async(dispatch_get_main_queue(), ^{
      NSRect frame = NSMakeRect(100, 100, 1024, 768);
      window =
          [[NSWindow alloc] initWithContentRect:frame
                                      styleMask:(NSWindowStyleMaskTitled |
                                                 NSWindowStyleMaskResizable |
                                                 NSWindowStyleMaskClosable)
                                        backing:NSBackingStoreBuffered
                                          defer:NO];

      WKWebViewConfiguration *config = [[WKWebViewConfiguration alloc] init];
      WKUserContentController *ucc = [[WKUserContentController alloc] init];
      [ucc addScriptMessageHandler:self name:@"bridge"];
      config.userContentController = ucc;

      webView = [[WKWebView alloc] initWithFrame:frame configuration:config];
      [window setContentView:webView];
      [window setDelegate:self];

      [[NSNotificationCenter defaultCenter]
          addObserver:self
             selector:@selector(appDidBecomeActive:)
                 name:NSApplicationDidBecomeActiveNotification
               object:nil];
      [window makeKeyAndOrderFront:nil];
      [NSApp activateIgnoringOtherApps:YES];

      if (url) {
        NSURL *nsurl = [NSURL URLWithString:url];
        NSURLRequest *req = [NSURLRequest requestWithURL:nsurl];
        [webView loadRequest:req];
      }
    });
  }
  return self;
}

- (BOOL)windowShouldClose:(id)sender {
  if (g_terminateOnWindowClose) {
    return YES; // allow normal close -> will trigger windowWillClose if needed
  }
  // Hide app instead of closing window to keep window object alive for
  // re-showing
  dispatch_async(dispatch_get_main_queue(), ^{
    [NSApp hide:nil];
  });
  return NO;
}

- (void)appDidBecomeActive:(NSNotification *)notification {
  dispatch_async(dispatch_get_main_queue(), ^{
    if (window && ![window isVisible]) {
      if ([window respondsToSelector:@selector(makeKeyAndOrderFront:)]) {
        [window makeKeyAndOrderFront:nil];
        [NSApp activateIgnoringOtherApps:YES];
      }
    }
  });
}

- (void)navigate:(NSString *)url {
  dispatch_async(dispatch_get_main_queue(), ^{
    NSURL *nsurl = [NSURL URLWithString:url];
    NSURLRequest *req = [NSURLRequest requestWithURL:nsurl];
    [webView loadRequest:req];
  });
}

- (void)eval:(NSString *)js {
  dispatch_async(dispatch_get_main_queue(), ^{
    [webView evaluateJavaScript:js
              completionHandler:^(id result, NSError *error) {
                if (result) {
                  NSString *s = [result description];
                  const char *utf8 = [s UTF8String];
                  if (cb) {
                    char *copy = strdup(utf8);
                    cb((const char *)copy);
                  }
                }
              }];
  });
}

- (void)userContentController:(WKUserContentController *)userContentController
      didReceiveScriptMessage:(WKScriptMessage *)message {
  if (cb && message.body) {
    NSString *s = [message.body description];
    const char *utf8 = [s UTF8String];
    char *copy = strdup(utf8);
    cb((const char *)copy);
  }
}

- (void)destroy {
  dispatch_async(dispatch_get_main_queue(), ^{
    [[NSNotificationCenter defaultCenter]
        removeObserver:self
                  name:NSApplicationDidBecomeActiveNotification
                object:nil];
    [window orderOut:nil];
    window = nil;
    webView = nil;
  });
}

@end

#ifdef __cplusplus
extern "C" {

__attribute__((visibility("default"))) void
wk_set_terminate_on_window_close(int val) {
  g_terminateOnWindowClose = val ? YES : NO;
  NSLog(@"[wk_set_terminate_on_window_close] set to %d", val);
}
#endif

__attribute__((visibility("default"))) void *wk_create_window(const char *url,
                                                              message_cb_t cb) {
  @autoreleasepool {
    NSLog(@"[wk_create_window] enter: %s", url ?: "(null)");
    NSString *nsurl = url ? [NSString stringWithUTF8String:url] : nil;
    WKBridge *bridge = [[WKBridge alloc] initWithURL:nsurl cb:cb];
    NSLog(@"[wk_create_window] created bridge: %p", bridge);
    return (__bridge void *)bridge;
  }
}

__attribute__((visibility("default"))) void wk_navigate(void *handle,
                                                        const char *url) {
  @autoreleasepool {
    NSLog(@"[wk_navigate] handle=%p url=%s", handle, url ?: "(null)");
    WKBridge *bridge = (__bridge WKBridge *)handle;
    if (bridge && url) {
      NSString *nsurl = [NSString stringWithUTF8String:url];
      [bridge navigate:nsurl];
      NSLog(@"[wk_navigate] dispatched navigate");
    }
  }
}

__attribute__((visibility("default"))) void wk_eval(void *handle,
                                                    const char *js) {
  @autoreleasepool {
    NSLog(@"[wk_eval] handle=%p js=%s", handle, js ?: "(null)");
    WKBridge *bridge = (__bridge WKBridge *)handle;
    if (bridge && js) {
      NSString *nsjs = [NSString stringWithUTF8String:js];
      [bridge eval:nsjs];
      NSLog(@"[wk_eval] dispatched eval");
    }
  }
}

__attribute__((visibility("default"))) void wk_destroy(void *handle) {
  @autoreleasepool {
    NSLog(@"[wk_destroy] handle=%p", handle);
    WKBridge *bridge = (__bridge WKBridge *)handle;
    if (bridge) {
      [bridge destroy];
      NSLog(@"[wk_destroy] destroyed bridge");
    }
  }
}

__attribute__((visibility("default"))) void wk_free_string(const char *s) {
  if (s)
    free((void *)s);
}

#ifdef __cplusplus
}
#endif
