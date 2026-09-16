using System;
using System.Collections.Generic;
using System.Globalization;

namespace PlayerStatus
{
    internal static class TextLayout
    {
        public static string Compose(string status, Config config)
        {
            int perLine = Math.Max(4, config.MaxCharsPerLine);
            List<string> lines = Break(status, perLine, Math.Max(1, config.MaxLines));

            int longest = 0;

            foreach (string line in lines)
                longest = Math.Max(longest, VisibleLength(line));

            int sizePercent = longest <= perLine
                ? 100
                : Math.Max(ClampPercent(config.MinTextSize), (int)Math.Round(100.0 * perLine / longest));

            string body = config.TextFormat.Replace("{status}", string.Join("\n", lines));

            return sizePercent >= 100
                ? body
                : "<size=" + sizePercent.ToString(CultureInfo.InvariantCulture) + "%>" + body + "</size>";
        }

        private static List<string> Break(string text, int perLine, int maxLines)
        {
            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int total = VisibleLength(text);
            int wanted = Math.Min(Math.Min(maxLines, words.Length), (total + perLine - 1) / perLine);

            if (wanted <= 1)
                return new List<string> { text };

            int count = words.Length;
            int[] prefix = new int[count + 1];

            for (int i = 0; i < count; i++)
                prefix[i + 1] = prefix[i] + VisibleLength(words[i]);

            int[,] best = new int[wanted + 1, count + 1];
            int[,] cut = new int[wanted + 1, count + 1];

            for (int k = 0; k <= wanted; k++)
            {
                for (int j = 0; j <= count; j++)
                    best[k, j] = int.MaxValue;
            }

            best[0, 0] = 0;

            for (int k = 1; k <= wanted; k++)
            {
                for (int j = k; j <= count; j++)
                {
                    for (int i = k - 1; i < j; i++)
                    {
                        if (best[k - 1, i] == int.MaxValue)
                            continue;

                        int lineLength = prefix[j] - prefix[i] + (j - i - 1);
                        int worst = Math.Max(best[k - 1, i], lineLength);

                        if (worst < best[k, j])
                        {
                            best[k, j] = worst;
                            cut[k, j] = i;
                        }
                    }
                }
            }

            List<string> lines = new List<string>(wanted);
            int end = count;

            for (int k = wanted; k >= 1; k--)
            {
                int start = cut[k, end];
                lines.Insert(0, string.Join(" ", words, start, end - start));
                end = start;
            }

            return lines;
        }

        private static int VisibleLength(string text)
        {
            int length = 0;
            bool inTag = false;

            foreach (char character in text)
            {
                if (character == '<')
                    inTag = true;
                else if (character == '>' && inTag)
                    inTag = false;
                else if (!inTag)
                    length++;
            }

            return length;
        }

        private static int ClampPercent(int percent) => Math.Max(10, Math.Min(100, percent));
    }
}
