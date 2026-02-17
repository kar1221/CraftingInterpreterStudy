using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Interpret.BuiltIn;

public class LoxInstance(LoxClass? @class)
{
    private readonly Dictionary<string, object?> _fields = new();
    public override string ToString() => $"{@class?.Name} instance";

    public virtual object? Get(Token name, Interpreter interpreter)
    {
        if (_fields.TryGetValue(name.Lexeme, out var value))
            return value;

        var method = @class?.FindMethod(name.Lexeme);

        if (method != null)
        {
            var bound = method.Bind(this);

            if (method.IsGetter)
                return bound.Call(interpreter, []);


            return bound;
        }

        throw new RuntimeError($"Undefined property {name.Lexeme}.", name);
    }

    public void Set(Token name, object? value)
    {
        _fields[name.Lexeme] = value;
    }
}