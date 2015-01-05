﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Wox.Infrastructure.Storage.UserSettings;

namespace Wox.Plugin.SystemPlugins.Program
{
    internal class FileChangeWatcher
    {
        private static bool isIndexing = false;
        private static List<string> watchedPath = new List<string>(); 

        public static void AddWatch(string path, bool includingSubDirectory = true)
        {
            if (watchedPath.Contains(path)) return;
            if (!Directory.Exists(path))
            {
                Debug.WriteLine(string.Format("FileChangeWatcher: {0} doesn't exist", path),"WoxDebug");
                return;
            }

            watchedPath.Add(path);
            foreach (string fileType in UserSettingStorage.Instance.ProgramSuffixes.Split(';'))
            {
                FileSystemWatcher watcher = new FileSystemWatcher
                {
                    Path = path,
                    IncludeSubdirectories = includingSubDirectory,
                    Filter = string.Format("*.{0}", fileType),
                    EnableRaisingEvents = true
                };
                watcher.Changed += FileChanged;
                watcher.Created += FileChanged;
                watcher.Deleted += FileChanged;
                watcher.Renamed += FileChanged;
            }
        }

        private static void FileChanged(object source, FileSystemEventArgs e)
        {
            if (!isIndexing)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Programs.IndexPrograms();
                    isIndexing = false;
                });
            }
        }
 
    }
}
