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

namespace nxfw_tool.Firmware
{
    public class NcaInfo
    { 
        public string Path;
        public Nca Nca;
        public NpdmBinary Npdm;

        public NcaInfo(IStorage ncaIStorage)
        {
            Nca = new Nca(Keys.Keyset, ncaIStorage);
            
            using (IFileSystem exefs = TryOpenFileSystemSection(NcaSectionType.Code))
            {
                if (exefs != null && exefs.FileExists("main.npdm"))
                {
                    IFile npdmFile;
                    if (exefs.OpenFile(out npdmFile, "main.npdm".ToU8String(), OpenMode.Read) == Result.Success)
                    {
                        Npdm = new NpdmBinary(npdmFile.AsStream());
                    }
                    else
                    {
                        Npdm = null;
                    }
                } 
            }
        }

        public IStorage TryOpenStorageSection(NcaSectionType section)
        {
            try
            {
                return Nca.OpenStorage(section, IntegrityCheckLevel.ErrorOnInvalid);
            }
            catch (System.ArgumentException)
            {
                return null;
            }
        }

        public IFileSystem TryOpenFileSystemSection(NcaSectionType section)
        {
            try
            {
                return Nca.OpenFileSystem(section, IntegrityCheckLevel.ErrorOnInvalid);
            }
            catch (System.ArgumentException)
            {
                return null;
            }
        }

        public string TitleName
        {
            get
            {
                if (Npdm != null)
                {
                    if (Npdm.TitleName != null)
                        return Npdm.TitleName;
                }
                return $"0{Nca.Header.TitleId:X}";
            }
        }

        public string FormatName
        {
            get
            {
                if(TitleName[0] != '0')
                {
                    return $"{TitleName}_0{Nca.Header.TitleId:X}";
                }
                return $"{Nca.Header.ContentType}_0{Nca.Header.TitleId:X}";
            }
        }

        private List<string> EnumerateDir(IFileSystem inIFs, string path = "/", Span<DirectoryEntry> outDirEnt = new Span<DirectoryEntry>())
        {

            long entryCount = 0;
            List<string> pathList = new List<string>();
            Span<DirectoryEntry> dirSpan = stackalloc DirectoryEntry[0x100];
            Result openRes = inIFs.OpenDirectory(out IDirectory iDir, path.ToU8Span(), OpenDirectoryMode.All);
            if(openRes != Result.Success)
            {
                Console.WriteLine($"Failed to open dir!");
            }
            else
            {
                iDir.Read(out entryCount, dirSpan);
                foreach(DirectoryEntry dirEnt in dirSpan)
                {
                    if (Convert.ToByte(dirEnt.Name[0]) == 0)
                        continue;
                    string subPath = path + System.Text.Encoding.Default.GetString(dirEnt.Name.ToArray());
                    
                    if ((dirEnt.Attributes & NxFileAttributes.Directory) == NxFileAttributes.Directory)
                    {
                        subPath += "/"; 
                        Console.WriteLine($"Dir: {subPath}");
                        List<string> subList = EnumerateDir(inIFs, subPath);
                        
                        foreach(string subIPath in subList)
                        {
                            pathList.Add(subPath + subIPath);
                        }
                    }
                    else
                    {
                        pathList.Add(subPath);
                    }
                }
            }
            
            if (outDirEnt != null)
            {

            }

            return pathList;
        }

        public int Extract(string outDir, bool code=true, bool data=true, bool logo=true)
        {
            
            int res = 0;
            Console.WriteLine($"--- {FormatName} ---");
            for(NcaSectionType section = NcaSectionType.Code; section <= NcaSectionType.Logo; section++)
            {
                
                switch (section)
                {
                case NcaSectionType.Code:
                    if (!code)
                        continue;
                    break;
                case NcaSectionType.Data:
                    if (!data)
                        continue;
                    break;
                case NcaSectionType.Logo:
                    if (!logo)
                        continue;
                    break;
                default:
                    continue;
                }

                using (IFileSystem currentIFs = TryOpenFileSystemSection(section))
                {
                    if (currentIFs != null)
                    {
                        string basePath = $"{outDir}/{FormatName}/{section}/";
                        Console.WriteLine();
                        List<string> dirEntList = EnumerateDir(currentIFs);
                        if (dirEntList.Count <= 0)
                            continue;
                        Directory.CreateDirectory(basePath);
                        foreach(string dirEnt in dirEntList)
                        {
                            string name = dirEnt;
                            string filePath = $"{basePath}{name}".TrimEnd('\x00');
                            
                            currentIFs.OpenFile(out IFile OpenFile, name.ToU8Span(), OpenMode.Read);
                            if (OpenFile != null)
                            {
                                Console.WriteLine($"{filePath}");
                                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                                {
                                    OpenFile.AsStream().CopyTo(fileStream);
                                }
                            }
                        }
                        Console.WriteLine("\n");
                    }
                }
            }
            return res;
        }
    }
}