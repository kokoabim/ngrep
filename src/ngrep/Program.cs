namespace Kokoabim.NGrep;

internal class Program
{
    private static int Main(string[] args)
    {
        IConsoleApp consoleApp = new ConsoleApp();
        return consoleApp.Run(args);
    }
}