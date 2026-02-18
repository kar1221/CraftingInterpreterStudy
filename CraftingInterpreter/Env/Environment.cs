using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Env;

public class Environment(Environment? enclosing = null)
{
    private readonly Dictionary<string, object?> _values = [];
    public Environment? Enclosing { get; } = enclosing;

    public void Define(string name, object? value)
    {
        _values[name] = value;
    }

    public object? GetAt(int distance, string name)
    {
        var ancestor = Ancestor(distance);
        if (!ancestor._values.TryGetValue(name, out var value))
        {
            throw new RuntimeError($"Static resolution error: Variable '{name}' not found at depth {distance}.");
        }
        return value;
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)._values[name.Lexeme] = value;
    }

    private Environment Ancestor(int distance)
    {
        var environment = this;

        for (var i = 0; i < distance; i++)
        {
            environment = environment.Enclosing!;
        }

        return environment;
    }

    public object? Get(Token name)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            return _values.GetValueOrDefault(name.Lexeme);
        }

        if (Enclosing != null)
            return Enclosing.Get(name);

        throw new RuntimeError($"Undefined variable {name.Lexeme}.", name);
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError($"Undefined variable {name.Lexeme}.", name);
    }
}
