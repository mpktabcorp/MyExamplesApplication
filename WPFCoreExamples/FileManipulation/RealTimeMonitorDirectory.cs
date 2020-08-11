using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WPFCoreExamples.FileManipulation
{
    /// <summary>
    /// Monitor a folder for file changes
    /// </summary>
    public class RealTimeMonitorDirectory : IDisposable
    {
        /// <summary>
        /// File watcher property
        /// </summary>
        private FileSystemWatcher _fileSystemWatcher;

        /// <summary>
        /// Monitor the specified folder or file
        /// </summary>
        /// <param name="path">folder or file to monitor</param>
        private void Monitor(string path)
        {
            _fileSystemWatcher = new FileSystemWatcher { Path = path };
            _fileSystemWatcher.Created += FileSystemWatcher_Created;
            _fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
            _fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// File or folder created event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File created: {0}", e.Name);
        }

        /// <summary>
        /// file or folder renamed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSystemWatcher_Renamed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File renamed: {0}", e.Name);
        }

        /// <summary>
        /// file or folder deleted event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File deleted: {0}", e.Name);
        }

        /// <summary>
        /// file watcher Dispose Method
        /// </summary>
        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }
    }
}
