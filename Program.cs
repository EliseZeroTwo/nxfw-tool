using nxfw_tool.Firmware;
using nxfw_tool.Gui.Cli;
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
            // Load keys from default locations
            Keys.TryLoadKeys();

            if (args.Length == 0)
                return;
            
            FwTui fwTui = new FwTui()
            {
                FwDir = args[0]
            };
            
            fwTui.Init();
            fwTui.DrawNcaSelection();
        }
    }
}
