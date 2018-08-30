﻿using System.Collections.Generic;
using System.Linq;
using Vostok.Logging.File.Rolling.SuffixFormatters;

namespace Vostok.Logging.File.Rolling
{
    internal static class RollingStrategyHelper
    {
        // TODO(krait): support file extension
        public static IEnumerable<(string path, TSuffix? suffix)> DiscoverExistingFiles<TSuffix>(string basePath, IFileSystem fileSystem, IFileSuffixFormatter<TSuffix> suffixFormatter, IFileNameTuner fileNameTuner)
            where TSuffix : struct
        {
            basePath = fileNameTuner.RemoveExtension(basePath);

            var allFiles = fileSystem.GetFilesByPrefix(basePath);

            var filesWithSuffix = allFiles.Select(path => (path, suffix: suffixFormatter.TryParseSuffix(fileNameTuner.RemoveExtension(path.Substring(basePath.Length)))));

            return filesWithSuffix.Where(file => file.suffix != null).OrderBy(file => file.suffix);
        }
    }
}