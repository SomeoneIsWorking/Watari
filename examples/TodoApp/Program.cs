new Watari.FrameworkBuilder()
    .Expose<Api>()
    .FrontendPath("frontend")
    .Build()
    .Run(args);
