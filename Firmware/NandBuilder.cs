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
            void WriteBct(IFile inputIFile, Stream outputStream)
            {
                byte[] bctBuffer = new byte[0x4000];
                inputIFile.Read(out _, 0, bctBuffer.AsSpan());
                bctBuffer[0x210] = 0x77;
                outputStream.Write(bctBuffer, 0, 0x4000);
            }

            void WritePkg1(IFile inputIFile, Stream outputStream)
            {
                byte[] pkg1Buffer = new byte[0x40000];
                inputIFile.Read(out _, 0, pkg1Buffer.AsSpan());
                outputStream.Write(pkg1Buffer, 0, 0x40000);
            }

            void WritePkg2(IFile inputIFile, Stream outputStream)
            {
                byte[] pkg2Buffer = new byte[0x800000 - 0x4000];
                inputIFile.Read(out _, 0, pkg2Buffer.AsSpan());
                PadStream(outputStream, 0x4000);
                outputStream.Write(pkg2Buffer, 0, 0x800000 - 0x4000);
            }

            void PadStream(Stream stream, long size)
            {
                stream.Position += size - 1;
                stream.WriteByte(0);
            }

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

            
            using (IFileSystem safeIFs = bcpkg23NcaInfo.TryOpenFileSystemSection(NcaSectionType.Data))
            using (IFileSystem normalIFs = bcpkg21NcaInfo.TryOpenFileSystemSection(NcaSectionType.Data))
            {
                normalIFs.Extract(FirmwareDirectory + "/owo");
                if ((safeIFs == null || normalIFs == null) || (!normalIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/bct") || !normalIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package1") || !normalIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package2") || !safeIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/bct") || !safeIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package1") || !safeIFs.FileExists($"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package2")))
                {
                    ThrowError("Invalid firmware! missing bct/pkg1/pkg2");
                    return;
                }

                // BOOT0
                using (FileStream boot0File = File.Create(Path.GetFullPath($"{FirmwareDirectory}/BOOT0.bin")))
                {
                    // Open files
                    normalIFs.OpenFile(out IFile nPkg1IF, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package1".ToU8Span(), OpenMode.Read);
                    normalIFs.OpenFile(out IFile normalBctIFile, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/bct".ToU8Span(), OpenMode.Read);
                    safeIFs.OpenFile(out IFile safeBctIFile, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/bct".ToU8Span(), OpenMode.Read);
                    for (int x = 0; x < 2; x++)
                    {
                        // Write normal bct
                        WriteBct(normalBctIFile, boot0File);

                        // Write safe bct
                        WriteBct(safeBctIFile, boot0File);
                    }

                    //Write padding
                    PadStream(boot0File, 0xF0000);

                    for (int x = 0; x < 2; x++)
                    {
                        nPkg1IF.GetSize(out long nPkg1Size);
                        WritePkg1(nPkg1IF, boot0File);
                    }
                }

                using (FileStream boot1File = File.Create(Path.GetFullPath($"{FirmwareDirectory}/BOOT1.bin")))
                {
                    safeIFs.OpenFile(out IFile safePkg1IFile, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package1".ToU8Span(), OpenMode.Read);
                    for (int x = 0; x < 2; x++)
                        WritePkg1(safePkg1IFile, boot1File);
                }
            
                // BCPKG
                using (FileStream bcpkg21File = File.Create(Path.GetFullPath($"{FirmwareDirectory}/BCPKG2-1.bin")))
                using (FileStream bcpkg22File = File.Create(Path.GetFullPath($"{FirmwareDirectory}/BCPKG2-2.bin")))
                using (FileStream bcpkg23File = File.Create(Path.GetFullPath($"{FirmwareDirectory}/BCPKG2-3.bin")))
                using (FileStream bcpkg24File = File.Create(Path.GetFullPath($"{FirmwareDirectory}/BCPKG2-4.bin")))
                {
                    normalIFs.OpenFile(out IFile normalPkg2IFile, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package2".ToU8Span(), OpenMode.Read);
                    safeIFs.OpenFile(out IFile safePkg2IFile, $"/{fwInfo.VersionInfo.VersionPlatform.ToLower()}/package2".ToU8Span(), OpenMode.Read);

                    WritePkg2(normalPkg2IFile, bcpkg21File);
                    WritePkg2(normalPkg2IFile, bcpkg22File);

                    WritePkg2(safePkg2IFile, bcpkg23File);
                    WritePkg2(safePkg2IFile, bcpkg24File);
                }  
            }
            return;
        }
    }
}