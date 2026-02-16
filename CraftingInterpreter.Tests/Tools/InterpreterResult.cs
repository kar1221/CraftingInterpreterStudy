namespace CraftingInterpreter.Tests.Tools;

public record InterpreterResult(
    string Output,
    string? RuntimeError = null,
    bool HadParseError = false
)
{
    public bool Success => !HadParseError && RuntimeError == null;
}