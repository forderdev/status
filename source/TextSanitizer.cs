using System.Globalization;
using System.Text;

namespace PlayerStatus
{
    internal static class TextSanitizer
    {
        public static string Clean(string raw, bool allowRichText)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            StringBuilder builder = new StringBuilder(raw.Length);
            bool pendingSpace = false;

            foreach (char character in raw)
            {
                if (char.IsWhiteSpace(character) || char.IsControl(character))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }

                if (character == '\\' || CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.Format)
                    continue;

                if (!allowRichText && (character == '<' || character == '>'))
                    continue;

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                builder.Append(character);
            }

            return builder.ToString();
        }
    }
}
