﻿using System;
using System.Text;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Vostok.Logging.File.Configuration;
using Vostok.Logging.File.EventsWriting;

namespace Vostok.Logging.File.Tests
{
    [TestFixture]
    internal class FileLogMuxer_Tests
    {
        private IFileSystem slowFileSystem;
        private IEventsWriter slowEventsWriter;
        private FileLogMuxer muxer;
        private IEventsWriterProvider eventsWriterProvider;

        [SetUp]
        public void TestSetup()
        {
            slowFileSystem = Substitute.For<IFileSystem>();
            slowEventsWriter = Substitute.For<IEventsWriter>();
            slowEventsWriter.When(ew => ew.WriteEvents(Arg.Any<LogEventInfo[]>(), Arg.Any<int>())).Do(_ => Thread.Sleep(100));
            slowFileSystem.OpenFile(new FilePath("log").NormalizedPath, Arg.Any<FileOpenMode>(), Arg.Any<Encoding>(), Arg.Any<int>()).Returns(slowEventsWriter);

            eventsWriterProvider = Substitute.For<IEventsWriterProvider>();

            muxer = new FileLogMuxer(100);

            throw new NotImplementedException();
        }

        [Test]
        public void FlushAsync_should_wait_for_events_to_flush()
        {
            var settings = new FileLogSettings {FilePath = "log"};
            var log = new FileLog(settings);

            var @event = CreateLogEvent();


            muxer.TryLog(@event, new FilePath(settings.FilePath), settings, eventsWriterProvider, log, true);

            var task = muxer.FlushAsync(new FilePath(settings.FilePath));
            task.IsCompleted.Should().BeFalse();

            task.Wait(1000);
            task.IsCompleted.Should().BeTrue();

            slowEventsWriter.Received().WriteEvents(Arg.Is<LogEventInfo[]>(events => ReferenceEquals(events[0].Event, @event)), 1);
        }
        
        private static LogEvent CreateLogEvent()
        {
            return new LogEvent(LogLevel.Info, DateTimeOffset.Now, "Hey!");
        }
    }
}