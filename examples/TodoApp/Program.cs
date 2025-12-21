new Watari.FrameworkBuilder()
    .SetDev(true)
    .Expose<Api>()
    .SetFrontendPathRelative("frontend")
    .Build()
    .Run(args);
