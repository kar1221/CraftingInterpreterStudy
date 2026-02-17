using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.Interpret.Interfaces;
using CraftingInterpreter.TokenModels;
using Environment = CraftingInterpreter.Env.Environment;

namespace CraftingInterpreter.Interpret.BuiltIn;

public class LoxCallable(List<Token> parameters, List<Stmt> body, Environment closure, Token? name = null)
    : ICallable
{
    private bool _isInitializer;
    public bool IsGetter { get; }
    
    public int Arity()
    {
        return IsGetter ? 0 : parameters.Count;
    }

    public LoxCallable(Stmt.Function declaration, Environment closure, bool isInitializer, bool isGetter) :
        this(declaration.Params, declaration.Body,
            closure, declaration.Name)
    {
        this._isInitializer = isInitializer;
        this.IsGetter = isGetter;
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
            if (_isInitializer)
                return closure.GetAt(0, "this");
            
            return returnValue.Value;
        }

        if (_isInitializer)
        {
            return closure.GetAt(0, "this");
        }

        return null;
    }

    public LoxCallable Bind(LoxInstance instance)
    {
        var environment = new Environment(closure);
        environment.Define("this", instance);

        var declaration = new Stmt.Function(name!, parameters, body);

        return new LoxCallable(declaration, environment, _isInitializer , IsGetter);
    }

    public override string ToString()
    {
        return $"<fn {name?.Lexeme ?? "Anonymous"}>";
    }
}