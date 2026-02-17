using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Resolution.Errors;

public class ResolutionError(Token token, string message) : Exception(message)
{
    public Token Token { get; } = token;
}