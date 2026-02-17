using CraftingInterpreter.Interpret.Interfaces;

namespace CraftingInterpreter.Interpret.BuiltIn;

public class Date : ICallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var now = DateTime.Now;
        return $"{now.Year}/{now.Month}/{now.Day}";
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}