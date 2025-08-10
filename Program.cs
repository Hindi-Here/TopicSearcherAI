using System.Text.RegularExpressions;
using ThemeBuilder.command;

namespace ThemeBuilder
{
    public static class Program
    {

        static async Task Main()
        {
            await CommandHandler.DownloadCertificate();
            await CommandHandler.SendRequest();

            ColorizeText.ColorizeOnReleaseConsole();
            string input = string.Empty;

            WelcomeMessage();

            while (input != "Q")
            {
                ColorizeText.MessagePrint(">> ", ColorizeText.colors["System"]);
                input = Console.ReadLine()!;

                if (input == "Q")
                    Environment.Exit(0);

                var match = Regex.Match(input.Trim(), @"^(--[\w.]+)(?:\s+(set|get))?(?:\s+<([^>]+)>)?$");
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

        private static void WelcomeMessage()
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