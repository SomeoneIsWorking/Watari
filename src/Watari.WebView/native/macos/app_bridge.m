#import <Cocoa/Cocoa.h>
#include <stdlib.h>

// Application-level bridge: initialize NSApplication, run/stop the main loop, and setup minimal menu

__attribute__((visibility("default"))) void wk_app_init(void) {
    NSLog(@"[wk_app_init] enter");

    void (^doInit)(void) = ^{
        NSLog(@"[wk_app_init] initializing NSApp");
        if (NSApp == nil) {
            [NSApplication sharedApplication];
            NSLog(@"[wk_app_init] NSApplication created");
        }
        [NSApp setActivationPolicy:NSApplicationActivationPolicyRegular];

        if ([NSApp mainMenu] == nil) {
            NSMenu *menubar = [[NSMenu alloc] init];
            NSMenuItem *appMenuItem = [[NSMenuItem alloc] init];
            [menubar addItem:appMenuItem];

            NSMenu *appMenu = [[NSMenu alloc] initWithTitle:@""];
            NSString *appName = [[NSProcessInfo processInfo] processName];
            NSMenuItem *quitItem = [[NSMenuItem alloc] initWithTitle:[NSString stringWithFormat:@"Quit %@", appName]
                                                                                                                    action:@selector(terminate:)
                                                                                                     keyEquivalent:@"q"];
            [appMenu addItem:quitItem];
            [appMenuItem setSubmenu:appMenu];
            [NSApp setMainMenu:menubar];
            NSLog(@"[wk_app_init] main menu created");
        }
    };

    if ([NSThread isMainThread]) {
        NSLog(@"[wk_app_init] already on main thread, running init inline");
        doInit();
    } else {
        NSLog(@"[wk_app_init] dispatching init to main thread");
        dispatch_sync(dispatch_get_main_queue(), doInit);
    }

    NSLog(@"[wk_app_init] exit");
}

__attribute__((visibility("default"))) void wk_run_loop(void) {
    @autoreleasepool {
        NSLog(@"[wk_run_loop] enter");
        if (NSApp == nil) {
            [NSApplication sharedApplication];
            NSLog(@"[wk_run_loop] NSApplication created in run_loop");
        }
        NSLog(@"[wk_run_loop] running NSApp");
        [NSApp run];
        NSLog(@"[wk_run_loop] exit");
    }
}

__attribute__((visibility("default"))) void wk_stop_loop(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (NSApp != nil) {
            NSLog(@"[wk_stop_loop] stopping NSApp");
            [NSApp stop:nil];
        }
    });
}
