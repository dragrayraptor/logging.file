﻿namespace Vostok.Logging.File.Configuration
{
    public class RollingStrategyOptions
    {
        public int MaxFiles = 5;

        public RollingStrategyType Type { get; set; } = RollingStrategyType.None;

        public RollingPeriod Period { get; set; } = RollingPeriod.Day;

        public long MaxSize { get; set; } = 100 * 1024 * 1024;

        internal RollingStrategyOptions Clone() => new RollingStrategyOptions
        {
            MaxFiles = MaxFiles,
            Type = Type,
            Period = Period,
            MaxSize = MaxSize
        }; // TODO(krait): test
    }
}