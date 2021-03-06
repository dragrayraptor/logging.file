﻿using System;
using Vostok.Logging.File.Configuration;
using Vostok.Logging.File.Helpers;

namespace Vostok.Logging.File.EventsWriting
{
    internal interface IEventsWriterProviderFactory
    {
        IEventsWriterProvider CreateProvider(FilePath filePath, Func<FileLogSettings> settingsProvider);
    }
}