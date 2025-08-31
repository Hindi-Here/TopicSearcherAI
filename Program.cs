using System.Text.RegularExpressions;
using ThemeBuilder.command;

namespace ThemeBuilder
{
    public static partial class Program
    {
        [GeneratedRegex(@"^(--[\w.]+)(?:\s+(set|get))?(?:\s+<([^>]+)>)?$")]
        private static partial Regex CommandParse();

        private static async Task Main()
        {
            await Start();
            IntroduceMessage();

            while (true)
            {
                ColorizeText.MessagePrint(">> ", ColorizeText.colors["System"]);
                string input = Console.ReadLine()!.Trim();

                if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.Exit(0);
                    break;
                }

                var match = CommandParse().Match(input);
                if (!match.Success)
                {
                    UncorrectMessage();
                    continue;
                }

                string command = match.Groups[1].Value; 
                string? type = match.Groups[2].Success ? match.Groups[2].Value : null;
                string? value = match.Groups[3].Success ? match.Groups[3].Value : null;

                bool isComand = false;
                switch (type)
                {
                    case null when value == null:
                        if (Commands.defaults.TryGetValue(command, out Action? noArgsCommand))
                        {
                            noArgsCommand.Invoke();
                            isComand = true;
                        }

                        break;

                    case "get" when value == null:
                        if (Commands.getters.TryGetValue((command, type), out Action? getterCommand))
                        {
                            getterCommand.Invoke();
                            isComand = true;
                        }

                        break;

                    case "set" when value != null:
                        if (Commands.setters.TryGetValue((command, type), out Action<string>? setterCommand))
                        {
                            setterCommand.Invoke(value!);
                            isComand = true;
                        }

                        break;
                }

                if(!isComand)
                {
                    UncorrectMessage();
                    continue;
                }

            }
        }

        private static async Task Start()
        {
            ColorizeText.ColorizeOnReleaseConsole();

            await CommandHandler.DownloadCertificate();
            await CommandHandler.SendRequest();
        }

        private static void IntroduceMessage()
        {
            ColorizeText.MessagePrint("[SYS] Программа для генерации отчетов по учебной практике 2024/2025\n" +
                                      "[SYS] Отчет сохраняются в виде Word файла \"{theme}.doc\" на рабочем столе\n" +
                                      "[SYS] Подробнее о консольных командах можно узнать через: --help \n" +
                                      "[SYS] Для принудительного выхода введите Q\n\n", ColorizeText.colors["System"]);
        }

        private static void UncorrectMessage()
        {
            ColorizeText.MessagePrint("[ERROR] НЕКОРРЕКТНАЯ КОМАНДА ИЛИ СИНТАКСИС\n" +
                                      "\tФормат: --command / --command get / --command set <value>\n" +
                                      "\tПросмотр команд: --help \n", ColorizeText.colors["Warning"]);
        }

    }

}