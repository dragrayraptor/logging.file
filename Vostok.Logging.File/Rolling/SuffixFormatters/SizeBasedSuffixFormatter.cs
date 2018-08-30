﻿namespace Vostok.Logging.File.Rolling.SuffixFormatters
{
    internal class SizeBasedSuffixFormatter : IFileSuffixFormatter<int>
    {
        public string FormatSuffix(int part) => "." + part;

        public int? TryParseSuffix(string suffix)
        {
            if (suffix.Length < 2 || suffix[0] != '.')
                return null;

            return int.TryParse(suffix.Substring(1), out var part) ? part : null as int?;
        }
    }
}