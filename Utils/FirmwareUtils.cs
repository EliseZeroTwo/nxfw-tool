using System;
using System.Collections.Generic;
using System.IO;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using nxfw_tool.Firmware;

namespace nxfw_tool.Utils
{
    public static class FirmwareUtils
    {
        public static string FirmwarePath
        {
            get
            {
                return FirmwarePath;
            }
            set
            {
                FirmwarePath = value;
            }
        }
        public static Dictionary<string, string> GetAllNcaPathsAndNames(string dirPath)
        {
            Dictionary<string, string> ncaPathList = new Dictionary<string, string>();
                
            if (!Directory.Exists(dirPath))
                return ncaPathList;

            foreach (string dir in Directory.EnumerateDirectories(dirPath, "*.nca"))
            {
                if (File.Exists(dir + Path.DirectorySeparatorChar + "00.nca"))
                {
                    NcaInfo nca;
                    using (IStorage inFile = new LocalStorage(dir + Path.DirectorySeparatorChar + "00.nca", FileAccess.Read))
                    {
                        if (inFile != null)
                        {
                            nca = new NcaInfo(inFile);
                            ncaPathList.Add(dir + Path.DirectorySeparatorChar + "00.nca", nca.TitleName);
                        }
                    }
                }
            }

            foreach (string ncaPath in Directory.EnumerateFiles(dirPath, "*.nca"))
            {
                NcaInfo nca;
                using (IStorage inFile = new LocalStorage(ncaPath, FileAccess.Read))
                {
                    if (inFile != null)
                    {
                        nca = new NcaInfo(inFile); 
                        ncaPathList.Add(ncaPath, nca.TitleName);
                    }
                }
            }

            return ncaPathList;
        }
        public static string GetNcaPathFromTID(string dirPath, ulong tid)
        {
            foreach(string path in GetAllNcaPaths(dirPath))
            {
                NcaInfo ncaInfo = new NcaInfo(new LocalStorage(path, FileAccess.Read));
                if (ncaInfo.Nca.Header.TitleId == tid)
                    return path;
            }
            return "";
        }
        public static string GetNcaPathFromName(string dirPath, string name)
        {
            Dictionary<string, string> PathNameMap = GetAllNcaPathsAndNames(dirPath);
            foreach(string path in PathNameMap.Keys)
            {
                if (!System.IO.File.Exists(path))
                    continue;
                
                IStorage ncaStorage = new LocalStorage(path, FileAccess.Read);
                
                NcaInfo ncaInfo = new NcaInfo(ncaStorage);
                if (ncaInfo.TitleName == name)
                    return path;
            }
            return "";
        }
        public static List<string> GetAllNcaPaths(string dirPath)
        {
            List<string> ncaPathList = new List<string>();
            if (!Directory.Exists(dirPath))
                return ncaPathList;
            foreach (string dir in Directory.EnumerateDirectories(dirPath, "*.nca"))
            {
                if (File.Exists(dir + Path.DirectorySeparatorChar + "00.nca"))
                    ncaPathList.Add(dir + Path.DirectorySeparatorChar + "00.nca");
            }

            foreach (string ncaPath in Directory.EnumerateFiles(dirPath, "*.nca"))
            {
                ncaPathList.Add(ncaPath);
            }

            return ncaPathList;
        }
        public static void ExtractAll(string inFirmwareDirectoryPath, string outputDirectoryPath)
        {
            foreach(string ncaPath in GetAllNcaPaths(inFirmwareDirectoryPath))
            {
                NcaInfo nca;
                using (IStorage inFile = new LocalStorage(ncaPath, FileAccess.Read))
                {
                    if (inFile != null)
                    {
                        nca = new NcaInfo(inFile);
                        nca.Extract(outputDirectoryPath);  
                    }
                }
            }
        }
    }
}