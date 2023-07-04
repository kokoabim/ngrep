using System.Text.RegularExpressions;
using Microsoft.Extensions.CommandLineUtils;

namespace Kokoabim.NGrep;

internal interface IConsoleApp
{
    int Run(string[] args);
}

internal class ConsoleApp : IConsoleApp
{
    public int Run(string[] args)
    {
        CommandLineApplication app = new()
        {
            ExtendedHelpText = "NGrep is a .NET Global Regular Expression Print CLI utility\nMIT License — Created by Spencer James — https://github.com/kokoabim/ngrep",
            FullName = "NGrep — .NET Global Regular Expression Print",
            Name = "ngrep",
        };

        app.HelpOption("-?|-h|--help");
        AddMatchCommand(app);
        AddReplaceCommand(app);

        if (args.Length == 0)
        {
            app.ShowHelp();
            return 0;
        }

        try { return app.Execute(args); }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); return 1; }
    }

    private static void AddMatchCommand(CommandLineApplication app)
    {
        _ = app.Command("m", command =>
        {
            command.FullName = "NGrep Match";
            command.Description = "Match pattern in input data";
            MatchCommandContext context = new(command);

            command.OnExecute(() =>
            {
                context.Init();
                if (!context.IsValid(out string? message))
                {
                    Console.WriteLine(message);
                    return 1;
                }

                string? input = null;
                int matchIndex = 0;
                Regex rx = new(context.Pattern!.Value, context.RegexOptions);

                while (!string.IsNullOrEmpty(input = context.ReadInput()))
                {
                    bool showIndexes = context.Index.HasValue();

                    MatchCollection matches = rx.Matches(input);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches.Cast<Match>())
                        {
                            ++matchIndex;
                            if (showIndexes) Console.Write($"{matchIndex}: ");

                            if (context.Output.HasValue())
                            {
                                Console.WriteLine(match.Result(context.Output.Value()));
                            }
                            else
                            {
                                Console.WriteLine(match.Value);
                            }
                        }
                    }
                }

                return matchIndex == 0 ? 1 : 0;
            });
        });
    }

    private static void AddReplaceCommand(CommandLineApplication app)
    {
        _ = app.Command("r", command =>
        {
            command.FullName = "NGrep Replace";
            command.Description = "Replace pattern in input data";
            ReplaceCommandContext context = new(command);

            command.OnExecute(() =>
            {
                context.Init();
                if (!context.IsValid(out string? message))
                {
                    Console.WriteLine(message);
                    return 1;
                }

                string? input = null;
                Regex rx = new(context.Pattern!.Value, context.RegexOptions);

                while (!string.IsNullOrEmpty(input = context.ReadInput()))
                {
                    Console.WriteLine(rx.Replace(input, context.Replacement!.Value, context.Global.HasValue() ? -1 : 1));
                }

                return 0;
            });
        });
    }
}
