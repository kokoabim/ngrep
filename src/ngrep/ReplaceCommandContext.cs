using Microsoft.Extensions.CommandLineUtils;

namespace Kokoabim.NGrep;

internal class ReplaceCommandContext : CommandContext
{
    public CommandOption Global { get; }
    
    public CommandArgument Replacement { get; }

    public ReplaceCommandContext(CommandLineApplication command) : base(command)
    {
        Global = command.Option("-g|--global", "Replace all occurrences, otherwise only the first", CommandOptionType.NoValue);

        Pattern = command.Argument("pattern", "Regular expression pattern");
        Replacement = command.Argument("replacement", "Replacement string");
        Input = command.Argument("input", "Input data");
    }

    internal override bool IsValid(out string? message)
    {
        if (!base.IsValid(out message))
        {
            return false;
        }
        else if (string.IsNullOrEmpty(Replacement.Value))
        {
            message = "Replacement is required";
            return false;
        }

        return true;
    }
}
