using System.Reflection;
using System.Text;

namespace Watari;

public class Types
{
    public bool Generate(TypeGeneratorOptions options)
    {
        Console.WriteLine($"Generating TypeScript definitions in {options.OutputPath}...");
        if (options.ExposedTypes is not { Count: > 0 })
        {
            return false;
        }
        var unhandledTypes = new HashSet<Type>();
        foreach (var type in options.ExposedTypes)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => !m.IsSpecialName && m.DeclaringType == type);
            foreach (var method in methods)
            {
                CollectTypes(method.ReturnType, unhandledTypes, options);
                foreach (var param in method.GetParameters())
                {
                    CollectTypes(param.ParameterType, unhandledTypes, options);
                }
            }
        }

        var outputDir = Path.Combine(options.OutputPath, "src", "generated");
        Directory.CreateDirectory(outputDir);

        foreach (var type in options.ExposedTypes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("import * as models from \"./models\";");
            sb.AppendLine();

            sb.AppendLine($"export class {type.Name} {{");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => !m.IsSpecialName && m.DeclaringType == type); // only declared in this type
            foreach (var method in methods)
            {
                var paramList = string.Join(", ", method.GetParameters()
                    .Select(p => $"{p.Name}: {MapType(p.ParameterType, options)}"));
                var paramNames = string.Join(", ", method.GetParameters().Select(p => p.Name));
                var returnType = MapType(method.ReturnType, options);
                sb.AppendLine($"    static {method.Name}({paramList}): Promise<{returnType}> {{");
                var args = string.IsNullOrEmpty(paramNames) ? "" : $", {paramNames}";
                sb.AppendLine($"        return watari_invoke<{returnType}>(\"{type.Name}.{method.Name}\"{args});");
                sb.AppendLine("    }");
            }
            sb.AppendLine("}");

            var fileName = ToCamelCase(type.Name) + ".ts";
            var outputFile = Path.Combine(outputDir, fileName);
            File.WriteAllText(outputFile, sb.ToString());
        }

        var modelsSb = new StringBuilder();
        foreach (var t in unhandledTypes.OrderBy(t => t.Name))
        {
            modelsSb.AppendLine($"export interface {t.Name} {{");
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                modelsSb.AppendLine($"    {prop.Name}: {MapType(prop.PropertyType, options)};");
            }
            modelsSb.AppendLine("}");
            modelsSb.AppendLine();
        }

        var modelsFile = Path.Combine(outputDir, "models.ts");
        File.WriteAllText(modelsFile, modelsSb.ToString());

        File.WriteAllText(Path.Combine(outputDir, ".gitignore"), "*");

        var dtsFile = Path.Combine(outputDir, "watari.d.ts");
        File.WriteAllText(dtsFile, "declare global {\n    function watari_invoke<T>(method: string, ...args: any[]): Promise<T>;\n}\n\nexport {};\n");

        return true;
    }

    private void CollectTypes(Type t, HashSet<Type> collected, TypeGeneratorOptions options)
    {
        if (collected.Contains(t) || IsPrimitive(t)) return;
        if (options.Handlers.TryGetValue(t, out var handler))
        {
            var interfaceType = handler.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeHandler<,>));
            var tsType = interfaceType.GetGenericArguments()[1];
            CollectTypes(tsType, collected, options);
            return;
        }
        collected.Add(t);
        foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            CollectTypes(prop.PropertyType, collected, options);
        }
    }

    private string ToCamelCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToLower(s[0]) + s[1..];
    }

    private bool IsPrimitive(Type t)
    {
        return t.IsPrimitive || t == typeof(string) || t == typeof(decimal) || t == typeof(void);
    }

    private string MapType(Type type, TypeGeneratorOptions options)
    {
        if (options.Handlers.TryGetValue(type, out var temp))
        {
            var interfaceType = temp.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeHandler<,>));
            var tsType = interfaceType.GetGenericArguments()[1];
            return MapType(tsType, options);
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = type.GetGenericArguments()[0];
            return MapType(innerType, options);
        }
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
        // For complex types, return the type name prefixed with models
        return "models." + type.Name;
    }
}
