// See https://aka.ms/new-console-template for more information

using CraftingInterpreter.AstGenerator.Extensions;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: generate_ast <output_directory>");
    Environment.Exit(64);
}

var outputDirectory = args[0];
DefineAst(outputDirectory, "Expression", [
    "Binary : Expression left, Token operator, Expression right",
    "Grouping : Expression expression",
    "Literal : object? value",
    "Unary : Token operator, Expression right",
]);

return;

static void DefineAst(string outputDir, string baseName, List<string> types)
{
    var path = Path.Combine(outputDir, baseName + ".cs");

    using var writer = File.CreateText(path);

    writer.WriteLine("using CraftingInterpreter.TokenModels;");
    writer.WriteLine();
    writer.WriteLine("namespace CraftingInterpreter.AbstractSyntaxTree;");
    writer.WriteLine();
    writer.WriteLine($"public abstract class {baseName}");
    writer.WriteLine("{");

    DefineVisitor(writer, baseName, types);

    foreach (var type in types)
    {
        var className = type.Split(":")[0].Trim();
        var fields = type.Split(":")[1].Trim();
        DefineType(writer, baseName, className, fields);
    }

    writer.WriteLine($"    public abstract T Accept<T>(IVisitor<T> visitor);");

    writer.WriteLine("}");
}

static void DefineType(TextWriter writer, string baseName, string className, string fieldList)
{
    var fields = fieldList.Split(",");

    var parameters = fields.Select(f => f.Trim().Split(" ")).Select(part =>
    {
        var type = part[0];
        var name = part[1];
        return $"{type} @{name}";
    }).ToList();

    writer.WriteLine($"    public class {className}({string.Join(", ", parameters)}) : {baseName}");
    writer.WriteLine("    {");

    foreach (var field in fields)
    {
        var parts = field.Trim().Split(" ");
        var type = parts[0];
        var name = parts[1];
        writer.WriteLine($"        public {type} {name.Capitalize()} {{ get; }} = @{name};");
    }

    writer.WriteLine();
    writer.WriteLine(
        $"        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit{className}{baseName}(this);");

    writer.WriteLine("    }");
    writer.WriteLine();
}

static void DefineVisitor(TextWriter writer, string baseName, List<string> types)
{
    writer.WriteLine("    public interface IVisitor<out T>");
    writer.WriteLine("    {");

    foreach (var type in types)
    {
        var typeName = type.Split(":")[0].Trim();
        writer.WriteLine($"        T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
    }

    writer.WriteLine("    }");
}