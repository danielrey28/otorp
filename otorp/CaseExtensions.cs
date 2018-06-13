using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace otorp
{
    public static class ExtensionMethods
    {
        public static string ToUnderscoreCase(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }

        public static string RemoveFileExtension(this string str)
        {
            return str.Substring(0, str.LastIndexOf('.'));
        }
    }
}
