new Watari.FrameworkBuilder()
    .SetDev(true)
    .Expose<Api>()
    .Expose<Api2>()
    .SetFrontendPathRelative("frontend")
    .Build()
    .Run(args);

