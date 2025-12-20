
new Watari.Framework(new Watari.FrameworkOptions
{
    Dev = true,
    FrontendPath = Path.Combine(Watari.FrameworkOptions.GetCallingFilePath(), "frontend")
}).Run(args);
