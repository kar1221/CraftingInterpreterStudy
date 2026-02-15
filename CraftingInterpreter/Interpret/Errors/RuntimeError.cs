using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Interpret.Errors;

public class RuntimeError(string message, Token? token = null) : SystemException(message)
{
    public Token? Token { get; } = token;
}