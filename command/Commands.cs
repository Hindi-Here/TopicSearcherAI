using View = (string command, string? type);
using static ThemeBuilder.command.CommandHandler;

namespace ThemeBuilder.command
{
    internal static class Commands
    {
        const string S = "set";
        const string G = "get";

        private static void AsyncToSyn(Func<Task> func) => func().GetAwaiter().GetResult();

        public static readonly Dictionary<string, Action> defaults = new()
        {
            { "--help", GetHelpList },
            { "--detail", GetDetailList },
            { "--list", GetList },
            { "--build", () => AsyncToSyn(() => Build()) },
            { "--default", Default },
        };

        public static readonly Dictionary<View, Action> getters = new()
        {
            { ("--theme", G), GetTheme },
            { ("--take", G), GetTake },

            { ("--model", G), GetModel },
            { ("--temperature", G), GetTemperature },
            { ("--topP", G), GetTopP },
            { ("--maxTokens", G), GetMaxTokens },

            { ("--doc.format", G), GetFormat },
            { ("--doc.margin", G), GetMargin },
            { ("--doc.aligment", G), GetAligment },
            { ("--doc.firstLine", G), GetFirstLine },
            { ("--doc.spacing", G), GetSpacing },
            { ("--doc.family", G), GetFamily },
            { ("--doc.size", G), GetSize }
        };

        public static readonly Dictionary<View, Action<string>> setters = new()
        {
            { ("--key", S), (_) => AsyncToSyn(() => SetKey(_)) },
            { ("--theme", S), (_) => AsyncToSyn(() => SetTheme(_)) },
            { ("--take", S), SetTake },

            { ("--model", S), (_) => AsyncToSyn(() => SetModel(_)) },
            { ("--maxTokens", S), SetMaxTokens },
            { ("--temperature", S), SetTemperature },
            { ("--topP", S), SetTopP },

            { ("--doc.format", S), SetFormat },
            { ("--doc.margin", S), SetMargin },
            { ("--doc.aligment", S), SetAligment },
            { ("--doc.firstLine", S), SetFirstLine },
            { ("--doc.spacing", S), SetSpacing },
            { ("--doc.family", S), SetFamily },
            { ("--doc.size", S), SetSize },
        };

    }
}
