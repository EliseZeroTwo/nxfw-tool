using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Npdm;
using nxfw_tool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;


namespace nxfw_tool.Firmware
{
    
    public class FirmwareInfo
    {
        protected string Directory;

        public NcaInfo SystemVersionNca;
        public SystemVersionFile VersionInfo = new SystemVersionFile();
        public FirmwareInfo(string directory)
        {
            Directory = directory;
            SystemVersionNca = new NcaInfo((IStorage)FirmwareUtils.OpenNcaStorageByTID(directory, 0x0100000000000809));
            using (IFileSystem systemRomFS = SystemVersionNca.TryOpenFileSystemSection(NcaSectionType.Data))
            {   
                if (systemRomFS.FileExists("/file") && systemRomFS.OpenFile(out IFile versionFile, "/file".ToU8Span(), OpenMode.Read) == Result.Success)
                {
                    VersionInfo.Read(versionFile.AsStream());
                }
            }
        }


    }
}