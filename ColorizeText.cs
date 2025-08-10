using System.Runtime.InteropServices;

namespace ThemeBuilder
{
    internal static class ColorizeText
    {
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        public static readonly Dictionary<string, string> colors = new()
        {
            { "Warning", "#eb6c73"},
            { "Success", "#6cebab"},
            { "System", "#ffffff"},
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void ColorizeOnReleaseConsole()
        {
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                Console.WriteLine("Не удалось получить режим консоли");
                return;
            }
            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            if (!SetConsoleMode(iStdOut, outConsoleMode))
            {
                Console.WriteLine("Не удалось установить режим консоли");
            }
        }

        public static void MessagePrint(string text, string hex)
        {
            int r = Convert.ToInt32(hex.Substring(1, 2), 16);
            int g = Convert.ToInt32(hex.Substring(3, 2), 16);
            int b = Convert.ToInt32(hex.Substring(5, 2), 16);

            Console.Write($"\u001b[38;2;{r};{g};{b}m{text}\x1b[0m");
        }

    }
}
