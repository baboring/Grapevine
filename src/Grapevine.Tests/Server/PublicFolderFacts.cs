using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Grapevine.Server;
using Grapevine.Shared;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Server
{
    public class PublicFolderFacts
    {
        private static readonly Random Random = new Random();
        private static string GenerateUniqueString()
        {
            return Guid.NewGuid().Truncate() + "-" + Random.Next(10,99);
        }

        private static void CleanUp(string folderpath)
        {
            try
            {
                foreach (var file in Directory.GetFiles(folderpath))
                {
                    File.Delete(file);
                }

                Directory.Delete(folderpath);
            }
            catch { /* ignored */ }
        }

        public class Constructors
        {
            [Fact]
            public void DefaultValues()
            {
                var folder = new PublicFolder();
                folder.DefaultFileName.ShouldBe("index.html");
                folder.FolderPath.EndsWith("public").ShouldBeTrue();
                folder.Prefix.Equals(string.Empty).ShouldBeTrue();
                folder.DirectoryListing.Any().ShouldBeFalse();
            }

            // no params
            // absolute path
            // relative path
            // path and prefix
        }

        public class FolderPathProperty
        {
            [Fact]
            public void CreatesFolderIfNotExists()
            {
                var folder = GenerateUniqueString();
                var root = new PublicFolder(folder);
                root.FolderPath.Equals(Path.Combine(Directory.GetCurrentDirectory(), folder)).ShouldBe(true);
                CleanUp(root.FolderPath);
            }
        }

        public class PrefixProperty
        {
            [Fact]
            public void IsEmptyStringWhenSetToNull()
            {
                var folder = new PublicFolder {Prefix = null};
                folder.Prefix.Equals(string.Empty).ShouldBeTrue();
            }

            [Fact]
            public void PrependsMissingForwardSlash()
            {
                var folder = new PublicFolder {Prefix = "hello"};
                folder.Prefix.Equals("hello").ShouldBeFalse();
                folder.Prefix.Equals("/hello").ShouldBeTrue();
            }

            [Fact]
            public void DoesNotPrependForwadSlashWhenExists()
            {
                var folder = new PublicFolder {Prefix = "/hello"};
                folder.Prefix.Equals("/hello").ShouldBeTrue();
            }

            [Fact]
            public void TrimsTrailingSlash()
            {
                var folder = new PublicFolder {Prefix = "hello/"};
                folder.Prefix.Equals("/hello").ShouldBeTrue();

                folder.Prefix = "/hello/";
                folder.Prefix.Equals("/hello").ShouldBeTrue();
            }

            [Fact]
            public void TrimsLeadingAndTrailingWhitespace()
            {
                var folder = new PublicFolder {Prefix = "  /hello/  "};
                folder.Prefix.Equals("/hello").ShouldBeTrue();
            }
        }
    }

    public static class PublicFolderExtensions
    {

    }
}
