// Window-only bridge. Creates an NSWindow and allows setting a content view.

#import <Cocoa/Cocoa.h>
#include <Foundation/Foundation.h>
#include <stdlib.h>

__attribute__((visibility("default"))) CFTypeRef Window_CreateWindow(void) {
  @autoreleasepool {
    NSLog(@"[Window_CreateWindow] creating NSWindow");
    NSRect frame = NSMakeRect(100, 100, 1024, 768);
    NSLog(@"[WindowBridge_CreateWindow] frame: %@", NSStringFromRect(frame));
    NSWindow *window =
        [[NSWindow alloc] initWithContentRect:frame
                                    styleMask:(NSWindowStyleMaskTitled |
                                               NSWindowStyleMaskResizable |
                                               NSWindowStyleMaskClosable)
                                      backing:NSBackingStoreBuffered
                                        defer:NO];
    NSLog(@"[WindowBridge_CreateWindow] window initialized");
    [window makeKeyWindow];
    NSLog(@"[WindowBridge_CreateWindow] created window %p", window);
    return CFBridgingRetain(window);
  }
}

__attribute__((visibility("default"))) void
Window_SetContent(void *windowHandle, void *viewHandle) {
  @autoreleasepool {
    NSLog(@"[Window_SetContent] setting content view");
    NSWindow *window = (__bridge NSWindow *)windowHandle;
    NSLog(@"[Window_SetContent] got window %p", window);
    NSView *view = (__bridge NSView *)viewHandle;
    NSLog(@"[Window_SetContent] got view %p", view);
    NSRect contentRect = [[window contentView] frame];
    NSLog(@"[Window_SetContent] content rect: %@",
          NSStringFromRect(contentRect));
    [view setFrame:contentRect];
    NSLog(@"[Window_SetContent] set view frame to content rect");
    [window setContentView:view];

    NSLog(@"[Window_SetContent] set view %p into window %p", view, window);
    [window makeKeyAndOrderFront:nil];
  }
}

__attribute__((visibility("default"))) void Window_Destroy(void *windowHandle) {
  @autoreleasepool {
    if (!windowHandle)
      return;
    NSWindow *window = (__bridge NSWindow *)windowHandle;
    [window orderOut:nil];
    NSLog(@"[WindowBridge_Destroy] destroyed window %p", window);
  }
}
