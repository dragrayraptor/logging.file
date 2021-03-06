using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Logging.File.Configuration;
using Vostok.Logging.File.Helpers;
using Vostok.Logging.File.Rolling.Strategies;

namespace Vostok.Logging.File.Tests.Functional
{
    [TestFixture]
    internal class Rolling_Tests : FileLogFunctionalTestsBase
    {
        [Test]
        public void Should_delete_old_files()
        {
            var logName = Folder.GetFileName("log");
        
            FilePath[] oldFiles;
        
            var rollingStrategyOptions = new RollingStrategyOptions
            {
                Type = RollingStrategyType.BySize,
                MaxSize = 1024,
                MaxFiles = 3
            };
        
            var oldMessages = GenerateMessages(0, 20);
            var newMessages = GenerateMessages(21, 120);
        
            using (var log = CreateRollingFileLog(logName, rollingStrategyOptions))
            {
                WriteMessagesWithTimeout(log, oldMessages);
                oldFiles = GetFilesByPrefixOrdered(logName, rollingStrategyOptions);
                WriteMessagesWithTimeout(log, newMessages);
            }
            var newFiles = GetFilesByPrefixOrdered(logName, rollingStrategyOptions);
            
            newFiles.All(filePath => !oldFiles.Contains(filePath)).Should().BeTrue();
            newFiles.Length.Should().Be(3);
        
            ShouldContainMessage(newFiles.Last(), newMessages.Last());
         }
        
        [TestCase(RollingStrategyType.BySize, "")]
        [TestCase(RollingStrategyType.BySize, ".txt")]
        [TestCase(RollingStrategyType.Hybrid, "")]
        [TestCase(RollingStrategyType.Hybrid, ".txt")]
        public void Should_roll_by_size(RollingStrategyType rollingStrategyType, string extension)
        {   
            var logName = Folder.GetFileName("log" + extension);
            
            var rollingStrategyOptions = new RollingStrategyOptions
            {
                Type = rollingStrategyType,
                MaxSize = 1024
            };

            var messages = GenerateMessages(0, 150);
            
            using (var log = CreateRollingFileLog(logName, rollingStrategyOptions))
            {
                WriteMessagesWithTimeout(log, messages, 10);
            }

            var files = GetFilesByPrefixOrdered(logName, rollingStrategyOptions);
            var fileLengths = files.Select(file => new FileInfo(file.NormalizedPath).Length).ToArray();

            files.Length.Should().Be(5);
            fileLengths.Take(4).All(length => 1024 <= length && length <= 2048).Should().BeTrue();
            fileLengths.Last().Should().BeLessThan(2048);
            
            ShouldContainMessage(files.Last(), messages.Last());
        }

        [TestCase(RollingStrategyType.ByTime, "")]
        [TestCase(RollingStrategyType.ByTime, ".txt")]
        [TestCase(RollingStrategyType.Hybrid, "")]
        [TestCase(RollingStrategyType.Hybrid, ".txt")]
        public void Should_roll_by_time(RollingStrategyType rollingStrategyType, string extension)
        {
            var logName = Folder.GetFileName("log" + extension);

            var rollingStrategyOptions = new RollingStrategyOptions
            {
                Type = RollingStrategyType.ByTime,
                Period = RollingPeriod.Second
            };

            using (var log = CreateRollingFileLog(logName, rollingStrategyOptions))
            {
                WriteMessagesWithTimeout(log, GenerateMessages(0, 10), 1);
                Thread.Sleep(1.5.Seconds());
                WriteMessagesWithTimeout(log, GenerateMessages(10, 20), 1);
                Thread.Sleep(1.5.Seconds());
                WriteMessagesWithTimeout(log, GenerateMessages(20, 30), 1);
            }

            var files = GetFilesByPrefixOrdered(logName, rollingStrategyOptions);
            files.Length.Should().BeGreaterOrEqualTo(3);

            ShouldContainMessage(files.Last(), FormatMessage(29));
        }

        [TestCase("")]
        [TestCase(".txt")]
        public void Should_roll_by_size_and_time(string extension)
        {
            var logName = Folder.GetFileName("log" + extension);
            
            var rollingStrategyOptions = new RollingStrategyOptions
            {
                MaxFiles = 0,
                Type = RollingStrategyType.Hybrid,
                Period = RollingPeriod.Second,
                MaxSize = 1024
            };

            FilePath[] firstWriteFiles;
            
            using (var log = CreateRollingFileLog(logName, rollingStrategyOptions))
            {
                WriteMessagesWithTimeout(log, GenerateMessages(0, 100), 1);
                
                log.Flush();
                
                firstWriteFiles = GetFilesByPrefixOrdered(logName, rollingStrategyOptions);
                firstWriteFiles.Length.Should().BeGreaterThan(1);
                
                Thread.Sleep(1.5.Seconds());

                WriteMessagesWithTimeout(log, GenerateMessages(100, 101), 0);
            }

            var secondWriteFiles = GetFilesByPrefixOrdered(logName, rollingStrategyOptions);

            secondWriteFiles.Length.Should().Be(firstWriteFiles.Length + 1);

            ShouldContainMessage(secondWriteFiles.Last(), FormatMessage(100));
        }
        
        [Test]
        public void Should_not_create_empty_files_when_rolled_by_time()
        {
            var logName = Folder.GetFileName("log");
            
            var rollingStrategyOptions = new RollingStrategyOptions
            {
                MaxFiles = 0,
                Type = RollingStrategyType.ByTime,
                Period = RollingPeriod.Second,
                MaxSize = 1024
            };
            
            using (var log = CreateRollingFileLog(logName, rollingStrategyOptions))
            {
                WriteMessagesWithTimeout(log, GenerateMessages(0, 1));
                
                Thread.Sleep(2.5.Seconds());
            }
            
            var files = GetFilesByPrefixOrdered(logName, rollingStrategyOptions);
            files.Length.Should().Be(1);
        }
        
        private static FileLog CreateRollingFileLog(string logName, RollingStrategyOptions options)
        {
            return new FileLog(
                new FileLogSettings
                {
                    FilePath = logName,
                    RollingStrategy = options,
                    FileSettingsUpdateCooldown = TimeSpan.FromMilliseconds(1)
                });
        }

        private static FilePath[] GetFilesByPrefixOrdered(FilePath prefix, RollingStrategyOptions rollingStrategyOptions)
        {
            return new RollingStrategyFactory()
                .CreateStrategy(prefix, rollingStrategyOptions.Type, () => new FileLogSettings
                {
                    RollingStrategy = rollingStrategyOptions
                })
                .DiscoverExistingFiles(prefix)
                .ToArray();
        }
    }
}