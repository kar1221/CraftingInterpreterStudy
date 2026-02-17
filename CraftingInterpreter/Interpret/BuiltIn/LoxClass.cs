using CraftingInterpreter.Interpret.Interfaces;

namespace CraftingInterpreter.Interpret.BuiltIn;

public class LoxClass(string name, Dictionary<string, LoxCallable> methods) : ICallable
{
    public string Name { get; } = name;
    
    public override string ToString()
    {
        return Name;
    }

    public LoxCallable? FindMethod(string name)
    {
        return methods.GetValueOrDefault(name);
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