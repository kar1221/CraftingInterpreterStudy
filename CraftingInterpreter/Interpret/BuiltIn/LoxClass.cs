using CraftingInterpreter.Interpret.Interfaces;

namespace CraftingInterpreter.Interpret.BuiltIn;

public class LoxClass(string name) : ICallable
{
    public string Name { get; } = name;
    
    public override string ToString()
    {
        return Name;
    }

    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new LoxInstance(this);
        return instance;
    }
}