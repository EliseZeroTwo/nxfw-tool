using nxfw_tool.Gui.Cli;

namespace nxfw_tool.Firmware
{
    public class NandBuilder
    {
        public string FirmwareDirectory;
        public string OutputDir;
        
        public NandBuilder(string firmwareDir, string outputDir)
        {
            FirmwareDirectory = firmwareDir;
            OutputDir = outputDir;
        }

        public int BuildAll()
        {
            int res = 0;
            
            FwTui.LoggerWM.Log($"{OutputDir}");

            retn:
            return res;
        }
    }
}