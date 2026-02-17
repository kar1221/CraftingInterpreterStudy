using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.Interpret.Interfaces;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Interpret.BuiltIn;

public class LoxClass(
    string name,
    Dictionary<string, LoxCallable> methods,
    Dictionary<string, LoxCallable> staticMethod)
    : LoxInstance(null), ICallable
{
    public string Name { get; } = name;
    private Dictionary<string, LoxCallable> _staticMethod = staticMethod;

    public override string ToString()
    {
        return Name;
    }

    public LoxCallable? FindMethod(string name)
    {
        return methods.GetValueOrDefault(name);
    }

    private LoxCallable? FindStaticMethod(string name) => _staticMethod.GetValueOrDefault(name);

    public override object? Get(Token name)
    {
        var method = FindStaticMethod(name.Lexeme);

        if (method != null)
            return method.Bind(this);

        try
        {
            return base.Get(name);
        }
        catch (RuntimeError)
        {
            throw new RuntimeError($"Undefined property '${name.Lexeme}' on class {Name}.");
        }
    }

    public int Arity()
    {
        var initializer = FindMethod("init");

        return initializer?.Arity() ?? 0;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new LoxInstance(this);

        var initializer = FindMethod("init");
        if (initializer != null)
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }

        return instance;
    }
}