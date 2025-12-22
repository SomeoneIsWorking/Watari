using Watari.Types;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace Watari.Tests;

public class SerializationTests
{
    [Fact]
    public void TestNestedTypeWithHandler()
    {
        var builder = new FrameworkBuilder()
            .AddHandler<X, Y, XHandler>()
            .Build();

        // Create TypeConverter instance
        var helper = new TypeConverter(builder.Options.JsonConverters);

        // Test serialization of Z containing X
        var z = new Z { MyProperty = new X { Value = 42 } };
        var json = helper.SerializeOutput(z);
        // Should serialize X as Y: { Value: 42 }
        Assert.Contains("\"Value\":42", json);

        // Test deserialization
        var deserializedZ = (Z?)helper.ParseInput(json, typeof(Z));
        Assert.NotNull(deserializedZ);
        Assert.NotNull(deserializedZ.MyProperty);
        Assert.Equal(42, deserializedZ.MyProperty.Value);
    }

    [Fact]
    public void TestAsyncResponse()
    {
        var builder = new FrameworkBuilder()
            .AddHandler<X, Y, XHandler>()
            .Build();
        var helper = new TypeConverter(builder.Options.JsonConverters);

        var taskX = Task.FromResult(new X { Value = 42 });
        var json = helper.SerializeOutput(taskX);
        Assert.Contains("\"Value\":42", json);

        // Deserialize back
        var deserializedX = (X?)helper.ParseInput(json, typeof(X));
        Assert.NotNull(deserializedX);
        Assert.Equal(42, deserializedX.Value);
    }

    [Fact]
    public void TestTypeScriptGeneration()
    {
        var xHandler = new XHandler();
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ITypeHandler<X>), xHandler);
        var provider = services.BuildServiceProvider();
        var exposedTypes = new List<Type> { typeof(TestApi) };
        var options = new TypeGeneratorOptions
        {
            OutputPath = Path.GetTempPath(),
            ExposedTypes = exposedTypes,
            Provider = provider
        };
        var types = new TypeGenerator(options);
        types.Generate();

        // Check if files are generated
        var outputDir = Path.Combine(options.OutputPath, "src", "generated");
        Assert.True(Directory.Exists(outputDir));
        var apiFile = Path.Combine(outputDir, "testApi.ts");
        var modelsFile = Path.Combine(outputDir, "models.ts");
        var dtsFile = Path.Combine(outputDir, "watari.d.ts");
        var gitignoreFile = Path.Combine(outputDir, ".gitignore");
        Assert.True(File.Exists(apiFile));
        Assert.True(File.Exists(modelsFile));
        Assert.True(File.Exists(dtsFile));
        Assert.True(File.Exists(gitignoreFile));

        // Check contents
        var apiContent = File.ReadAllText(apiFile);
        Assert.Contains("export class TestApi {", apiContent);
        Assert.Contains("static GetX(): Promise<models.Y> {", apiContent);
        Assert.Contains("static GetXAsync(): Promise<models.Y> {", apiContent);

        var modelsContent = File.ReadAllText(modelsFile);
        Assert.Contains("export interface Y {", modelsContent);
        Assert.Contains("Value: number;", modelsContent);

        var dtsContent = File.ReadAllText(dtsFile);
        Assert.Contains("declare global {", dtsContent);
        Assert.Contains("function watari_invoke<T>(method: string, ...args: any[]): Promise<T>;", dtsContent);

        var gitignoreContent = File.ReadAllText(gitignoreFile);
        Assert.Equal("*", gitignoreContent.Trim());
    }
}

// Test types
public class X
{
    public int Value { get; set; }
}

public class Y
{
    public int Value { get; set; }
}

public class Z
{
    public X? MyProperty { get; set; }
}

public class XHandler : ITypeHandler<X, Y>
{
    public Y ToTypeScript(X obj)
    {
        return new Y { Value = obj.Value };
    }

    public X FromTypeScript(Y dto)
    {
        return new X { Value = dto.Value };
    }
}

public class TestApi
{
    public X GetX() => new X { Value = 42 };
    public Task<X> GetXAsync() => Task.FromResult(new X { Value = 42 });
}