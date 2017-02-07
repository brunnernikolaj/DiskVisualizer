﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DiskVisualizer
{
    public class FileSystemExplorer
    {


        public Dictionary<string, FolderInfo> FolderInfoDictionary;
        public event EventHandler<FileExplorerDriveAnalyzeDoneEventArgs> DriveAnalyzeDone;
        public event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;

        //Used for calculating the progress of scaning a HDD
        private double PercentDone = 0;
        private long SizeOfProcessedFiles = 0;
        private long SizeOfDrive;

        private static FileSystemExplorer instance;

        private FileSystemExplorer() { }

        public static FileSystemExplorer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FileSystemExplorer();
                }
                return instance;
            }
        }

        public void ScanDrive(string path, long driveSize)
        {
            SizeOfDrive = driveSize;
            FolderInfoDictionary = new Dictionary<string, FolderInfo>();
            FolderInfoDictionary.Add(path, new FolderInfo { name = path, depth = 1 }); //Add drive folder at 1 depth

            var thread = new BackgroundWorker();
            thread.DoWork += (obj, e) => SearchDirectory(path, ProcessFile);
            thread.RunWorkerCompleted += (obj, e) =>
            {
                //Order FolderInfoDictionary by desending depth so the size of each folder can be added to its parent folder 
                var orderedByDepthList = FolderInfoDictionary.OrderByDescending(x => x.Value.depth).ToDictionary(x => x.Key, y => y.Value);
                foreach (KeyValuePair<string, FolderInfo> Entry in orderedByDepthList)
                {
                    if (!string.IsNullOrEmpty(Entry.Value.parentFolder))
                    {
                        FolderInfoDictionary[Entry.Value.parentFolder].size += Entry.Value.size;
                        FolderInfoDictionary[Entry.Value.parentFolder].FileCount += Entry.Value.FileCount;
                        FolderInfoDictionary[Entry.Value.parentFolder].FolderCount += Entry.Value.FolderCount;
                    }
                }
                PercentDone = 0;
                SizeOfProcessedFiles = 0;
                DriveAnalyzeDone?.Invoke("FileSystemExplorer", new FileExplorerDriveAnalyzeDoneEventArgs(path));
            };
            thread.RunWorkerAsync();
        }

        void ProcessFile(string path)
        {
            FileInfo info = new FileInfo(path);
            FolderInfoDictionary[info.Directory.FullName].size += info.Length;
            FolderInfoDictionary[info.Directory.FullName].FileCount++;
            SizeOfProcessedFiles += info.Length;

            double percentCompleted = (SizeOfProcessedFiles / (double)SizeOfDrive) * 100;

            //Update the UI on the main thread
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                //Every 0.5 percent done raise ProgressUpdated                
                if (percentCompleted > PercentDone + 0.5)
                {
                    PercentDone = (SizeOfProcessedFiles / (double)SizeOfDrive) * 100;
                    ProgressUpdated?.Invoke(null, new ProgressUpdatedEventArgs(PercentDone));

                }
            }));
        }

        //Add folder to folderInfoDictionary
        void AddFolderToDictionary(string path)
        {
            DirectoryInfo dinfo = new DirectoryInfo(path);
            string[] depthSplit = dinfo.FullName.Split('\\'); //Find depth of folder

            long Directorycount = dinfo.GetDirectories().Length;

            FolderInfoDictionary.Add(dinfo.FullName, new FolderInfo
            {
                path = dinfo.FullName,
                name = dinfo.Name,
                depth = depthSplit.Length,
                parentFolder = dinfo.Parent.FullName,
                FolderCount = Directorycount,
                DateCreated = dinfo.CreationTime
            });
        }

        void SearchDirectory(string folder, Action<string> fileAction)
        {
            foreach (string file in Directory.GetFiles(folder))
            {
                fileAction(file);
            }

            foreach (string subDir in Directory.GetDirectories(folder))
            {
                try
                {
                    AddFolderToDictionary(subDir);
                    SearchDirectory(subDir, fileAction);
                }
                catch (Exception ex)
                {
                    //ExceptionLogger.Log(ex, "FileSystem Explorer");
                }
            }
        }

        public void DeleteDirectoryAsync(string path)
        {
            //Delete folder and supfolders on a worker thread
            var thread = new BackgroundWorker();
            thread.DoWork += (obj, e) => DeleteDirectory(path);
            thread.RunWorkerAsync();
        }

        private void DeleteDirectory(string path)
        {
            //Decrease Size of Parent Folders 
            foreach (KeyValuePair<string, FolderInfo> pair in FolderInfoDictionary.FindFromList(path.ParentFoldersToList()))
            {
                pair.Value.size -= FolderInfoDictionary[path].size;
            }

            //if (Directory.Exists(path))
            //{
            //    //Delete all files from the Directory
            //    foreach (string file in Directory.GetFiles(path))
            //    {

            //        File.Delete(file);
            //    }
            //    //Delete all child Directories
            //    foreach (string directory in Directory.GetDirectories(path))
            //    {

            //        DeleteDirectory(directory);
            //    }
            //    //Delete a Directory
            //    Directory.Delete(path);
            //}
        }
    }
}
