namespace CraftingInterpreter.Parsing.Errors;

public class ParseError(string? message = null) : SystemException(message);