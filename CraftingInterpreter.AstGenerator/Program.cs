// See https://aka.ms/new-console-template for more information

using CraftingInterpreter.AstGenerator.Extensions;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: generate_ast <output_directory>");
    Environment.Exit(64);
}

var outputDirectory = args[0];
DefineAst(outputDirectory, "Expr", [
    "Assign : Token name, Expr? value",
    "Binary : Expr left, Token operator, Expr right",
    "Grouping : Expr expression",
    "Literal : object? value",
    "Logical : Expr left, Token operator, Expr right",
    "Unary : Token operator, Expr right",
    "Ternary : Expr condition, Expr thenBranch, Expr elseBranch",
    "Comma : Expr left, Expr right",
    "Variable : Token name"
]);

DefineAst(outputDirectory, "Stmt", [
    "Block : List<Stmt> statements",
    "Expression : Expr expr",
    "Print : Expr expr",
    "Var : Token name, Expr? initializer",
    "If : Expr condition, Stmt thenBranch, Stmt? elseBranch",
    "While : Expr condition, Stmt body",
    "ForIncrement : Expr incrementExpr",
    "Break :",
    "Continue :"
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
    writer.WriteLine();

    foreach (var type in types)
    {
        var className = type.Split(":")[0].Trim();
        var fields = type.Split(":")[1].Trim();
        DefineType(writer, baseName, className, fields);
    }

    writer.WriteLine($"    public abstract T? Accept<T>(IVisitor<T> visitor);");

    writer.WriteLine("}");
}

static void DefineType(TextWriter writer, string baseName, string className, string fieldList)
{
    var fields = fieldList
        .Split(",", StringSplitOptions.RemoveEmptyEntries)
        .Select(f => f.Trim())
        .Where(f => !string.IsNullOrWhiteSpace(f))
        .ToList();

    var parameters = fields.Select(f =>
    {
        var parts = f.Split(" ");
        return $"{parts[0]} @{parts[1]}";
    }).ToList();

    writer.WriteLine($"    public class {className}({string.Join(", ", parameters)}) : {baseName}");
    writer.WriteLine("    {");

    foreach (var field in fields)
    {
        var parts = field.Split(" ");
        writer.WriteLine($"        public {parts[0]} {parts[1].Capitalize()} {{ get; }} = @{parts[1]};");
    }

    writer.WriteLine();
    writer.WriteLine(
        $"        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.Visit{className}{baseName}(this);");

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
        writer.WriteLine($"        T? Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
    }

    writer.WriteLine("    }");
}