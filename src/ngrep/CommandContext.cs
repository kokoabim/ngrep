using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.CommandLineUtils;

namespace Kokoabim.NGrep;

internal class CommandContext
{
    public CommandOption File { get; }
    public CommandOption IgnoreCase { get; }
    public CommandArgument? Input { get; internal set; }
    public InputSource InputSource { get; private set; }
    public CommandOption Multiline { get; }
    public CommandArgument? Pattern { get; internal set; }
    public RegexOptions RegexOptions { get; private set; }
    public CommandOption Singleline { get; }

    private StreamReader? _reader;
    private bool _singleline;

    public CommandContext(CommandLineApplication command)
    {
        command.HelpOption("-?|-h|--help");
        File = command.Option("-f|--file", "Input file", CommandOptionType.SingleValue);
        IgnoreCase = command.Option("-i|--ignore-case", "Case-insensitive matching", CommandOptionType.NoValue);
        Multiline = command.Option("-m|--multiline", "Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string", CommandOptionType.NoValue);
        Singleline = command.Option("-s|--singleline", "Changes the meaning of the dot (.) so it matches every character (instead of every character except \\n)", CommandOptionType.NoValue);
    }

    internal void Init()
    {
        InputSource = GetInputSource();

        if (InputSource == InputSource.File)
        {
            _reader = new StreamReader(File.Value());
        }
        else if (InputSource == InputSource.StdIn)
        {
            _reader = new(Console.OpenStandardInput());
        }
        else if (!string.IsNullOrEmpty(Input?.Value))
        {
            _reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(Input.Value)));
        }

        _singleline = Singleline.HasValue();

        RegexOptions = GetRegexOptions();
    }

    internal virtual bool IsValid(out string? message)
    {
        message = null;

        if (string.IsNullOrEmpty(Pattern?.Value))
        {
            message = "Pattern is required";
            return false;
        }
        else if (InputSource == InputSource.Argument && string.IsNullOrEmpty(Input?.Value))
        {
            message = "Input is required";
            return false;
        }
        else if (InputSource == InputSource.File && !System.IO.File.Exists(File.Value()))
        {
            message = $"File not found: {File.Value()}";
            return false;
        }

        return true;
    }

    internal string? ReadInput() => _singleline ? _reader?.ReadToEnd() : _reader?.ReadLine();

    private InputSource GetInputSource() =>
        Console.IsInputRedirected
        ? InputSource.StdIn
        : (File.HasValue() ? InputSource.File : InputSource.Argument);

    private RegexOptions GetRegexOptions()
    {
        RegexOptions regexOptions = RegexOptions.None;
        if (IgnoreCase.HasValue()) regexOptions |= RegexOptions.IgnoreCase;
        if (Multiline.HasValue()) regexOptions |= RegexOptions.Multiline;
        if (Singleline.HasValue()) regexOptions |= RegexOptions.Singleline;
        return regexOptions;
    }
}
