﻿using JetBrains.Annotations;

namespace Vostok.Logging.File.Configuration
{
    /// <summary>
    /// Specifies how to open an existing log file.
    /// </summary>
    [PublicAPI]
    public enum FileOpenMode
    {
        /// <summary>
        /// Append new text to existing file content.
        /// </summary>
        Append,

        /// <summary>
        /// Re-create existing file from scratch.
        /// </summary>
        Rewrite
    }
}