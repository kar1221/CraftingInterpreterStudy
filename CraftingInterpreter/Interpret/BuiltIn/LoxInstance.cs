namespace CraftingInterpreter.Interpret.BuiltIn;

public class LoxInstance(LoxClass @class)
{
    public override string ToString() => $"{@class.Name} instance";
}