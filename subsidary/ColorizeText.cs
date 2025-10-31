using System.Runtime.InteropServices;

namespace ThemeBuilder
{
    internal static partial class ColorizeText
    {
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        public static readonly Dictionary<string, string> colors = new()
        {
            { "Warning", "#eb6c73"},
            { "Success", "#6cebab"},
            { "System", "#ffffff"},
        };

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr GetStdHandle(int nStdHandle);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void ColorizeOnReleaseConsole()
        {
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
                return;
            
            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(iStdOut, outConsoleMode);
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
