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
            string sysVerNcaPath = FirmwareUtils.GetNcaPathFromTID(Directory, 0x0100000000000809);
            if (sysVerNcaPath != "")
            {
                IStorage sysVerNcaStorage = new LocalStorage(sysVerNcaPath, FileAccess.Read);
                if (sysVerNcaStorage == null)
                    return;
                SystemVersionNca = new NcaInfo(sysVerNcaStorage);


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
}