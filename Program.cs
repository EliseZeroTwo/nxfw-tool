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
            
        }
    }
}
