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
        public static void ExtractAll(string inFirmwareDirectoryPath, string outputDirectoryPath)
        {
            List<string> ncaPathList = new List<string>();
            
            foreach (string dir in Directory.EnumerateDirectories(inFirmwareDirectoryPath, "*.nca"))
            {
                if (File.Exists(dir + "/00.nca"))
                    ncaPathList.Add(dir + "/00.nca");
            }

            foreach (string ncaPath in Directory.EnumerateFiles(inFirmwareDirectoryPath, "*.nca"))
            {
                ncaPathList.Add(ncaPath);
            }

            foreach(string ncaPath in ncaPathList)
            {
                NcaInfo nca;
                using (IStorage inFile = new LocalStorage(ncaPath, FileAccess.Read))
                {
                    nca = new NcaInfo(inFile);
                    nca.Extract(outputDirectoryPath);  
                }
            }
        }
    }
}