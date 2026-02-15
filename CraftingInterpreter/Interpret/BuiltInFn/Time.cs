using CraftingInterpreter.Interpret.Interfaces;

namespace CraftingInterpreter.Interpret.BuiltInFn;

public class Time : ICallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var now = DateTime.Now;
        return $"{now.Hour}:{now.Minute}:{now.Second}";
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}