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
        public static Dictionary<string, string> GetAllNcasAndName(string dirPath)
        {
            Dictionary<string, string> ncaPathList = new Dictionary<string, string>();
            foreach (string dir in Directory.EnumerateDirectories(dirPath, "*.nca"))
            {
                if (File.Exists(dir + "/00.nca"))
                {
                    NcaInfo nca;
                    using (IStorage inFile = new LocalStorage(dir + "/00.nca", FileAccess.Read))
                    {
                        nca = new NcaInfo(inFile);
                        ncaPathList.Add(dir + "/00.nca", nca.TitleName);
                    }
                }
            }

            foreach (string ncaPath in Directory.EnumerateFiles(dirPath, "*.nca"))
            {
                NcaInfo nca;
                using (IStorage inFile = new LocalStorage(ncaPath, FileAccess.Read))
                {
                    nca = new NcaInfo(inFile); 
                    ncaPathList.Add(ncaPath, nca.TitleName);
                }
            }

            return ncaPathList;
        }
        public static List<string> GetAllNcas(string dirPath)
        {
            List<string> ncaPathList = new List<string>();
            foreach (string dir in Directory.EnumerateDirectories(dirPath, "*.nca"))
            {
                if (File.Exists(dir + "/00.nca"))
                    ncaPathList.Add(dir + "/00.nca");
            }

            foreach (string ncaPath in Directory.EnumerateFiles(dirPath, "*.nca"))
            {
                ncaPathList.Add(ncaPath);
            }

            return ncaPathList;
        }
        public static void ExtractAll(string inFirmwareDirectoryPath, string outputDirectoryPath)
        {
            foreach(string ncaPath in GetAllNcas(inFirmwareDirectoryPath))
            {
                NcaInfo nca;
                using (IStorage inFile = new LocalStorage(ncaPath, FileAccess.Read))
                {
                    nca = new NcaInfo(inFile);
                    nca.Extract(outputDirectoryPath);  
                }
            }
        }

        public static Object OpenNcaStorageByTID(string dirPath, ulong titleId, bool shouldReturnPath=false)
        {
            string outPath = null;
            foreach(string ncaPath in GetAllNcas(dirPath))
            {
                NcaInfo nca;
                using (IStorage inFile = new LocalStorage(ncaPath, FileAccess.Read))
                {
                    nca = new NcaInfo(inFile);
                    if (nca.Nca.Header.TitleId == titleId)
                    {
                        outPath = ncaPath;
                        break;
                    }
                }
            }

            if (outPath != null && shouldReturnPath == false)
            {
                return new LocalStorage(outPath, FileAccess.Read);
            }

            return outPath;
        }

        public static Object OpenNcaStorageByTitleName(string dirPath, string titleName, bool shouldReturnPath=false)
        {
            string outPath = null;
            foreach(string ncaPath in GetAllNcas(dirPath))
            {
                NcaInfo nca;
                using (IStorage inFile = new LocalStorage(ncaPath, FileAccess.Read))
                {
                    nca = new NcaInfo(inFile);
                    if (nca.TitleName == titleName)
                    {
                        outPath = ncaPath;
                        break;
                    }
                }
            }

            if (outPath != null && shouldReturnPath == false)
            {
                return new LocalStorage(outPath, FileAccess.Read);
            }

            return outPath;
        }
    }
}