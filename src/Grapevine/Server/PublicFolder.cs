using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grapevine.Exceptions.Server;
using Grapevine.Interfaces.Server;
using FileNotFoundException = Grapevine.Exceptions.Server.FileNotFoundException;

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
        //void UploadFile(IHttpContext context);

        /// <summary>
        /// Send file
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        void SendFile(IHttpContext context);
    }

    public class PublicFolder : IPublicFolder
    {
        protected ConcurrentDictionary<string, string> DirectoryList = new ConcurrentDictionary<string, string>();
        protected const string DefaultFolderName = "public";
        protected FileSystemWatcher Watcher;

        protected static IList<string> ExistingPublicFolders = new List<string>();

        private string _prefix;
        private string _path;

        public PublicFolder() : this(Path.Combine(Directory.GetCurrentDirectory(), DefaultFolderName)) { }

        public PublicFolder(string path) : this(path, string.Empty) { }

        public PublicFolder(string path, string prefix)
        {
            FolderPath = Path.GetFullPath(path);
            if (ExistingPublicFolders.Contains(FolderPath)) throw new Exception();
            ExistingPublicFolders.Add(FolderPath);

            _prefix = prefix;

            Watcher = new FileSystemWatcher
            {
                Path = FolderPath,
                Filter = "*",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
            };

            Watcher.Created += UpdateDirectoryList;
            Watcher.Deleted += UpdateDirectoryList;
            Watcher.Renamed += UpdateDirectoryList;

            PopulateDirectoryList();
        }

        public string DefaultFileName { get; set; } = "index.html";

        public string Prefix
        {
            get { return _prefix; }
            set
            {
                var prefix = string.IsNullOrWhiteSpace(value) ? string.Empty : $"/{value.Trim().TrimStart('/').TrimEnd('/').Trim()}";
                if (prefix.Equals(_prefix)) return;

                _prefix = prefix;
                PopulateDirectoryList();
            }
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

        //public void UploadFile(IHttpContext context)
        //{
        //    throw new NotImplementedException();
        //}

        public void SendFile(IHttpContext context)
        {
            if (DirectoryList.ContainsKey(context.Request.PathInfo))
            {
                context.Response.SendResponse(DirectoryList[context.Request.PathInfo], true);
            }

            if (Prefix != null && context.Request.PathInfo.StartsWith(Prefix) && !context.WasRespondedTo)
            {
                throw new FileNotFoundException(context);
            }
        }

        protected void UpdateDirectoryList(object source, FileSystemEventArgs args)
        {
            switch (args.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    AddToDirectoryList(args.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    RemoveFromDirectoryList(args.FullPath);
                    break;
                case WatcherChangeTypes.Renamed:
                    AddToDirectoryList(args.FullPath);
                    RemoveFromDirectoryList((args as RenamedEventArgs)?.OldFullPath);
                    break;
            }

            foreach (var key in DirectoryList.Keys)
            {
                Console.WriteLine(key);
            }
        }

        protected void PopulateDirectoryList()
        {
            DirectoryList.Clear();
            foreach (var item in Directory.GetFiles(FolderPath, "*", SearchOption.AllDirectories).ToList())
            {
                AddToDirectoryList(item);
            }
        }

        protected void AddToDirectoryList(string item)
        {
            if (item != null) DirectoryList[CreateDirectoryListKey(item)] = item;
        }

        protected void RemoveFromDirectoryList(string item)
        {
            if (item == null) return;
            var key = CreateDirectoryListKey(item);
            if (DirectoryList.ContainsKey(key)) DirectoryList.TryRemove(key, out key);
        }

        protected string CreateDirectoryListKey(string item)
        {
            return $"{Prefix}{item.Replace(FolderPath, string.Empty)}";
        }

        public void Dispose()
        {
            Watcher.Dispose();
        }
    }
}