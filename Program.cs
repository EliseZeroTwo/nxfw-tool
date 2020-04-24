using nxfw_tool.Firmware;
using nxfw_tool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Security.Cryptography.X509Certificates;
using LibHac.Fs;
using LibHac.FsSystem;

namespace nxfw_tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Keys.TryLoadKeys();
            using(IStorage ncaStorage = Utils.FirmwareUtils.OpenNcaStorageByTID(args[0], 0x010000000000080e))
            {
                NcaInfo ncaInfo = new NcaInfo(ncaStorage);
                Console.WriteLine(ncaInfo.TitleName);
            }
        
        }
    }
}
