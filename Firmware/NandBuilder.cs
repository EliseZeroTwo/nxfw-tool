using System.Collections.Generic;
using System.IO;
using System;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using nxfw_tool.Gui.Cli;
namespace nxfw_tool.Firmware
{
    public class NandBuilder
    {
        public const ulong tidStdBCPKG21 = 0x0100000000000819;
        public const ulong tidStdBCPKG23 = 0x010000000000081a;
        public const ulong tidExfBCPKG21 = 0x010000000000081b;
        public const ulong tidExfBCPKG23 = 0x010000000000081c;
        public string FirmwareDirectory;
        public string OutputDir;

        public enum PartitionId
        {
            Boot0,
            Boot1,
            BCPKG21,
            BCPKG22,
            BCPKG23,
            BCPKG24,
            BCPKG25,
            BCPKG26,
            Safe,
            System,
            User
        }

        public void ThrowError(string error)
        {
            WarningBox warningBox = new WarningBox(error);
            warningBox.OkButton.Clicked += () => { FwTui.InfoWin.Remove(warningBox); FwTui.ReloadActiveNcas(); };
            FwTui.InfoWin.Add(warningBox);
        }
        public NandBuilder(string firmwareDir, string outputDir)
        {
            FirmwareDirectory = firmwareDir;
            OutputDir = outputDir;
        }

        public void BuildAll(bool exfat=true)
        {
            int res = 0;
            NcaInfo bcpkg21NcaInfo;
            NcaInfo bcpkg23NcaInfo;
            string bcpkg21NcaPath = "";
            string bcpkg23NcaPath = "";

            FirmwareInfo fwInfo = new FirmwareInfo(FirmwareDirectory);

            if (exfat && fwInfo.VersionInfo.Major != 1)
            {
                bcpkg21NcaPath = Utils.FirmwareUtils.GetNcaPathFromTID(FirmwareDirectory, tidExfBCPKG21);
                bcpkg23NcaPath = Utils.FirmwareUtils.GetNcaPathFromTID(FirmwareDirectory, tidExfBCPKG23);
            }
            else
            {
                bcpkg21NcaPath = Utils.FirmwareUtils.GetNcaPathFromTID(FirmwareDirectory, tidStdBCPKG21);
                bcpkg23NcaPath = Utils.FirmwareUtils.GetNcaPathFromTID(FirmwareDirectory, tidStdBCPKG23);
            }

            if (bcpkg21NcaPath != "" && bcpkg23NcaPath != "")
            {
                bcpkg21NcaInfo = new NcaInfo(new LocalStorage(bcpkg21NcaPath, FileAccess.Read));
                bcpkg23NcaInfo = new NcaInfo(new LocalStorage(bcpkg23NcaPath, FileAccess.Read));
            }
            else
            {
                ThrowError("Invalid firmware!");
                return;
            }

            byte[] bctContents;
            byte[] pkg1Contents;
            byte[] pkg2Contents;

            using (IFileSystem safeIFs = bcpkg23NcaInfo.TryOpenFileSystemSection(NcaSectionType.Data))
            using (IFileSystem normalIFs = bcpkg21NcaInfo.TryOpenFileSystemSection(NcaSectionType.Data))
            {
                if ((safeIFs == null && normalIFs == null) || (!normalIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/bct") || !normalIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package1") || !normalIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package2") || !safeIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/bct")))
                {
                    ThrowError("Invalid firmware! missing bct/pkg1/pkg2");
                    return;
                }

                using (FileStream boot0File = new FileStream(Path.GetFullPath($"{FirmwareDirectory}/BOOT0.bin"), FileMode.Create))
                {
                    for (int x = 0; x < 2; x++)
                    {
                        // Open Files
                        normalIFs.OpenFile(out IFile normalBctIFile, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/bct".ToU8Span(), OpenMode.Read);
                        safeIFs.OpenFile(out IFile safeBctIFile, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/bct".ToU8Span(), OpenMode.Read);

                        // Get file sizes
                        normalBctIFile.GetSize(out long normalBctSize);
                        safeBctIFile.GetSize(out long safeBctSize);

                        // Write normal bct
                        byte[] normalBctBuffer = new byte[normalBctSize];
                        normalBctIFile.Read(out long normalBytesRead, 0, normalBctBuffer.AsSpan());
                        normalBctBuffer[0x210] = 0x77;
                        boot0File.Write(normalBctBuffer, 0, (int)normalBctSize);
                        for (int j = 0; j < (0x4000 - normalBctSize); j++)
                            boot0File.WriteByte(0);

                        // Write safe bct
                        byte[] safeBctBuffer = new byte[safeBctSize];
                        safeBctIFile.Read(out long safeBytesRead, 0, safeBctBuffer.AsSpan());
                        safeBctBuffer[0x210] = 0x77;
                        boot0File.Write(safeBctBuffer, 0, (int)safeBctSize);
                        for (int j = 0; j < (0x4000 - safeBctSize); j++)
                            boot0File.WriteByte(0);
                    }

                    for (int j = 0; j < 0xF0000; j++)
                            boot0File.WriteByte(0);

                    for (int x = 0; x < 2; x++)
                    {
                        normalIFs.OpenFile(out IFile nPkg1IF, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package1".ToU8Span(), OpenMode.Read);
                        nPkg1IF.GetSize(out long nPkg1Size);
                        nPkg1IF.AsStream().CopyStream(boot0File, nPkg1Size);
                        for (int j = 0; j < (0x40000 - nPkg1Size); j++)
                            boot0File.WriteByte(0);
                    }
                }
                

            }

            retn:
            return;
        }
    }
}