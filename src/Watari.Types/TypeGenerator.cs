using System.Reflection;
using System.Text;
using Watari.Types;

namespace Watari;

public class TypeGenerator(TypeGeneratorOptions options)
{
    private readonly TypeGeneratorOptions options = options;
    private readonly HashSet<Type> _collectedTypes = [];

    public void Generate()
    {
        Console.WriteLine($"Generating TypeScript definitions in {options.OutputPath}...");
        if (options.ExposedTypes is not { Count: > 0 })
        {
            return;
        }

        CollectTypes();

        var outputDir = Path.Combine(options.OutputPath, "src", "generated");
        Directory.CreateDirectory(outputDir);

        foreach (var type in options.ExposedTypes)
        {
            Console.WriteLine($"Processing type: {type.Name}");
            var sb = new StringBuilder();
            sb.AppendLine("import * as models from \"./models\";");
            sb.AppendLine();

            sb.AppendLine($"export class {type.Name} {{");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => !m.IsSpecialName && m.DeclaringType == type); // only declared in this type
                              
            foreach (var method in methods)
            {
                var paramList = string.Join(", ", method.GetParameters()
                    .Select(p => $"{p.Name}: {MapType(p.ParameterType)}"));
                var paramNames = string.Join(", ", method.GetParameters().Select(p => p.Name));
                var returnType = MapType(method.ReturnType);
                sb.AppendLine($"    static {method.Name}({paramList}): Promise<{returnType}> {{");
                var args = string.IsNullOrEmpty(paramNames) ? "" : $", {paramNames}";
                sb.AppendLine($"        return watari.invoke<{returnType}>(\"{type.Name}.{method.Name}\"{args});");
                sb.AppendLine("    }");
                Console.WriteLine($"> {method.Name}({paramList}): Promise<{returnType}>");
            }
            sb.AppendLine("}");

            var fileName = ToCamelCase(type.Name) + ".ts";
            var outputFile = Path.Combine(outputDir, fileName);
            File.WriteAllText(outputFile, sb.ToString());
            Console.WriteLine($"Generated file: {outputFile}\n");
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
        Console.WriteLine($"Generated models file: {modelsFile}:\n\n{modelsSb}");

        File.WriteAllText(Path.Combine(outputDir, ".gitignore"), "*");

        var dtsFile = Path.Combine(outputDir, "watari.d.ts");
        File.WriteAllText(dtsFile, "declare global {\n    const watari: {\n        invoke<T>(method: string, ...args: any[]): Promise<T>;\n        on(event: string, handler: (data: any) => void): void;\n        off(event: string, handler: (data: any) => void): void;\n    };\n}\n\nexport {};\n");
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
        }
    }

    private void CollectTypes(Type t)
    {
        if (_collectedTypes.Contains(t) || IsPrimitive(t))
        {
            return;
        }

        var handlerType = typeof(ITypeHandler<>).MakeGenericType(t);

        if (options.Provider.GetService(handlerType) is ITypeHandler handler)
        {
            var interfaceType = handler.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeHandler<,>));
            var tsType = interfaceType.GetGenericArguments()[1];
            CollectTypes(tsType);
            return;
        }

        if (IsDictionary(t))
        {
            var dictInterface = t.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));
            var keyType = dictInterface.GetGenericArguments()[0];
            var valueType = dictInterface.GetGenericArguments()[1];
            CollectTypes(keyType);
            CollectTypes(valueType);
            return;
        }

        if (IsEnumerable(t))
        {
            var enumInterface = t.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
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

    private string MapType(Type type)
    {
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
         type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) ||
         type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            return "number";
        if (type == typeof(string))
            return "string";
        if (type == typeof(bool))
            return "boolean";
        if (type == typeof(void))
            return "void";

        var handlerType = typeof(ITypeHandler<>).MakeGenericType(type);
        if (options.Provider.GetService(handlerType) is ITypeHandler temp)
        {
            var interfaceType = temp.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeHandler<,>));
            var tsType = interfaceType.GetGenericArguments()[1];
            return MapType(tsType);
        }
        if (IsDictionary(type))
        {
            var dictInterface = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));
            var keyType = dictInterface.GetGenericArguments()[0];
            var valueType = dictInterface.GetGenericArguments()[1];
            return $"Record<{MapType(keyType)}, {MapType(valueType)}>";
        }
        if (IsEnumerable(type))
        {
            var enumInterface = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            var itemType = enumInterface.GetGenericArguments()[0];
            return $"{MapType(itemType)}[]";
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = type.GetGenericArguments()[0];
            return MapType(innerType);
        }

        // For complex types, return the type name prefixed with models
        return "models." + type.Name;
    }

    private static bool IsEnumerable(Type type)
    {
        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    private static bool IsDictionary(Type type)
    {
        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));
    }
}
