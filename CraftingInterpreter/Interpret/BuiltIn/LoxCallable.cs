using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.Interpret.Interfaces;
using CraftingInterpreter.TokenModels;
using Environment = CraftingInterpreter.Env.Environment;

namespace CraftingInterpreter.Interpret.BuiltIn;

public class LoxCallable(List<Token> parameters, List<Stmt> body, Environment closure, Token? name = null)
    : ICallable
{
    public int Arity()
    {
        return parameters.Count;
    }

    public LoxCallable(Stmt.Function declaration, Environment closure) :
        this(declaration.Params, declaration.Body,
            closure, declaration.Name)
    {
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new Environment(closure);
        for (var i = 0; i < parameters.Count; i++)
        {
            environment.Define(parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn {name?.Lexeme ?? "Anonymous"}>";
    }
}