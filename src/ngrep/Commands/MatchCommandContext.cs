using Microsoft.Extensions.CommandLineUtils;

namespace Kokoabim.NGrep;

internal class MatchCommandContext : CommandContext
{
    public CommandOption Index { get; }
    public CommandOption Output { get; }

    public MatchCommandContext(CommandLineApplication command) : base(command)
    {
        Output = command.Option("-o|--output", "Output format", CommandOptionType.SingleValue);
        Index = command.Option("-x|--index", "Print match indexes", CommandOptionType.NoValue);

        Pattern = command.Argument("pattern", "Regular expression pattern");
        Input = command.Argument("input", "Input data");
    }
}
