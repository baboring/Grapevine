using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Grapevine.Interfaces.Server;

namespace Grapevine.Server
{
    public interface IPublicFolder : IDisposable
    {
        /// <summary>
        /// Gets or sets the default file to return when a directory is requested
        /// </summary>
        string DefaultFileName { get; set; }

        /// <summary>
        /// Gets or sets the optional prefix for specifying when static content should be returned
        /// </summary>
        string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the folder to be scanned for static content requests
        /// </summary>
        string FolderPath { get; }

        /// <summary>
        /// Upload a file to the location specified
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IHttpContext UploadFile(IHttpContext context);

        /// <summary>
        /// Send file
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IHttpContext SendFile(IHttpContext context);
    }

    public class PublicFolder : IPublicFolder
    {
        protected ConcurrentDictionary<string, string> DirectoryList;
        protected const string DefaultFolderName = "public";
        protected FileSystemWatcher Watcher;

        protected static IList<string> ExistingPublicFolders = new List<string>();

        private string _prefix;
        private string _path;

        public PublicFolder() : this(Path.Combine(Directory.GetCurrentDirectory(), DefaultFolderName)) { }

        public PublicFolder(string path)
        {
            Initialize(path);
        }

        private void Initialize(string path)
        {
            FolderPath = Path.GetFullPath(path);
            if (ExistingPublicFolders.Contains(FolderPath)) throw new Exception();
            ExistingPublicFolders.Add(FolderPath);

            Watcher = new FileSystemWatcher
            {
                Path = FolderPath,
                Filter = "*",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
            };

            Watcher.Changed += RefreshDirectoryList;
            Watcher.Created += RefreshDirectoryList;
            Watcher.Deleted += RefreshDirectoryList;
            Watcher.Renamed += RefreshDirectoryList;

            PopulateDirectoryList();

            _prefix = string.Empty;
        }

        public string DefaultFileName { get; set; } = "index.html";

        public string Prefix
        {
            get { return _prefix; }
            set { _prefix = string.IsNullOrWhiteSpace(value) ? string.Empty : $"/{value.Trim().TrimStart('/').TrimEnd('/').Trim()}"; }
        }

        public string FolderPath
        {
            get
            {
                return _path;
            }

            protected internal set
            {
                var path = Path.GetFullPath(value);
                if (!Directory.Exists(path)) path = Directory.CreateDirectory(path).FullName;
                _path = path;
            }
        }

        public IHttpContext UploadFile(IHttpContext context)
        {
            throw new NotImplementedException();
        }

        public IHttpContext SendFile(IHttpContext context)
        {
            throw new NotImplementedException();
        }

        protected void RefreshDirectoryList(object source, EventArgs args)
        {
            // Add or remove items
        }

        protected void PopulateDirectoryList()
        {
            foreach (var item in Directory.GetFiles(FolderPath, "*", SearchOption.AllDirectories).ToList())
            {
                DirectoryList[$"{Prefix}{item.Replace(FolderPath, string.Empty)}"] = item;
            }
        }

        public void Dispose()
        {
            Watcher.Dispose();
        }
    }
}
