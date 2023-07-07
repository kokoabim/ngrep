using System.Text.Encodings.Web;
using System.Text.Json;
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

                Regex jsonEscapeRx = new(@"\bje\((?<v>(.*?((?<=\\)\))*)*)\)");
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false,
                };

                Regex outputGroupRx = new(@"\$-(?<gi>\d+)");

                while (!string.IsNullOrEmpty(input = context.ReadInput()))
                {
                    bool showIndexes = context.Index.HasValue();

                    MatchCollection matches = rx.Matches(input);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches.Where(m => m.Success).Cast<Match>())
                        {
                            ++matchIndex;
                            if (showIndexes) Console.Write($"{matchIndex}: ");

                            if (context.Output.HasValue())
                            {
                                string outputValue = context.Output.Value();

                                int groupCount = match.Groups.Count;
                                outputGroupRx.Matches(outputValue).ForEach(m =>
                                {
                                    int groupIndex = groupCount - int.Parse(m.Groups["gi"].Value);
                                    if (groupIndex >= 0 && groupIndex <= groupCount) outputValue = outputValue.Replace(m.Value, $"${groupIndex}");
                                },
                                m => m.Success);

                                outputValue = match.Result(outputValue);

                                jsonEscapeRx.Matches(outputValue).ForEach(m =>
                                {
                                    string valueToEscape = m.Groups["v"].Value;
                                    outputValue = outputValue[0..m.Index] + JsonSerializer.Serialize(valueToEscape, jsonSerializerOptions)[1..^1] + outputValue[(m.Index + m.Length)..];
                                },
                                m => m.Success);

                                Console.WriteLine(outputValue);
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
