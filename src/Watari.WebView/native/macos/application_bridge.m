#include "objc/runtime.h"
#include <AppKit/AppKit.h>
#import <Cocoa/Cocoa.h>
#include <stdlib.h>

@interface ApplicationDelegate : NSObject <NSApplicationDelegate>

@property(strong) IBOutlet NSWindow *mainWindow;

@end

@implementation ApplicationDelegate

- (BOOL)applicationShouldHandleReopen:(NSApplication *)sender
                    hasVisibleWindows:(BOOL)flag {
  NSLog(@"[ApplicationDelegate] applicationShouldHandleReopen called");
  [self.mainWindow makeKeyAndOrderFront:nil];
  return YES;
}

@end

__attribute__((visibility("default"))) CFTypeRef Application_Init(void) {
  NSLog(@"[ApplicationBridge] initializing NSApp");
  NSApplication *app = [NSApplication sharedApplication];
  NSLog(@"[ApplicationBridge] NSApplication created");
  [app setActivationPolicy:NSApplicationActivationPolicyRegular];

  NSMenu *menubar = [[NSMenu alloc] init];
  NSMenuItem *appMenuItem = [[NSMenuItem alloc] init];
  [menubar addItem:appMenuItem];

  NSMenu *appMenu = [[NSMenu alloc] initWithTitle:@""];
  NSString *appName = [[NSProcessInfo processInfo] processName];
  NSMenuItem *quitItem = [[NSMenuItem alloc]
      initWithTitle:[NSString stringWithFormat:@"Quit %@", appName]
             action:@selector(terminate:)
      keyEquivalent:@"q"];
  [appMenu addItem:quitItem];
  [appMenuItem setSubmenu:appMenu];
  [app setMainMenu:menubar];
  ApplicationDelegate *delegate = [[ApplicationDelegate alloc] init];
  [app setDelegate:delegate];
  objc_setAssociatedObject(app, "ApplicationDelegate", delegate,
                           OBJC_ASSOCIATION_RETAIN_NONATOMIC);

  NSLog(@"[ApplicationBridge] main menu created");
  return CFBridgingRetain(app);
}

__attribute__((visibility("default"))) void Application_RunLoop(void *app) {
  @autoreleasepool {
    NSLog(@"[ApplicationBridge runLoop] enter");
    [(__bridge NSApplication *)app run];
    NSLog(@"[ApplicationBridge runLoop] exit");
  }
}

__attribute__((visibility("default"))) void Application_StopLoop(void *app) {
  NSLog(@"[ApplicationBridge stopLoop] stopping");
  [(__bridge NSApplication *)app stop:nil];
}

__attribute__((visibility("default"))) void
Application_SetMainWindow(void *app, void *window) {
  @autoreleasepool {
    NSLog(@"[ApplicationBridge setKeyWindow] setting key window");
    NSApplication *application = (__bridge NSApplication *)app;
    NSWindow *nsWindow = (__bridge NSWindow *)window;
    [nsWindow makeKeyAndOrderFront:nil];
    ApplicationDelegate *delegate =
        (ApplicationDelegate *)[application delegate];
    delegate.mainWindow = nsWindow;
    for (NSWindow *win in [application windows]) {
        [win setReleasedWhenClosed:[win isKeyWindow]];
  }
    NSLog(@"[ApplicationBridge setKeyWindow] set key window %p", nsWindow);
  }
}

__attribute__((visibility("default"))) void
Application_RunOnMainThread(void *app, void (*callback)(void)) {
  dispatch_async(dispatch_get_main_queue(), ^{
    callback();
  });
}

__attribute__((visibility("default"))) void
Application_AddMenuItem(void *app, const char *title) {
  @autoreleasepool {
    NSApplication *application = (__bridge NSApplication *)app;
    NSMenu *menubar = [application mainMenu];
    NSMenuItem *appMenuItem = [menubar itemAtIndex:0];
    NSMenu *appMenu = [appMenuItem submenu];
    NSMenuItem *newItem = [[NSMenuItem alloc] initWithTitle:[NSString stringWithUTF8String:title]
                                                     action:@selector(dummyAction:)
                                              keyEquivalent:@""];
    [appMenu addItem:newItem];
  }
}