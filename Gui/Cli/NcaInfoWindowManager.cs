using nxfw_tool.Firmware;
using nxfw_tool.Gui.Cli;
using nxfw_tool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Terminal.Gui;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;


namespace nxfw_tool.Gui.Cli
{
    public class NcaInfoWindowManager
    {
        private LoggerWindowManager Logger;
        private Window Window;
        private string Path;
        public FirmwareInfo FwInfo;

        public void ShowNcaInfo()
        {
            Window.RemoveAll();

            string NcaPath = (string)Utils.FirmwareUtils.GetNcaPathFromName(Path, FwTui.NcaNames[FwTui.FirmwareListView.SelectedItem]);
            NcaInfo ncaInfo = new NcaInfo(new LocalStorage(NcaPath, FileAccess.Read));
            
            List<string> NcaInfoLines = new List<string>();

            string FormattedTid = $"{ncaInfo.Nca.Header.TitleId:X16}";
            string FormattedName = $"{ncaInfo.TitleName}";

            NcaInfoLines.Add($"Title ID: {FormattedTid}");

            if (FormattedName != FormattedTid)
                NcaInfoLines.Add($"Title Name: {FormattedName}");
            
            if ((FwInfo.VersionInfo.Major | FwInfo.VersionInfo.Minor | FwInfo.VersionInfo.Micro) != 0)
            {
                NcaInfoLines.Add($"Firmware Version: {FwInfo.VersionInfo.Major}.{FwInfo.VersionInfo.Minor}.{FwInfo.VersionInfo.Micro}");
                NcaInfoLines.Add($"Firmware Description: {FwInfo.VersionInfo.VersionDescription}");
            }

            NcaInfoLines.Add($"Content Type: {ncaInfo.Nca.Header.ContentType}");
            
            string ncaID = NcaPath.Split("/").Last();
            if (ncaID == "00.nca")
                ncaID = NcaPath.Split("/")[NcaPath.Split("/").Length - 2];
            else
                ncaID = ncaID.Replace(".nca", "");
            
            NcaInfoLines.Add($"Nca ID: {ncaID}");

            for (NcaSectionType section = NcaSectionType.Code; section <= NcaSectionType.Logo; section++)
            {
                if (ncaInfo.Nca.SectionExists(section))
                {
                    NcaInfoLines.Add($"\n{section}");
                    try
                    {
                        using (PartitionFileSystem pfs = new PartitionFileSystem(ncaInfo.TryOpenStorageSection(section)))
                        {
                            foreach(DirectoryEntryEx dirEnt in pfs.EnumerateEntries())
                            {
                                NcaInfoLines.Add($"{dirEnt.Name} - {dirEnt.Size} bytes");
                            }
                        }
                    }
                    catch (LibHac.HorizonResultException) { }
                }
            }


            //Actually draw them
            int Y = 1;
            foreach (string content in NcaInfoLines)
            {
                int lines = System.Text.RegularExpressions.Regex.Matches(content, "\n").Count + 1;
                Window.Add(new Label(1, Y, content));
                Y += lines;
            }
        }

        public NcaInfoWindowManager(string path, Window window, LoggerWindowManager logger)
        {
            Path = path;
            Window = window;
            Logger = logger;
            FwInfo = new FirmwareInfo(Path);
            if (FwInfo.SystemVersionNca != null)
            Logger.Log($"Opened Firmware {FwInfo.VersionInfo.VersionString}");
        }

    }
}