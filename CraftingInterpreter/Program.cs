// See https://aka.ms/new-console-template for more information

using CraftingInterpreter.LoxConsole;

namespace CraftingInterpreter;

internal static class CraftingInterpreter
{
    public static void Main(string[] args)
    {
        Lox.Run(args);
    }
}