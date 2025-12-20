#include <AppKit/AppKit.h>
#import <Cocoa/Cocoa.h>
#include <stdlib.h>

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
