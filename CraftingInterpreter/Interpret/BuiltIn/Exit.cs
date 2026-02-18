using CraftingInterpreter.Interpret.Interfaces;

namespace CraftingInterpreter.Interpret.BuiltIn;

public class Exit : ICallable
{
    public int Arity() => 0;

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        Environment.Exit(0);
        return null;
    }
}