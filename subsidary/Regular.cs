using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThemeBuilder.subsidary
{
    internal static partial class Regular
    {
        [GeneratedRegex(@"^(--[\w.]+)(?:\s+(set|get))?(?:\s+<([^>]+)>)?$")]
        public static partial Regex CommandParse();

        [GeneratedRegex(@"#{1,}|\*{1,}")]
        public static partial Regex RemoveMarkdownSymbols();

        [GeneratedRegex(@"^#{1,}\s?|^\*{1,}\s?")]
        public static partial Regex RemoveLineSpace();
    }
}
