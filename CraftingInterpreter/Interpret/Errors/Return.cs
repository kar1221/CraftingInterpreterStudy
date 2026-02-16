namespace CraftingInterpreter.Interpret.Errors;

public class Return(object? value) : SystemException
{
    public readonly object? Value = value;
}