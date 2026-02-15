using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Interpret.Interfaces;
using Environment = CraftingInterpreter.Env.Environment;

namespace CraftingInterpreter.Interpret.BuiltInFn;

public class LoxFunction(Stmt.Function declaration) : ICallable
{
    public int Arity()
    {
        return declaration.Params.Count;
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new Environment();
        for (var i = 0; i < declaration.Params.Count; i++)
        {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]);
        }
        
        interpreter.ExecuteBlock(declaration.Body, environment);
        return null;
    }

    public override string ToString()
    {
        return $"<fn {declaration.Name.Lexeme}>";
    }
}