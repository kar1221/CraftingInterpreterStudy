using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Env;

public class Environment(Environment? enclosing = null)
{
    private readonly Dictionary<string, object?> _values = new();

    public void Define(string name, object? value)
    {
        // Explicitly allow redeclaration in global scope for repl convenience
        // Not sure if this is appropriate, but a small interpreted language like this
        // I think it brings more benefits than harm i guess
        if (enclosing == null)
        {
            _values[name] = value;
            return;
        }

        if (!_values.TryAdd(name, value))
            throw new RuntimeError("Redeclaration of declared variable.");
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        if (enclosing != null)
            return enclosing.Get(name);

        throw new RuntimeError($"Undefined variable {name.Lexeme}.", name);
    }

    public void Assign(Token name, Object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError($"Undefined variable {name.Lexeme}.", name);
    }
}