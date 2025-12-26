using System.Reflection;
using System.Text;
using Watari.Types;
using Microsoft.Extensions.Logging;

namespace Watari;

public static class TypeGenerator
{
    public static void Generate(TypeGeneratorOptions options, ILogger logger)
    {
        new TypeGeneratorInstance(options, logger).Generate();
    }
}

public class TypeGeneratorInstance(TypeGeneratorOptions options, ILogger logger)
{
    private readonly HashSet<Type> _collectedTypes = [];

    public void Generate()
    {
        logger.LogInformation("Generating TypeScript definitions in {OutputPath}...", options.OutputPath);
        if (options.ExposedTypes is not { Count: > 0 })
        {
            return;
        }

        CollectTypes();

        var outputDir = Path.Combine(options.OutputPath, "src", "generated");
        Directory.CreateDirectory(outputDir);

        foreach (var type in options.ExposedTypes)
        {
            logger.LogInformation("Processing type: {TypeName}", type.Name);
            var sb = new StringBuilder();
            var usedTypes = new HashSet<string>();

            sb.AppendLine($"export class {type.Name} {{");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => !m.IsSpecialName && m.DeclaringType == type); // only declared in this type

            foreach (var method in methods)
            {
                var paramList = string.Join(", ", method.GetParameters()
                    .Select(p => $"{p.Name}: {MapType(p.ParameterType, usedTypes)}"));
                var paramNames = string.Join(", ", method.GetParameters().Select(p => p.Name));
                var returnType = MapType(method.ReturnType, usedTypes);
                sb.AppendLine($"    static {method.Name}({paramList}): Promise<{returnType}> {{");
                var args = string.IsNullOrEmpty(paramNames) ? "" : $", {paramNames}";
                sb.AppendLine($"        return watari.invoke<{returnType}>(\"{type.Name}.{method.Name}\"{args});");
                sb.AppendLine("    }");
                logger.LogInformation("> {MethodName}({ParamList}): Promise<{ReturnType}>", method.Name, paramList, returnType);
            }

            var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance);
            foreach (var evt in events)
            {
                var invokeMethod = evt.EventHandlerType!.GetMethod("Invoke");
                var parameters = invokeMethod!.GetParameters();
                if (parameters.Length == 1)
                {
                    var paramType = parameters[0].ParameterType;
                    CollectTypes(paramType);
                    var paramTypeName = MapType(paramType, usedTypes);
                    sb.AppendLine($"    static {evt.Name}(handler: (data: {paramTypeName}) => void): () => void {{");
                    sb.AppendLine($"        watari.on(\"{type.Name}.{evt.Name}\", handler);");
                    sb.AppendLine($"        return () => watari.off(\"{type.Name}.{evt.Name}\", handler);");
                    sb.AppendLine("    }");
                    logger.LogInformation("> {EventName}(handler: (data: {ParamTypeName}) => void): () => void", evt.Name, paramTypeName);
                }
            }
            sb.AppendLine("}");

            var generatedCode = sb.ToString();

            if (usedTypes.Any())
            {
                var importLine = $"import type {{ {string.Join(", ", usedTypes.OrderBy(t => t))} }} from \"./models\";";
                generatedCode = importLine + "\n\n" + generatedCode;
            }

            var fileName = ToCamelCase(type.Name) + ".ts";
            var outputFile = Path.Combine(outputDir, fileName);
            File.WriteAllText(outputFile, generatedCode);
            logger.LogInformation("Generated file: {OutputFile}", outputFile);
        }

        var modelsSb = new StringBuilder();
        foreach (var t in _collectedTypes.OrderBy(t => t.Name))
        {
            modelsSb.AppendLine($"export interface {t.Name} {{");
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                modelsSb.AppendLine($"    {prop.Name}: {MapType(prop.PropertyType)};");
            }
            modelsSb.AppendLine("}");
            modelsSb.AppendLine();
        }

        var modelsFile = Path.Combine(outputDir, "models.ts");
        File.WriteAllText(modelsFile, modelsSb.ToString());
        logger.LogInformation("Generated models file: {ModelsFile}", modelsFile);

        File.WriteAllText(Path.Combine(outputDir, ".gitignore"), "*");

        var targetDtsFile = Path.Combine(outputDir, "watari.d.ts");
        File.WriteAllText(targetDtsFile, options.WatariDtsContent);
    }

    private void CollectTypes()
    {
        foreach (var type in options.ExposedTypes)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => !m.IsSpecialName && m.DeclaringType == type);
            foreach (var method in methods)
            {
                CollectTypes(method.ReturnType);
                foreach (var param in method.GetParameters())
                {
                    CollectTypes(param.ParameterType);
                }
            }

            var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance);
            foreach (var evt in events)
            {
                var invokeMethod = evt.EventHandlerType!.GetMethod("Invoke");
                foreach (var param in invokeMethod!.GetParameters())
                {
                    CollectTypes(param.ParameterType);
                }
            }
        }
    }

    private void CollectTypes(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = t.GetGenericArguments()[0];
            CollectTypes(innerType);
            return;
        }

        if (_collectedTypes.Contains(t) || IsPrimitive(t) || t == typeof(Task))
        {
            return;
        }

        if (options.Handlers.TryGetValue(t, out var handler))
        {
            var interfaceType = handler.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeHandler<,>));
            var tsType = interfaceType.GetGenericArguments()[1];
            CollectTypes(tsType);
            return;
        }

        if (GetDictionary(t) is { } dictInterface)
        {
            var keyType = dictInterface.GetGenericArguments()[0];
            var valueType = dictInterface.GetGenericArguments()[1];
            CollectTypes(keyType);
            CollectTypes(valueType);
            return;
        }

        if (GetEnumerable(t) is { } enumInterface)
        {
            var itemType = enumInterface.GetGenericArguments()[0];
            CollectTypes(itemType);
            return;
        }

        _collectedTypes.Add(t);
        foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            CollectTypes(prop.PropertyType);
        }
    }

    private static string ToCamelCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToLower(s[0]) + s[1..];
    }

    private static bool IsPrimitive(Type t)
    {
        return t.IsPrimitive || t == typeof(string) || t == typeof(decimal) || t == typeof(void);
    }

    private string MapType(Type type, HashSet<string>? usedTypes = null)
    {
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
         type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) ||
         type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            return "number";
        if (type == typeof(string))
            return "string";
        if (type == typeof(bool))
            return "boolean";
        if (type == typeof(void) || type == typeof(Task))
            return "void";

        if (options.Handlers.TryGetValue(type, out var handler))
        {
            var interfaceType = handler.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeHandler<,>));
            var tsType = interfaceType.GetGenericArguments()[1];
            return MapType(tsType, usedTypes);
        }
        if (GetDictionary(type) is { } dictInterface)
        {
            var keyType = dictInterface.GetGenericArguments()[0];
            var valueType = dictInterface.GetGenericArguments()[1];
            return $"Record<{MapType(keyType, usedTypes)}, {MapType(valueType, usedTypes)}>";
        }
        if (GetEnumerable(type) is { } enumInterface)
        {
            var itemType = enumInterface.GetGenericArguments()[0];
            return $"{MapType(itemType, usedTypes)}[]";
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = type.GetGenericArguments()[0];
            return MapType(innerType, usedTypes);
        }

        // For complex types, return the type name and add to used types
        usedTypes?.Add(type.Name);
        return type.Name;
    }

    private static IEnumerable<Type> SelfAndInterfaces(Type type)
    {
        return type.GetInterfaces().Prepend(type);
    }

    private static Type? GetEnumerable(Type type)
    {
        return SelfAndInterfaces(type).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    private static Type? GetDictionary(Type type)
    {
        return SelfAndInterfaces(type).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));
    }
}
