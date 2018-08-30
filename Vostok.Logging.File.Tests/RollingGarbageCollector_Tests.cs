﻿using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Logging.File.Rolling;

namespace Vostok.Logging.File.Tests
{
    [TestFixture]
    internal class RollingGarbageCollector_Tests
    {
        private RollingGarbageCollector collector;
        private List<string> removedFiles;
        private int filesToKeep;

        [SetUp]
        public void TestSetup()
        {
            filesToKeep = 2;
            removedFiles = new List<string>();

            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.TryRemoveFile(Arg.Do<string>(s => removedFiles.Add(s)));

            collector = new RollingGarbageCollector(fileSystem, () => filesToKeep);
        }

        [Test]
        public void Should_remove_oldest_files()
        {
            collector.RemoveStaleFiles(DiscoverFiles(5));

            removedFiles.Should().Equal("0", "1", "2");
        }

        [Test]
        public void Should_do_nothing_if_there_is_not_enough_files()
        {
            collector.RemoveStaleFiles(DiscoverFiles(1));

            removedFiles.Should().BeEmpty();
        }

        [Test]
        public void Should_do_nothing_if_filesToKeep_is_zero()
        {
            filesToKeep = 0;

            collector.RemoveStaleFiles(DiscoverFiles(1));

            removedFiles.Should().BeEmpty();
        }

        [Test]
        public void Should_do_nothing_if_filesToKeep_is_negative()
        {
            filesToKeep = -10;

            collector.RemoveStaleFiles(DiscoverFiles(1));

            removedFiles.Should().BeEmpty();
        }

        private static string[] DiscoverFiles(int files)
        {
            return Enumerable.Range(0, files).Select(i => i.ToString()).ToArray();
        }
    }
}